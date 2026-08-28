// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Threading;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;
using Replanetizer.Frames;
using Replanetizer.Renderer;
using Replanetizer.Utils;
using static LibReplanetizer.DataFunctions;

namespace Replanetizer.MemoryHook
{
    public class MemoryHookHandle : IDisposable
    {
        const long GUEST_MEMORY_HOST_BASE = 0x300000000;
        const int CAMERA_DATA_SIZE = 0x20;
        const int CUTSCENE_CAMERA_FRAME_SIZE = 0x20;
        const int CUTSCENE_STATE = 2;
        const int MOBY_TABLE_DATA_SIZE = 0x0C;
        const int MOBY_DATA_SIZE = 0x100;

        private readonly IProcessMemory? PROCESS_MEMORY;
        private readonly MemoryAddresses? ADDRESSES;

        private sealed class MemorySnapshot
        {
            public readonly object WRITE_LOCK = new object();
            public readonly Camera camera = new Camera();
            public readonly List<Moby.IngameMobyMemory> mobyMemory =
                new List<Moby.IngameMobyMemory>();
            public int mobyCount;
            public int frameNumber;
            public int readerCount;
        }

        private const int SNAPSHOT_COUNT = 3;
        private readonly MemorySnapshot[] SNAPSHOTS =
        {
            new MemorySnapshot(),
            new MemorySnapshot(),
            new MemorySnapshot()
        };
        private int PUBLISHED_SNAPSHOT_INDEX = -1;
        private int PREVIOUS_SNAPSHOT_INDEX = -1;
        private readonly byte[] CAMERA_DATA_BUFFER = new byte[CAMERA_DATA_SIZE];
        private readonly byte[] CUTSCENE_CAMERA_POINTER_BUFFER = new byte[sizeof(uint)];
        private readonly byte[] CUTSCENE_CAMERA_FRAME_BUFFER = new byte[CUTSCENE_CAMERA_FRAME_SIZE];
        private readonly byte[] MOBY_TABLE_DATA_BUFFER = new byte[MOBY_TABLE_DATA_SIZE];
        private readonly byte[] FRAME_DATA_BUFFER = new byte[sizeof(int)];
        private byte[] MOBY_DATA_BUFFER = Array.Empty<byte>();
        private Thread? SNAPSHOT_THREAD;
        private volatile bool STOP_SNAPSHOT_THREAD;
        private readonly GameType GAME;
        private IFrameWatchpoint? FRAME_WATCHPOINT;

        public bool hookWorking { get; private set; } = false;
        private string errorMessage = "";

