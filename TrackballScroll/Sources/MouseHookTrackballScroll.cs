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
            System.Diagnostics.Debug.WriteLine(s); // Writes to VS output view
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
    class MouseHookTrackballScroll : MouseHookBase, IDisposable
    {
        private NLog.ILogger Log { get; }

        private State State { get; set; }
        private System.Timers.Timer Timer { get; set; }


        public MouseHookTrackballScroll()
        {
            State = new StateNormal();

            Timer = new System.Timers.Timer();
            Timer.Interval = 10;
            Timer.Elapsed += (sender, e) => {
                Timer.Enabled = false;
            };

#if DEBUG
            log = new NLog.ILogger();
#endif
        }

        protected uint SendInput(WinAPI.INPUT[] input)
        {
            return NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(WinAPI.INPUT)));
        }

        public override IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode != WinAPI.HC_ACTION)
            {
                return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            WinAPI.MSLLHOOKSTRUCT llHookStruct = (WinAPI.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(WinAPI.MSLLHOOKSTRUCT));

            var result = State.Process(wParam, llHookStruct, Properties.Settings.Default);

            if (result.Input != null)
            {
                if (!Timer.Enabled)
                {
                    Timer.Start();
                    SendInput(result.Input);
                }                
            }

            State = result.NextState;
            return (result.PreventCallNextHookEx ? (IntPtr)1 : NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam));
        }

        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
