﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OnlyV.Helpers.JwLib
{
    internal static class JwLibHelperNativeMethods
    {
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(
            IntPtr hwndParent,
            IntPtr hwndChildAfter,
            string lpszClass,
            string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("User32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("User32.dll")]
        public static extern void GetClassName(IntPtr handle, StringBuilder s, int nMaxCount);

        [DllImport("User32.dll")]
        public static extern void GetWindowText(IntPtr handle, StringBuilder s, int nMaxCount);
    }
}