        public MemoryHookHandle(Level level, bool useWatchPoint)
        {
            GAME = level.game;
            switch (level.game.num)
            {
                case 1:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0x300A390A0,
                        camera = 0x300951500,
                        gameState = 0x300A10708,
                        cutsceneCamera = 0x30095CDA4,
                        cutsceneCameraFrame = 0x30095CD88,
                        skybox = 0x300A1A79C,
                        planetId = 0x300969C70,
                        levelFrames = 0x300a10710
                    };
                    break;
                case 2:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0x3015927B0,
                        camera = 0x30146E3C0,
                        levelFrames = 0x30156B070
                    };
                    break;
                case 3:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0x300F22260,
                        camera = 0x300D6B400,
                        levelFrames = 0x301A70B30
                    };
                    break;
                default:
                    hookWorking = false;
                    errorMessage = "Memory hooks are not supported for Deadlocked.";
                    return;
            }

            if (IsX64() == false)
            {
                hookWorking = false;
                errorMessage = "Memory hooks are only supported on 64 bit machines.";
                return;
            }

            PROCESS_MEMORY = ProcessMemoryFactory.Create();
            if (PROCESS_MEMORY.IsAvailable)
            {
                hookWorking = true;
                errorMessage = PROCESS_MEMORY.ErrorMessage;

#if _WINDOWS
                if (useWatchPoint && ADDRESSES?.levelFrames != 0 && PROCESS_MEMORY is WindowsProcessMemory windowsProcessMemory)
                {
                    IFrameWatchpoint frameWatchpoint = new WindowsFrameWatchpoint(
                        windowsProcessMemory.ProcessId,
                        ADDRESSES?.levelFrames ?? 0);
                    if (frameWatchpoint.IsAvailable)
                    {
                        FRAME_WATCHPOINT = frameWatchpoint;
                        errorMessage = frameWatchpoint.ErrorMessage;
                    }
                    else
                    {
                        errorMessage = $"{frameWatchpoint.ErrorMessage} Falling back to suspended snapshots.";
                        frameWatchpoint.Dispose();
                    }
                }
#endif
            }
            else
            {
                hookWorking = false;
                errorMessage = PROCESS_MEMORY.ErrorMessage;
                return;
            }

            level.EmplaceCommonData();
            SNAPSHOT_THREAD = new Thread(SnapshotLoop)
            {
                IsBackground = true,
                Name = "RPCS3 memory snapshot"
            };
            SNAPSHOT_THREAD.Start();
        }

        public string GetLastErrorMessage()
        {
            return errorMessage;
        }

        private void UpdateMobys(MemorySnapshot snapshot, Level level, LevelRenderer renderer)
        {
            if (ADDRESSES == null) return;
            if (ADDRESSES.moby == 0) return;

            int numMobs = snapshot.mobyCount;
            while (level.mobs.Count < numMobs)
            {
                Moby mob = new Moby(level.game);
                level.mobs.Add(mob);
                renderer.Include(mob);
            }

            if (numMobs < level.mobs.Count)
            {
                for (int i = numMobs; i < level.mobs.Count; i++)
                {
                    level.mobs[i].SetDead();
                }
            }

            for (int i = 0; i < numMobs; i++)
            {
                level.mobs[i].ApplyMemory(snapshot.mobyMemory[i], level.mobyModels);
            }
        }

        private void UpdateCamera(MemorySnapshot snapshot, Camera? camera)
        {
            if (ADDRESSES == null) return;
            if (ADDRESSES.camera == 0) return;
            if (camera == null) return;

            camera.position = snapshot.camera.position;
            camera.rotation = snapshot.camera.rotation;
        }

        public void UpdateLevelObjects(Level level, LevelRenderer renderer, Camera? camera)
        {
            if (!hookWorking) return;

            if (!TryAcquireCurrentSnapshot(out MemorySnapshot snapshot)) return;
            try
            {
                UpdateMobys(snapshot, level, renderer);
                UpdateCamera(snapshot, camera);
            }
            finally
            {
                ReleaseSnapshot(snapshot);
            }
        }

        public int GetLevelFrameNumber()
        {
            if (!hookWorking) return -1;
            if (ADDRESSES == null) return -1;
            if (ADDRESSES.levelFrames == 0) return -1;

            if (!TryAcquireCurrentSnapshot(out MemorySnapshot snapshot)) return -1;
            try
            {
                return snapshot.frameNumber;
            }
            finally
            {
                ReleaseSnapshot(snapshot);
            }
        }

        private bool TryAcquireCurrentSnapshot(out MemorySnapshot snapshot)
        {
            while (true)
            {
                int index = Volatile.Read(ref PUBLISHED_SNAPSHOT_INDEX);
                if (index < 0)
                {
                    snapshot = null!;
                    return false;
                }

                MemorySnapshot candidate = SNAPSHOTS[index];
                Interlocked.Increment(ref candidate.readerCount);
                if (index == Volatile.Read(ref PUBLISHED_SNAPSHOT_INDEX))
                {
                    snapshot = candidate;
                    return true;
                }

                Interlocked.Decrement(ref candidate.readerCount);
            }
        }

        private static void ReleaseSnapshot(MemorySnapshot snapshot)
        {
            Interlocked.Decrement(ref snapshot.readerCount);
        }

        private void SnapshotLoop()
        {
            try
            {
                while (!STOP_SNAPSHOT_THREAD)
                {
                    bool frameWatchpointStopped = false;
                    if (FRAME_WATCHPOINT != null && ADDRESSES?.levelFrames != 0)
                    {
                        if (!FRAME_WATCHPOINT.WaitForWrite())
                        {
                            if (STOP_SNAPSHOT_THREAD) break;

                            errorMessage = FRAME_WATCHPOINT.ErrorMessage;
                            FRAME_WATCHPOINT.Dispose();
                            FRAME_WATCHPOINT = null;
                            continue;
                        }
                        frameWatchpointStopped = true;
                    }

                    try
                    {
                        CaptureSnapshot(frameWatchpointStopped);
                    }
                    finally
                    {
                        if (frameWatchpointStopped)
                        {
                            if (FRAME_WATCHPOINT != null && !FRAME_WATCHPOINT.RearmAfterWrite())
                            {
                                errorMessage = FRAME_WATCHPOINT.ErrorMessage;
                                FRAME_WATCHPOINT.Dispose();
                                FRAME_WATCHPOINT = null;
                            }
                        }
                    }

                    if (!frameWatchpointStopped)
                    {
                        Thread.SpinWait(32);
                    }
                }
            }
            finally
            {
                if (FRAME_WATCHPOINT != null)
                {
                    FRAME_WATCHPOINT.Dispose();
                    FRAME_WATCHPOINT = null;
                }
            }
        }

        private void CaptureSnapshot(bool processStopped)
        {
            if (ADDRESSES == null) return;

            bool hasFrameCounter = ADDRESSES.levelFrames != 0;
            int frameBefore = -1;
            if (hasFrameCounter && !processStopped)
            {
                if (!ReadProcessInt(ADDRESSES.levelFrames, out frameBefore)) return;

                if (IsCurrentFrame(frameBefore)) return;
            }

            if (!TryGetWritableSnapshot(
                out int snapshotIndex,
                out MemorySnapshot snapshot))
            {
                return;
            }

            bool captureSucceeded;
            try
            {
                captureSucceeded = CaptureIntoSnapshot(
                    snapshot,
                    hasFrameCounter,
                    frameBefore,
                    processStopped);
            }
            finally
            {
                Monitor.Exit(snapshot.WRITE_LOCK);
            }

            if (captureSucceeded)
            {
                int previousIndex = Volatile.Read(ref PUBLISHED_SNAPSHOT_INDEX);
                Volatile.Write(ref PREVIOUS_SNAPSHOT_INDEX, previousIndex);
                Volatile.Write(ref PUBLISHED_SNAPSHOT_INDEX, snapshotIndex);
            }
        }

        private bool IsCurrentFrame(int frameNumber)
        {
            if (!TryAcquireCurrentSnapshot(out MemorySnapshot snapshot)) return false;
            try
            {
                return snapshot.frameNumber == frameNumber;
            }
            finally
            {
                ReleaseSnapshot(snapshot);
            }
        }

        private bool TryGetWritableSnapshot(
            out int snapshotIndex,
            out MemorySnapshot snapshot)
        {
            int publishedIndex = Volatile.Read(ref PUBLISHED_SNAPSHOT_INDEX);
            for (int i = 0; i < SNAPSHOT_COUNT; i++)
            {
                if (i == publishedIndex) continue;

                MemorySnapshot candidate = SNAPSHOTS[i];
                if (Volatile.Read(ref candidate.readerCount) != 0) continue;

                Monitor.Enter(candidate.WRITE_LOCK);
                if (i != Volatile.Read(ref PUBLISHED_SNAPSHOT_INDEX) &&
                    Volatile.Read(ref candidate.readerCount) == 0)
                {
                    snapshotIndex = i;
                    snapshot = candidate;
                    return true;
                }

                Monitor.Exit(candidate.WRITE_LOCK);
            }

            snapshotIndex = -1;
            snapshot = null!;
            return false;
        }

        private bool CaptureIntoSnapshot(
            MemorySnapshot snapshot,
            bool hasFrameCounter,
            int frameBefore,
            bool processStopped)
        {
            if (ADDRESSES == null) return false;

            bool processSuspended = false;
            if (!processStopped)
            {
                if (PROCESS_MEMORY == null || !PROCESS_MEMORY.Suspend()) return false;
                processSuspended = true;
            }

            try
            {
                if (!TryReadCameraTransform(out Vector3 cameraPosition, out Vector3 cameraRotation)) return false;
                if (!ReadProcessBytes(ADDRESSES.moby, MOBY_TABLE_DATA_BUFFER)) return false;

                uint firstMoby = ReadUint(MOBY_TABLE_DATA_BUFFER, 0x00);
                uint lastMoby = ReadUint(MOBY_TABLE_DATA_BUFFER, 0x08);
                if (!TryGetMobyRange(firstMoby, lastMoby, out int mobyCount, out int mobyDataSize))
                {
                    return false;
                }

                if (MOBY_DATA_BUFFER.Length != mobyDataSize)
                {
                    MOBY_DATA_BUFFER = new byte[mobyDataSize];
                }

                long mobyAddress = GUEST_MEMORY_HOST_BASE + firstMoby;
                if (!ReadProcessBytes(mobyAddress, MOBY_DATA_BUFFER)) return false;

                snapshot.camera.position = cameraPosition;
                snapshot.camera.rotation = cameraRotation;

                while (snapshot.mobyMemory.Count < mobyCount)
                {
                    snapshot.mobyMemory.Add(new Moby.IngameMobyMemory());
                }

                for (int i = 0; i < mobyCount; i++)
                {
                    snapshot.mobyMemory[i].LoadFromMemory(
                        GAME,
                        MOBY_DATA_BUFFER,
                        i * MOBY_DATA_SIZE,
                        ReadGuestProcessBytes);
                }

                int frameAfter = frameBefore;
                if (hasFrameCounter && !ReadProcessInt(ADDRESSES.levelFrames, out frameAfter))
                {
                    return false;
                }

                snapshot.mobyCount = mobyCount;
                snapshot.frameNumber = frameAfter;
                return true;
            }
            finally
            {
                if (processSuspended)
                {
                    PROCESS_MEMORY?.Resume();
                }
            }
        }

        private bool ReadProcessBytes(long address, byte[] buffer)
        {
            return PROCESS_MEMORY?.Read(address, buffer) ?? false;
        }

        private bool TryReadCameraTransform(out Vector3 position, out Vector3 rotation)
        {
            position = Vector3.Zero;
            rotation = Vector3.Zero;
            if (ADDRESSES == null) return false;

            if (ADDRESSES.gameState != 0 &&
                ADDRESSES.cutsceneCamera != 0 &&
                ADDRESSES.cutsceneCameraFrame != 0 &&
                ReadProcessInt(ADDRESSES.gameState, out int gameState) &&
                gameState == CUTSCENE_STATE)
            {
                if (!ReadProcessBytes(ADDRESSES.cutsceneCamera, CUTSCENE_CAMERA_POINTER_BUFFER)) return false;
                if (!ReadProcessInt(ADDRESSES.cutsceneCameraFrame, out int frameIndex)) return false;
                if (frameIndex < 0) return false;

                uint cameraAddress = ReadUint(CUTSCENE_CAMERA_POINTER_BUFFER, 0);
                if (cameraAddress == 0) return false;

                long frameAddress = GUEST_MEMORY_HOST_BASE + cameraAddress +
                    (long) frameIndex * CUTSCENE_CAMERA_FRAME_SIZE;
                if (!ReadProcessBytes(frameAddress, CUTSCENE_CAMERA_FRAME_BUFFER)) return false;

                position = new Vector3(
                    ReadFloat(CUTSCENE_CAMERA_FRAME_BUFFER, 0x00),
                    ReadFloat(CUTSCENE_CAMERA_FRAME_BUFFER, 0x04),
                    ReadFloat(CUTSCENE_CAMERA_FRAME_BUFFER, 0x08));
                rotation = new Vector3(
                    ReadFloat(CUTSCENE_CAMERA_FRAME_BUFFER, 0x10) - (float) (Math.PI / 2),
                    ReadFloat(CUTSCENE_CAMERA_FRAME_BUFFER, 0x14),
                    ReadFloat(CUTSCENE_CAMERA_FRAME_BUFFER, 0x18));
                return true;
            }

            if (!ReadProcessBytes(ADDRESSES.camera, CAMERA_DATA_BUFFER)) return false;

            position = new Vector3(
                ReadFloat(CAMERA_DATA_BUFFER, 0x00),
                ReadFloat(CAMERA_DATA_BUFFER, 0x04),
                ReadFloat(CAMERA_DATA_BUFFER, 0x08));
            rotation = new Vector3(
                -ReadFloat(CAMERA_DATA_BUFFER, 0x14),
                ReadFloat(CAMERA_DATA_BUFFER, 0x10),
                ReadFloat(CAMERA_DATA_BUFFER, 0x18) - (float) (Math.PI / 2));
            return true;
        }

        private bool ReadGuestProcessBytes(uint address, byte[] buffer)
        {
            return ReadProcessBytes(GUEST_MEMORY_HOST_BASE + address, buffer);
        }

        private bool ReadProcessInt(long address, out int value)
        {
            bool readSucceeded = ReadProcessBytes(address, FRAME_DATA_BUFFER);
            value = readSucceeded ? ReadInt(FRAME_DATA_BUFFER, 0) : -1;
            return readSucceeded;
        }

        private bool TryGetMobyRange(uint firstMoby, uint lastMoby, out int mobyCount, out int mobyDataSize)
        {
            mobyCount = 0;
            mobyDataSize = 0;
            if (lastMoby < firstMoby) return false;

            ulong mobySpan = (ulong) lastMoby - firstMoby;
            if (mobySpan % MOBY_DATA_SIZE != 0) return false;

            ulong mobyCountValue = mobySpan / MOBY_DATA_SIZE + 1;
            ulong mobyDataSizeValue = mobyCountValue * MOBY_DATA_SIZE;
            if (mobyCountValue > int.MaxValue || mobyDataSizeValue > int.MaxValue) return false;

            mobyCount = (int) mobyCountValue;
            mobyDataSize = (int) mobyDataSizeValue;
            return true;
        }

        private bool IsX64()
        {
            // The memory hook functions depend on reading 64 bit addresses,
            // thus we need to check that the pointer size is 8 (ie 64 bits)
            return IntPtr.Size == 8;
        }

        public void HandleSplineTranslation(Level level, Spline spline, int currentSplineVertex)
        {
            /*
             * This code was already commented out before I moved it here.
             * TODO: Uncomment and test this at some point. Contributions welcomed.
             *
            //write at 0x346BA1180 + 0xC0 + spline.offset + currentSplineVertex * 0x10;
            // List of splines 0x300A51BE0

            byte[] ptrBuff = new byte[0x04];
            int bytesRead = 0;
            ReadProcessMemory(processHandle, 0x300A51BE0 + level.splines.IndexOf(spline) * 0x04, ptrBuff, ptrBuff.Length, ref bytesRead);
            long splinePtr = ReadUint(ptrBuff, 0) + 0x300000010;

            byte[] buff = new byte[0x0C];
            Vector3 vec = spline.GetVertex(currentSplineVertex);
            WriteFloat(buff, 0x00, vec.X);
            WriteFloat(buff, 0x04, vec.Y);
            WriteFloat(buff, 0x08, vec.Z);

            WriteProcessMemory(processHandle, splinePtr + currentSplineVertex * 0x10, buff, buff.Length, ref bytesRead);
            */
        }

        public void Dispose()
        {
            STOP_SNAPSHOT_THREAD = true;
            FRAME_WATCHPOINT?.RequestStop();
            SNAPSHOT_THREAD?.Join();
            PROCESS_MEMORY?.Dispose();
        }
    }
}
