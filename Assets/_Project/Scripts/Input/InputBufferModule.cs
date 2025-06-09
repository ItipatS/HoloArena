using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputBufferModule
{
    private struct BufferedInput
    {
        public string action;
        public float frameStamp;
        public float realTimeStamp;
    }

    private List<BufferedInput> buffer = new List<BufferedInput>();
    float bufferTimeInSeconds;
    private float bufferFrames;
    private int currentFrame;
    private const int maxBufferSize = 20; // Prevent buffer overflow

    public InputBufferModule(float bufferTimeInSeconds, int targetFPS = 60)
    {
        this.bufferTimeInSeconds = bufferTimeInSeconds;
        this.bufferFrames = Mathf.RoundToInt(bufferTimeInSeconds * targetFPS);
        this.currentFrame = 0;
    }

    // Add new input to the buffer
    public void AddInput(string action)
    {
        // Prevent duplicates if the same input is spammed within a single frame
        if (buffer.Count == 0 || buffer.Last().action != action || buffer.Last().frameStamp != currentFrame)
        {
            buffer.Add(new BufferedInput
            {
                action = action,
                frameStamp = currentFrame,
                realTimeStamp = Time.fixedTime
            });
            if (buffer.Count > maxBufferSize)
            {
                buffer.RemoveAt(0);
            }
        }
    }

    // Consume an input
    public bool ConsumeInput(string action)
    {
        var index = buffer.FindIndex(b => b.action == action);
        if (index >= 0)
        {
            buffer.RemoveAt(index);
            return true;
        }
        return false;
    }

    // Check for a combo sequence with flexible timing
    public bool CheckCommandInput(string[] sequence, float maxFrameGap, bool consume = true)
    {
        if (buffer.Count < sequence.Length)
        {
            return false;
        }

        int seqIndex = sequence.Length - 1;
        int bufferIndex = buffer.Count - 1;
        float lastTime = Time.fixedTime;

        List<int> matchedIndices = new List<int>();

        while (bufferIndex >= 0 && seqIndex >= 0)
        {
            if (buffer[bufferIndex].action.Equals(sequence[seqIndex], StringComparison.OrdinalIgnoreCase) &&
                (lastTime - buffer[bufferIndex].realTimeStamp) <= maxFrameGap)
            {
                matchedIndices.Add(bufferIndex);
                lastTime = buffer[bufferIndex].realTimeStamp;
                seqIndex--;
                if (seqIndex < 0) break;
            }
            bufferIndex--;
        }

        bool success = seqIndex < 0;
        if (success && consume)
        {
            matchedIndices.Sort();
            matchedIndices.Reverse(); // Remove from highest index first to avoid shifting issues
            foreach (int idx in matchedIndices)
            {
                if (idx >= 0 && idx < buffer.Count)
                    buffer.RemoveAt(idx);
            }
            PrintBuffer();
            Debug.Log($"Combo detected and consumed: {string.Join(" -> ", sequence)}");
        }
        return success;
    }

    // Peek if an input exists
    public bool PeekInput(string action)
    {
        return buffer.Any(input => input.action == action);
    }

    // Debug helper to print buffer contents
     private void PrintBuffer()
    {
        Debug.Log($"Current Frame: {currentFrame}, Buffer: " +
            string.Join(", ", buffer.Select(b => 
                $"{b.action}(f:{currentFrame - b.frameStamp}, t:{Time.fixedTime - b.realTimeStamp:F2}s)")));
    }
    public void Tick()
    {
        currentFrame++;

         buffer.RemoveAll(b => 
            (currentFrame - b.frameStamp) > bufferFrames || 
            (Time.fixedTime - b.realTimeStamp) > bufferTimeInSeconds
        );
    }

    public void Cleanup()
    {
        buffer.Clear();
        Debug.Log("Input buffer cleared");
    }

}
