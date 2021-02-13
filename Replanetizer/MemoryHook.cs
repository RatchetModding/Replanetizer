using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static LibReplanetizer.DataFunctions;

namespace RatchetEdit
{

    public struct MemoryAdresses
    {
        public long moby;
    }
    class MemoryHook
    {
        // Read and write acceess
        const int PROCESS_WM_READ = 0x38;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);

        private readonly Process process;
        private readonly IntPtr processHandle;
        private readonly MemoryAddresses addresses;

        public bool hookWorking = false;

        public MemoryHook(int gameNum)
        {
            switch (gameNum)
            {
                case 1:
                    addresses = new MemoryAddresses
                    {
                        moby = 0x300A390A0,
                        camera = 0x300951500
                    };
                    break;
                default:
                    break;
            }

            Process[] processList = Process.GetProcessesByName("rpcs3");
            if (processList.Length > 0)
            {
                process = processList[0];
                processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

                hookWorking = true;
            }
        }

        public void UpdateCamera(Camera camera)
        {
            if (!hookWorking) return;
            int bytesRead = 0;
            byte[] camBfr = new byte[0x20];
            ReadProcessMemory(processHandle, addresses.camera, camBfr, camBfr.Length, ref bytesRead);
            camera.position = new Vector3(ReadFloat(camBfr, 0x00), ReadFloat(camBfr, 0x04), ReadFloat(camBfr, 0x08));
            camera.rotation = new Vector3(ReadFloat(camBfr, 0x10), ReadFloat(camBfr, 0x14), ReadFloat(camBfr, 0x18) - (float)(Math.PI / 2));
        }

        public void UpdateMobys(List<Moby> levelMobs, List<Model> models)
        {
            if (!hookWorking) return;
            if (!IsX64()) return;

            int bytesRead = 0;
            byte[] ptrbuf = new byte[0xC];

            ReadProcessMemory(processHandle, addresses.moby, ptrbuf, ptrbuf.Length, ref bytesRead);
            int firstMoby = ReadInt(ptrbuf, 0x00);
            int lastMoby = ReadInt(ptrbuf, 0x08);

            byte[] mobys = new byte[lastMoby - firstMoby + 0x100];

            ReadProcessMemory(processHandle, 0x300000000 + firstMoby, mobys, mobys.Length, ref bytesRead);

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
    }
}
