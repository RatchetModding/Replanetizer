// Copyright (C) 2018-2021, The Replanetizer Contributors.
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
using Replanetizer.Utils;
using static LibReplanetizer.DataFunctions;

namespace Replanetizer.MemoryHook
{
    public class MemoryHookHandle
    {
        // Read and write acceess
        const int PROCESS_WM_READ = 0x38;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);

        private readonly Process? PROCESS;
        private readonly IntPtr PROCESS_HANDLE;
        private readonly MemoryAddresses? ADDRESSES;

        public bool hookWorking { get; private set; } = false;
        private string errorMessage = "";

        public MemoryHookHandle(GameType game)
        {
            switch (game.num)
            {
                case 1:
                    ADDRESSES = new MemoryAddresses
                    {
                        moby = 0x300A390A0,
                        camera = 0x300951500
                    };
                    break;
                default:
                    hookWorking = false;
                    errorMessage = "Memory hooks are only supported for RaC 1.";
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
        }

        public string GetLastErrorMessage()
        {
            return errorMessage;
        }

        public void UpdateCamera(Camera camera)
        {
            if (!hookWorking) return;
            if (ADDRESSES == null) return;
            int bytesRead = 0;
            byte[] camBfr = new byte[0x20];
            ReadProcessMemory(PROCESS_HANDLE, ADDRESSES.camera, camBfr, camBfr.Length, ref bytesRead);
            camera.position = new Vector3(ReadFloat(camBfr, 0x00), ReadFloat(camBfr, 0x04), ReadFloat(camBfr, 0x08));
            camera.rotation = new Vector3(ReadFloat(camBfr, 0x10), ReadFloat(camBfr, 0x14), ReadFloat(camBfr, 0x18) - (float) (Math.PI / 2));
        }

        public void UpdateMobys(List<Moby> levelMobs, List<Model> models)
        {
            if (!hookWorking) return;
            if (ADDRESSES == null) return;
            if (!IsX64()) return;

            int bytesRead = 0;
            byte[] ptrbuf = new byte[0xC];

            ReadProcessMemory(PROCESS_HANDLE, ADDRESSES.moby, ptrbuf, ptrbuf.Length, ref bytesRead);
            int firstMoby = ReadInt(ptrbuf, 0x00);
            int lastMoby = ReadInt(ptrbuf, 0x08);

            byte[] mobys = new byte[lastMoby - firstMoby + 0x100];

            ReadProcessMemory(PROCESS_HANDLE, 0x300000000 + firstMoby, mobys, mobys.Length, ref bytesRead);

            while (levelMobs.Count < mobys.Length / 0x100)
            {
                levelMobs.Add(new Moby());
            }

            for (int i = 0; i < mobys.Length / 0x100; i++)
            {
                levelMobs[i].UpdateFromMemory(mobys, i * 0x100, models);
            }
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
