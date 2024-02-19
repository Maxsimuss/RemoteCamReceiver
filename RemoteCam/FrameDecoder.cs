using System;
using System.Collections.Generic;
using System.Threading;

namespace RemoteCamReceiver;

public class FrameDecoder
{
    private readonly PriorityQueue<Frame, int> _readyFrames = new();
    private int _lastFrame;

    public Frame? GetNextFrame()
    {
        Frame? displayFrame = null;

        lock (_readyFrames)
        {
            if (_readyFrames.Count > 15) // Give me ANY frame
            {
                displayFrame = _readyFrames.Dequeue();
            }

            if (_readyFrames.Count > 0) // Find latest frame in order, drop others
            {
                bool cont;
                do
                {
                    var frame = _readyFrames.Peek();
                    if (frame.Id == _lastFrame + 1)
                    {
                        cont = true;
                        displayFrame = _readyFrames.Dequeue();
                        _lastFrame = displayFrame.Id;
                    }
                    else
                    {
                        cont = false;
                    }
                } while (cont && _readyFrames.Count > 3); // drop frames if we have more than 3 in buffer
            }
        }

        if (displayFrame != null) _lastFrame = displayFrame.Id;

        return displayFrame;
    }

    protected void HandleFrame(int id, byte[] data)
    {
        lock (_readyFrames)
        {
            if (_readyFrames.Count > 30) return; // Drop frame in case we can't keep up displaying them
        }

        Frame frame = new Frame(id, data);

        lock (_readyFrames)
        {
            _readyFrames.Enqueue(frame, frame.Id);
        }
    }
}