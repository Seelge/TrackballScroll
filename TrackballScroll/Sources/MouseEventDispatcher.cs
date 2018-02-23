using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace TrackballScroll
{
    class MouseEvent
    {
        public WinAPI.INPUT[] Input { get; }

        public MouseEvent(WinAPI.INPUT[] input)
        {
            Input = input;
        }
    }

    class MouseEventDispatcher : IDisposable
    {
        private ConcurrentQueue<MouseEvent> Queue { get; }
        private System.Timers.Timer Timer { get; set; }

        public MouseEventDispatcher(ConcurrentQueue<MouseEvent> queue)
        {
            Queue = queue;

            Timer = new System.Timers.Timer
            {
                Interval = 20
            };
            Timer.Elapsed += (sender, e) => {
                Timer.Enabled = false;
                Dispatch();
            };
        }

        protected uint SendInput(WinAPI.INPUT[] input)
        {
            return NativeMethods.SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(WinAPI.INPUT)));
        }

        public void Dispose()
        {
            Timer.Dispose();
        }

        private void Dispatch()
        {
            if (!Queue.IsEmpty && Queue.TryDequeue(out MouseEvent mouseEvent))
            {
                SendInput(mouseEvent.Input);
            }

            Timer.Start();
        }
    }
}
