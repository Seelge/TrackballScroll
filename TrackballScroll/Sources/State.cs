using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TrackballScroll
{
    abstract class State
    {
        public class Result
        {
            public State NextState { get; }
            public bool PreventCallNextHookEx { get; }
            public WinAPI.INPUT[] Input { get; }

            public Result(State nextState)
                : this(nextState, false, null)
            { }

            public Result(State nextState, bool preventCallNextHookEx, WinAPI.INPUT[] input)
            {
                NextState = nextState;
                PreventCallNextHookEx = preventCallNextHookEx;
                Input = input;
            }
        }

        [Pure]
        public abstract Result Process(IntPtr wParam, WinAPI.MSLLHOOKSTRUCT llHookStruct, Properties.Settings settings);

        // Instead of handling different scaling values on multiple monitors, they both position variants are stored independantly.
        public WinAPI.POINT Origin { get; } // Origin contains original screen resolution values as reported by the event message.
        public Point OriginScaled  { get; } // OriginScaled containes scaled positions as required by SetCursorPos/Cursor.Position.

        protected State(WinAPI.POINT origin, Point originScaled)
        {
            Origin = origin;
            OriginScaled = originScaled;
        }

        [Pure]
        protected uint GetXButton(uint llHookStructMouseData)
        {
            return (llHookStructMouseData & 0xFFFF0000) >> 16; // see https://msdn.microsoft.com/de-de/library/windows/desktop/ms644970(v=vs.85).aspx
        }

    }

    class StateNormal : State
    {
        public StateNormal()
            : base(new WinAPI.POINT(), new Point())
        { }

        [Pure]
        public override Result Process(IntPtr wParam, WinAPI.MSLLHOOKSTRUCT llHookStruct, Properties.Settings settings)
        {
            if (WinAPI.MouseMessages.WM_XBUTTONDOWN == (WinAPI.MouseMessages)wParam)
            { // NORMAL->DOWN: remember position
                var xbutton = GetXButton(llHookStruct.mouseData);
                if (!(settings.useX1 && xbutton == 1) && !(settings.useX2 && xbutton == 2))
                {
                    return new Result(this);
                }

                return new Result(new StateDown(llHookStruct.pt, System.Windows.Forms.Cursor.Position), true, null);
            }
            return new Result(this);
        }
    }

    class StateDown : State
    {
        public StateDown(WinAPI.POINT origin, Point originScaled)
            : base(origin, originScaled)
        { }

        [Pure]
        public override Result Process(IntPtr wParam, WinAPI.MSLLHOOKSTRUCT llHookStruct, Properties.Settings settings)
        {
            if (WinAPI.MouseMessages.WM_XBUTTONUP == (WinAPI.MouseMessages)wParam)
            { // DOWN->NORMAL: middle button click
                var xbutton = GetXButton(llHookStruct.mouseData);

                if (!(settings.useX1 && xbutton == 1) && !(settings.useX2 && xbutton == 2))
                {
                    return new Result(this);
                }

                if (!settings.emulateMiddleButton)
                {
                    return new Result(this);
                }

                var input = InputMiddleClick(llHookStruct.pt);
                //SendInput(input);

                return new Result(new StateNormal(), true, input);
            }
            else if (WinAPI.MouseMessages.WM_MOUSEMOVE == (WinAPI.MouseMessages)wParam)
            { // DOWN->SCROLL                
                System.Windows.Forms.Cursor.Position = OriginScaled;

                return new Result(new StateScroll(Origin, OriginScaled, 0, 0), true, null);
            }
            return new Result(this);
        }

        [Pure]
        public WinAPI.INPUT[] InputMiddleClick(WinAPI.POINT pt)
        {
            WinAPI.INPUT[] input = new WinAPI.INPUT[2];
            input[0].type = WinAPI.INPUT_MOUSE;
            input[0].mi.dx = pt.x;
            input[0].mi.dy = pt.y;
            input[0].mi.mouseData = 0x0;
            input[0].mi.dwFlags = (uint)WinAPI.MouseEvent.MOUSEEVENTF_MIDDLEDOWN; // middle button down
            input[0].mi.time = 0x0;
            input[0].mi.dwExtraInfo = IntPtr.Zero;
            input[1].type = WinAPI.INPUT_MOUSE;
            input[1].mi.dx = pt.x;
            input[1].mi.dy = pt.y;
            input[1].mi.mouseData = 0x0;
            input[1].mi.dwFlags = (uint)WinAPI.MouseEvent.MOUSEEVENTF_MIDDLEUP; // middle button up
            input[1].mi.time = 0x0;
            input[1].mi.dwExtraInfo = IntPtr.Zero;
            return input;
        }
    }

    class StateScroll : State
    {
        private static readonly int X_THRESHOLD = 10;   // threshold in pixels to trigger wheel event
        private static readonly int Y_THRESHOLD = 10;   // threshold in pixels to trigger wheel event
        private static readonly uint WHEEL_FACTOR = 1; // number of wheel events. The lines scrolled per wheel event are determined by the Microsoft Windows mouse wheel settings.

        public int Xcount { get; } // accumulated horizontal movement while in state SCROLL
        public int Ycount { get; } // accumulated vertical   movement while in state SCROLL

        public StateScroll(WinAPI.POINT origin, Point originScaled, int xcount, int ycount)
            : base(origin, originScaled)
        {
            Xcount = xcount;
            Ycount = ycount;
        }

        [Pure]
        public override Result Process(IntPtr wParam, WinAPI.MSLLHOOKSTRUCT llHookStruct, Properties.Settings settings)
        {
            if (WinAPI.MouseMessages.WM_XBUTTONUP == (WinAPI.MouseMessages)wParam)
            { // SCROLL->NORMAL
                var xbutton = GetXButton(llHookStruct.mouseData);
                if (!(settings.useX1 && xbutton == 1) && !(settings.useX2 && xbutton == 2))
                {
                    return new Result(this);
                }

                return new Result(new StateNormal(), true, null);
            }

            WinAPI.INPUT[] input = null;

            if (WinAPI.MouseMessages.WM_MOUSEMOVE == (WinAPI.MouseMessages)wParam)
            { // SCROLL->SCROLL: wheel event
                System.Windows.Forms.Cursor.Position = OriginScaled;

                int x = Xcount;
                int y = Ycount;

                if (Xcount < -X_THRESHOLD || Xcount > X_THRESHOLD)
                {
                    uint mouseData = (uint)(Xcount > 0 ? +WinAPI.WHEEL_DELTA : -WinAPI.WHEEL_DELTA); // scroll direction
                    x = 0;
                    if (settings.preferAxis)
                    {
                        y = 0;
                    }
                    input = InputWheel(llHookStruct.pt, mouseData, WinAPI.MouseEvent.MOUSEEVENTF_HWHEEL);
                }

                if (Ycount < -Y_THRESHOLD || Ycount > Y_THRESHOLD)
                {
                    uint mouseData = (uint)(Ycount > 0 ? -WinAPI.WHEEL_DELTA : +WinAPI.WHEEL_DELTA); // scroll direction
                    if (settings.preferAxis)
                    {
                        x = 0;
                    }
                    y = 0;
                    input = InputWheel(llHookStruct.pt, mouseData, WinAPI.MouseEvent.MOUSEEVENTF_WHEEL);
                }

                x += llHookStruct.pt.x - Origin.x;
                y += llHookStruct.pt.y - Origin.y;

                return new Result(new StateScroll(Origin, OriginScaled, x, y), true, input);
            }

            return new Result(this);
        }

        // MOUSEEVENTF_HWHEEL or MOUSEEVENTF_WHEEL
        [Pure]
        private WinAPI.INPUT[] InputWheel(WinAPI.POINT pt, uint mouseData, WinAPI.MouseEvent wheel_type)
        {
            var input = new WinAPI.INPUT[WHEEL_FACTOR];
            for (uint i = 0; i < WHEEL_FACTOR; ++i)
            {
                input[i].type = WinAPI.INPUT_MOUSE;
                input[i].mi.dx = pt.x;
                input[i].mi.dy = pt.y;
                input[i].mi.mouseData = mouseData;
                input[i].mi.dwFlags = (uint)wheel_type;
                input[i].mi.time = 0x0;
                input[i].mi.dwExtraInfo = IntPtr.Zero;
            }
            return input;
        }
    }
}
