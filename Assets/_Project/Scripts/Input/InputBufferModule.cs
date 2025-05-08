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
    private float bufferFrames;
    private int currentFrame;
    private const int maxBufferSize = 20; // Prevent buffer overflow

    public InputBufferModule(float bufferTime, int targetFPS = 60)
    {
        this.bufferFrames = Mathf.RoundToInt(bufferTime * targetFPS);
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
                realTimeStamp = Time.realtimeSinceStartup
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
            //PrintBuffer();
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
        PrintBuffer();

        int seqIndex = sequence.Length - 1;
        int bufferIndex = buffer.Count - 1;
        float lastTime = currentFrame;

        List<int> matchedIndices = new List<int>();

        while (bufferIndex >= 0 && seqIndex >= 0)
        {
            if (buffer[bufferIndex].action.Equals(sequence[seqIndex], StringComparison.OrdinalIgnoreCase) &&
                (lastTime - buffer[bufferIndex].frameStamp) <= maxFrameGap)
            {
                matchedIndices.Add(bufferIndex);
                lastTime = buffer[bufferIndex].frameStamp;
                seqIndex--;
                Debug.LogWarning( "seqIndex:  " + seqIndex);
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
            Debug.Log($"Combo detected and consumed: {string.Join(" -> ", sequence)}");
        }
        else if (!success)
        {
            Debug.Log($"Combo failed: {string.Join(" -> ", sequence)}, Buffer: {string.Join(", ", buffer.Select(b => b.action))}");
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
        /* string debug = string.Join(", ", buffer.Select(b =>
            $"{b.action}(f:{currentFrame - b.frameStamp}, t:{Time.realtimeSinceStartup - b.realTimeStamp:F2}s)")); */
        Debug.Log($"Current Frame: {currentFrame}, Buffer: " +
            string.Join(", ", buffer.Select(b => $"{b.action}(f:{currentFrame - b.frameStamp})")));
    }
    public void Tick()
    {
        currentFrame = Time.frameCount;

        // Only remove expired inputs
        buffer.RemoveAll(b => (currentFrame - b.frameStamp) > bufferFrames);
    }

    public void Cleanup()
    {
        buffer.Clear();
        Debug.Log("Input buffer cleared");
    }

}
