// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK.Mathematics;
using Replanetizer.Frames;
using Replanetizer.Utils;
using static LibReplanetizer.DataFunctions;

namespace Replanetizer.MemoryHook
{
    public class MemoryHookHandle
    {
        // Read and write acceess
        const int PROCESS_WM_READ = 0x38;

#if _WINDOWS
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);
#endif

        private readonly Process? PROCESS;
        private readonly IntPtr PROCESS_HANDLE;
        private readonly MemoryAddresses? ADDRESSES;

        public bool hookWorking { get; private set; } = false;
        private string errorMessage = "";

        public MemoryHookHandle(Level level)
        {
#if _WINDOWS
            switch (level.game.num)
            {
                case 1:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0x300A390A0,
                        camera = 0x300951500,
                        levelFrames = 0x300a10710
                    };
                    break;
                case 2:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0x3015927B0,
                        camera = 0x30146E3C0,
                        levelFrames = 0
                    };
                    break;
                case 3:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0,
                        camera = 0x300D6B400,
                        levelFrames = 0
                    };
                    break;
                default:
                    hookWorking = false;
                    errorMessage = "Memory hooks are not supported for Deadlocked.";
                    return;
            }

            Process[] processList = Process.GetProcessesByName("rpcs3");
            if (processList.Length > 0)
            {
                PROCESS = processList[0];
                PROCESS_HANDLE = OpenProcess(PROCESS_WM_READ, false, PROCESS.Id);

                hookWorking = true;
                errorMessage = "Success!";
            }
            else
            {
                hookWorking = false;
                errorMessage = "Failed to find a running RPCS3 process.";
            }

            if (hookWorking)
            {
                level.EmplaceCommonData();
            }
#else
            hookWorking = false;
            errorMessage = "Memory hooks are only supported for Windows.";
#endif
        }

        public string GetLastErrorMessage()
        {
            return errorMessage;
        }

        public void UpdateCamera(Camera camera)
        {
#if _WINDOWS
            if (!hookWorking) return;
            if (ADDRESSES == null) return;
            if (ADDRESSES.camera == 0) return;
            int bytesRead = 0;
            byte[] camBfr = new byte[0x20];
            ReadProcessMemory(PROCESS_HANDLE, ADDRESSES.camera, camBfr, camBfr.Length, ref bytesRead);
            camera.position = new Vector3(ReadFloat(camBfr, 0x00), ReadFloat(camBfr, 0x04), ReadFloat(camBfr, 0x08));
            camera.rotation = new Vector3(-ReadFloat(camBfr, 0x14), ReadFloat(camBfr, 0x10), ReadFloat(camBfr, 0x18) - (float) (Math.PI / 2));
#endif
        }

        public void UpdateMobys(List<Moby> levelMobs, List<Model> models, LevelFrame frame)
        {
#if _WINDOWS
            if (!hookWorking) return;
            if (ADDRESSES == null) return;
            if (ADDRESSES.moby == 0) return;
            if (!IsX64()) return;

            int bytesRead = 0;
            byte[] ptrbuf = new byte[0xC];

            ReadProcessMemory(PROCESS_HANDLE, ADDRESSES.moby, ptrbuf, ptrbuf.Length, ref bytesRead);
            int firstMoby = ReadInt(ptrbuf, 0x00);
            int lastMoby = ReadInt(ptrbuf, 0x08);
            int numMobs = (lastMoby - firstMoby) / 0x100 + 1;

            byte[] mobys = new byte[numMobs * 0x100];

            ReadProcessMemory(PROCESS_HANDLE, 0x300000000 + firstMoby, mobys, mobys.Length, ref bytesRead);

            while (levelMobs.Count < numMobs)
            {
                Moby mob = new Moby();
                levelMobs.Add(mob);
                frame.levelRenderer?.Include(mob);
            }

            if (numMobs < levelMobs.Count)
            {
                for (int i = numMobs; i < levelMobs.Count; i++)
                {
                    levelMobs[i].SetDead();
                }
            }

            for (int i = 0; i < numMobs; i++)
            {
                levelMobs[i].UpdateFromMemory(mobys, i * 0x100, models);
            }
#endif
        }

        public int GetLevelFrameNumber()
        {
#if _WINDOWS
            if (!hookWorking) return -1;
            if (ADDRESSES == null) return -1;
            if (ADDRESSES.levelFrames == 0) return -1;

            int bytesRead = 0;
            byte[] buffer = new byte[0x4];

            ReadProcessMemory(PROCESS_HANDLE, ADDRESSES.levelFrames, buffer, buffer.Length, ref bytesRead);

            return ReadInt(buffer, 0);
#else
            return -1;
#endif
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
    }
}
