using System.Collections.Generic;
using UnityEngine;

public class InputWindowCheckerModule
{
    private const int MAX_HISTORY = 20;
    private List<(string input, float time)> inputHistory = new List<(string, float)>();

    // Add input with timestamp
    public void RecordInput(string inputName)
    {
        if (inputHistory.Count >= MAX_HISTORY)
            inputHistory.RemoveAt(0);

        inputHistory.Add((inputName, Time.time));
    }

    // Check if input occurred within a time window
    public bool CheckInputInWindow(string inputName, float windowDuration, bool consume = true)
    {
        float currentTime = Time.time;
        for (int i = inputHistory.Count - 1; i >= 0; i--)
        {
            var (name, time) = inputHistory[i];
            if (name == inputName && currentTime - time <= windowDuration)
            {
                if (consume) inputHistory.RemoveAt(i); // ✅ Only remove if requested
                return true;
            }
        }
        return false;
    }
    // Clear old inputs to prevent memory issues
    public void Cleanup(float maxLifetime = 1f)
    {
        float currentTime = Time.time;
        
        while (inputHistory.Count > 0 && currentTime - inputHistory[0].time > maxLifetime)
        {
            inputHistory.RemoveAt(0); // ✅ Removes only the oldest inputs
        }
    }
}
