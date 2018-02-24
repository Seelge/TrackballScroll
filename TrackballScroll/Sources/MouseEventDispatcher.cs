using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private System.Timers.Timer Timer { get; }

        public MouseEventDispatcher(ConcurrentQueue<MouseEvent> queue)
        {
            Queue = queue;

            Timer = new System.Timers.Timer
            {
                AutoReset = false,
                Interval = 100
            };
            Timer.Elapsed += (sender, e) => {
                Dispatch();
            };
            Timer.Enabled = true;
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
            if(!Queue.IsEmpty)
            {
                // get and count all existing inputs from the queue
                var dispatchQueue = new Queue<MouseEvent>();
                int inputCount = 0;
                while(!Queue.IsEmpty && Queue.TryDequeue(out MouseEvent mouseEvent))
                {
                    dispatchQueue.Enqueue(mouseEvent);
                    inputCount += mouseEvent.Input.Length; // input size may vary per event
                }
                // create new input containing all inputs
                var input = new WinAPI.INPUT[inputCount];
                for(int i = 0; i < inputCount;)
                {
                    var mouseEvent = dispatchQueue.Dequeue();
                    for(int j = 0; j < mouseEvent.Input.Length; ++j)
                    {
                        input[i + j] = mouseEvent.Input[j];
                    }
                    i += mouseEvent.Input.Length; // advance by number of inputs
                }
                SendInput(input);
            }

            Timer.Enabled = true;
        }
    }
}
