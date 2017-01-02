using System;
using System.Runtime.InteropServices;

// NLog-like api for writing to console
namespace NLog
{
    class ILogger
    {
        public void Trace(string s)
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        }
    }
}

namespace TrackballScroll
{
    /*
     * Callback function which will be called by mouse events.
     * 
     * @author: Martin Seelge
     */
    class MouseHookTrackballScroll : MouseHookBase
    {
        const int X_THRESHOLD = 10;   // threshold in pixels to trigger wheel event
        const int Y_THRESHOLD = 10;   // threshold in pixels to trigger wheel event
        const uint WHEEL_FACTOR = 1; // number of wheel events. The lines scrolled per wheel event are determined by the Microsoft Windows mouse wheel settings.

        public bool preferAxisMovement { get; set; }
        private NLog.ILogger log { get; }

        enum State
        {
            NORMAL = 0, // default state
            DOWN,       // mouse XButton pressed, no movement
            SCROLL,     // mouse XButton pressed + moved
        };

        State _state = State.NORMAL;  // initial state

        // On scaling:
        // _origin contains original screen resolution values as reported by the event message.
        // _originScaled containes scaled positions as required by SetCursorPos/Cursor.Position.
        // Instead of handling different scaling values on multiple monitors, they both positions are stored independantly.
        WinAPI.POINT _origin;               // cursor position when entering state DOWN
        System.Drawing.Point _originScaled; // cursor position when entering state DOWN with scaling
        int _xcount = 0;                    // accumulated horizontal movement while in state SCROLL
        int _ycount = 0;                    // accumulated vertical movement while in state SCROLL

        public MouseHookTrackballScroll()
        {
#if DEBUG
            log = new NLog.ILogger();
#endif
        }

        public override IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode != WinAPI.HC_ACTION)
            {
                return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            bool preventCallNextHookEx = false;
            WinAPI.MSLLHOOKSTRUCT p = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));
#if DEBUG
            var oldState = _state;
#endif
            switch (_state)
            {
                case State.NORMAL:
                    if (WinAPI.MouseMessages.WM_XBUTTONDOWN == (WinAPI.MouseMessages)wParam)
                    { // NORMAL->DOWN: remember position
                        preventCallNextHookEx = true;
                        _state = State.DOWN;
                        _origin = p.pt;
                        _originScaled = System.Windows.Forms.Cursor.Position;
#if DEBUG
                        log.Trace(String.Format("{0}->{1} set origin {2},{3} scaled {4},{5}", oldState, _state, _origin.x, _origin.y, _originScaled.X, _originScaled.Y));
#endif
                    }
                    break;
                case State.DOWN:
                    if (WinAPI.MouseMessages.WM_XBUTTONUP == (WinAPI.MouseMessages)wParam)
                    { // DOWN->NORMAL: middle button click
                        preventCallNextHookEx = true;
                        _state = State.NORMAL;
                        WinAPI.INPUT[] input = new WinAPI.INPUT[2];
                        input[0].type = WinAPI.INPUT_MOUSE;
                        input[0].mi.dx = p.pt.x;
                        input[0].mi.dy = p.pt.y;
                        input[0].mi.mouseData = 0x0;
                        input[0].mi.dwFlags = (uint)WinAPI.MouseEvent.MOUSEEVENTF_MIDDLEDOWN; // middle button down
                        input[0].mi.time = 0x0;
                        input[0].mi.dwExtraInfo = IntPtr.Zero;
                        input[1].type = WinAPI.INPUT_MOUSE;
                        input[1].mi.dx = p.pt.x;
                        input[1].mi.dy = p.pt.y;
                        input[1].mi.mouseData = 0x0;
                        input[1].mi.dwFlags = (uint)WinAPI.MouseEvent.MOUSEEVENTF_MIDDLEUP; // middle button up
                        input[1].mi.time = 0x0;
                        input[1].mi.dwExtraInfo = IntPtr.Zero;
                        NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(WinAPI.INPUT)));
                    }
                    else if (WinAPI.MouseMessages.WM_MOUSEMOVE == (WinAPI.MouseMessages)wParam)
                    { // DOWN->SCROLL
                        preventCallNextHookEx = true;
                        _state = State.SCROLL;
                        _xcount = 0;
                        _ycount = 0;
                        System.Windows.Forms.Cursor.Position = _originScaled;
                    }
                    break;
                case State.SCROLL:
                    if (WinAPI.MouseMessages.WM_XBUTTONUP == (WinAPI.MouseMessages)wParam)
                    { // SCROLL->NORMAL
                        preventCallNextHookEx = true;
                        _state = State.NORMAL;
                    }
                    if (WinAPI.MouseMessages.WM_MOUSEMOVE == (WinAPI.MouseMessages)wParam)
                    { // SCROLL->SCROLL: wheel event
                        preventCallNextHookEx = true;
                        _xcount += p.pt.x - _origin.x;
                        _ycount += p.pt.y - _origin.y;

                        System.Windows.Forms.Cursor.Position = _originScaled;
                        if (_xcount < -X_THRESHOLD || _xcount > X_THRESHOLD)
                        {
                            uint mouseData = (uint)(_xcount > 0 ? +WinAPI.WHEEL_DELTA : -WinAPI.WHEEL_DELTA); // scroll direction
                            _xcount = 0;
                            if(preferAxisMovement)
                            {
                                _ycount = 0;
                            }
                            WinAPI.INPUT[] input = new WinAPI.INPUT[WHEEL_FACTOR];
                            for (uint i = 0; i < WHEEL_FACTOR; ++i)
                            {
                                input[i].type = WinAPI.INPUT_MOUSE;
                                input[i].mi.dx = p.pt.x;
                                input[i].mi.dy = p.pt.y;
                                input[i].mi.mouseData = mouseData;
                                input[i].mi.dwFlags = (uint)WinAPI.MouseEvent.MOUSEEVENTF_HWHEEL; // horizontal wheel
                                input[i].mi.time = 0x0;
                                input[i].mi.dwExtraInfo = IntPtr.Zero;
                            }
                            NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(WinAPI.INPUT)));
                        }

                        if (_ycount < -Y_THRESHOLD || _ycount > Y_THRESHOLD)
                        {
                            uint mouseData = (uint)(_ycount > 0 ? -WinAPI.WHEEL_DELTA : +WinAPI.WHEEL_DELTA); // scroll direction
                            if (preferAxisMovement)
                            {
                                _xcount = 0;
                            }
                            _ycount = 0;
                            WinAPI.INPUT[] input = new WinAPI.INPUT[WHEEL_FACTOR];
                            for (uint i = 0; i < WHEEL_FACTOR; ++i)
                            {
                                input[i].type = WinAPI.INPUT_MOUSE;
                                input[i].mi.dx = p.pt.x;
                                input[i].mi.dy = p.pt.y;
                                input[i].mi.mouseData = mouseData;
                                input[i].mi.dwFlags = (uint)WinAPI.MouseEvent.MOUSEEVENTF_WHEEL; // vertical wheel
                                input[i].mi.time = 0x0;
                                input[i].mi.dwExtraInfo = IntPtr.Zero;
                            }
                            NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(WinAPI.INPUT)));
                        }
                    }
                    break;
            }
            // Return TRUE if handled
            return (preventCallNextHookEx ? (IntPtr)1 : NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam));
        }
    }
}
