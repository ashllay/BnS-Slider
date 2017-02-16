using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BnS_Slider_Mod
{
    internal class MemoryScanner
    {
        public MemoryScanner()
        {
        }

        private static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CompareByteArraySequences(byte[] a, byte[] b, int aStart)
        {
            if (aStart + b.Length > a.Length)
            {
                return false;
            }
            for (int i = 0; i < (int)b.Length; i++)
            {
                if (a[i + aStart] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static IntPtr ScanModule(Memory memory, string moduleName, int baseAddress, byte[] target, int range)
        {
            IntPtr zero = IntPtr.Zero;
            if (memory == null || target == null || string.IsNullOrEmpty(moduleName))
            {
                return IntPtr.Zero;
            }
            ProcessModule processModule = memory.FindModule(moduleName);
            if (processModule == null)
            {
                return IntPtr.Zero;
            }
            IntPtr intPtr = (IntPtr)memory.ReadInt32(processModule.BaseAddress + baseAddress);
            IntPtr intPtr1 = intPtr + range;
            for (int i = intPtr.ToInt32(); i < intPtr1.ToInt32() - (int)target.Length; i++)
            {
                byte[] numArray = new byte[(int)target.Length];
                try
                {
                    memory.ReadMemory((IntPtr)i, numArray, (int)target.Length);
                    if (CompareByteArrays(target, numArray))
                    {
                        zero = (IntPtr)i;
                        return zero;
                    }
                }
                catch (Exception exception)
                {
                }
            }
            return zero;
        }

        public static IntPtr ScanRange(Memory memory, IntPtr startAddress, IntPtr endAddress, byte[] target, byte[] buffer)
        {
            Win32.MEMORY_BASIC_INFORMATION mEMORYBASICINFORMATION;
            IntPtr intPtr;
            IntPtr zero = IntPtr.Zero;
            if (memory == null || target == null || target.Length == 0 || buffer.Length == 0)
            {
                return IntPtr.Zero;
            }
            List<IntPtr> intPtrs = new List<IntPtr>();
            long regionSize = (long)startAddress;
            long num = (long)endAddress;
            while (regionSize < num)
            {
                try
                {
                    if (Win32.VirtualQueryEx(memory.Process.Handle, (IntPtr)regionSize, out mEMORYBASICINFORMATION, 
                        (uint)Marshal.SizeOf(typeof(Win32.MEMORY_BASIC_INFORMATION))) != 0 && 
                        (mEMORYBASICINFORMATION.Protect & 1) == 0 && 
                        (mEMORYBASICINFORMATION.Protect & 256) == 0 && mEMORYBASICINFORMATION.Protect != 0)
                    {
                        long regionSize1 = (long)mEMORYBASICINFORMATION.RegionSize;
                        int num1 = (regionSize1 < buffer.Length ? (int)regionSize1 : buffer.Length);
                        long num2 = regionSize + regionSize1;
                        for (long i = regionSize; i < num2 - target.Length; i = i + num1)
                        {
                            if (i + num1 > num2)
                            {
                                num1 = (int)(num2 - i);
                            }
                            memory.ReadMemory((IntPtr)i, buffer, num1);
                            int num3 = 0;
                            while (num3 < num1 - target.Length)
                            {
                                if (!CompareByteArraySequences(buffer, target, num3))
                                {
                                    num3++;
                                }
                                else
                                {
                                    intPtrs.Add((IntPtr)(i + num3));
                                    intPtr = (IntPtr)(i + num3);
                                    return intPtr;
                                }
                            }
                        }
                    }
                    regionSize = regionSize + (long)mEMORYBASICINFORMATION.RegionSize;
                    continue;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    continue;
                }
                return intPtr;
            }
            if (intPtrs.Count <= 0)
            {
                return IntPtr.Zero;
            }
            return intPtrs[0];
        }

        public IntPtr TestScan(Memory memory, Configuration config)
        {
            return ScanModule(memory, config.Module, config.BaseAddress, config.ByteArray, 65535);
        }
    }
}