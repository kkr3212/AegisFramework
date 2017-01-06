using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;



namespace Aegis.IO
{
    public class GlobalKeyboardListener
    {
        public event EventHandler<GlobalKeyboardListenerEventArgs> KeyboardPressed;

        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private SystemDll.User32.WindowsHookProc _hookProc;


        public GlobalKeyboardListener()
        {
            _windowsHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;
            _hookProc = LowLevelKeyboardProc;

            _user32LibraryHandle = SystemDll.Kernel32.LoadLibrary("User32");
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }



            _windowsHookHandle = SystemDll.User32.SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
            if (_windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
        }


        ~GlobalKeyboardListener()
        {
            Dispose(false);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!SystemDll.User32.UnhookWindowsHookEx(_windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsHookHandle = IntPtr.Zero;
                    _hookProc -= LowLevelKeyboardProc;
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!SystemDll.Kernel32.FreeLibrary(_user32LibraryHandle))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct LowLevelKeyboardInputEvent
        {
            /// <summary>
            /// A virtual-key code. The code must be a value in the range 1 to 254.
            /// </summary>
            public int VirtualCode;

            /// <summary>
            /// A hardware scan code for the key. 
            /// </summary>
            public int HardwareScanCode;

            /// <summary>
            /// The extended-key flag, event-injected Flags, context code, and transition-state flag. This member is specified as follows. An application can use the following values to test the keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a process running at lower integrity level.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The time stamp stamp for this message, equivalent to what GetMessageTime would return for this message.
            /// </summary>
            public int TimeStamp;

            /// <summary>
            /// Additional information associated with the message. 
            /// </summary>
            public IntPtr AdditionalInformation;
        }

        public const int WH_KEYBOARD_LL = 13;
        public const int HC_ACTION = 0;

        public enum KeyboardState
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105
        }

        public const int VkSnapshot = 0x2c;
        public const int VkLwin = 0x5b;
        public const int VkRwin = 0x5c;
        public const int VkTab = 0x09;
        public const int VkEscape = 0x18;
        public const int VkControl = 0x11;
        public const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = (KfAltdown >> 8);

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;

            var wParamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wParamTyped))
            {
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                var eventArguments = new GlobalKeyboardListenerEventArgs(p, (KeyboardState)wParamTyped);
                EventHandler<GlobalKeyboardListenerEventArgs> handler = KeyboardPressed;
                handler?.Invoke(this, eventArguments);

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr)1 : SystemDll.User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }


    public class GlobalKeyboardListenerEventArgs : HandledEventArgs
    {
        public GlobalKeyboardListener.KeyboardState KeyboardState { get; private set; }
        public GlobalKeyboardListener.LowLevelKeyboardInputEvent KeyboardData { get; private set; }


        public GlobalKeyboardListenerEventArgs(
            GlobalKeyboardListener.LowLevelKeyboardInputEvent keyboardData,
            GlobalKeyboardListener.KeyboardState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }
}
