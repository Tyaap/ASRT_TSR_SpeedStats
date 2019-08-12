using System;
using System.Runtime.InteropServices;

namespace SpeedStatsTSR
{
    public static class NativeMethods
    {
        public delegate bool HandlerRountine(CtrlType sig);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern UIntPtr OpenProcess(
        int dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool ReadProcessMemory(
        UIntPtr hProcess,
        UIntPtr lpBaseAddress,
        byte[] lpBuffer,
        UIntPtr dwSize,
        UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern UIntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool EnableMenuItem(UIntPtr hMenu, uint uIdEnableItem, uint uEnable);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern UIntPtr GetSystemMenu(UIntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern UIntPtr RemoveMenu(UIntPtr hMenu, uint nPosition, uint wFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetConsoleCtrlHandler(HandlerRountine handlerRountine, bool add);
    }
}