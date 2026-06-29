using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class SceneActivityLog
{
    public struct Entry
    {
        public Entry(float time, string category, string message)
        {
            Time = time;
            Category = category;
            Message = message;
        }

        public float Time { get; }
        public string Category { get; }
        public string Message { get; }
    }

    private static readonly List<Entry> entries = new List<Entry>();

    public static IReadOnlyList<Entry> Entries => entries;
    public static int Count => entries.Count;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetForLoadedScene()
    {
        Clear();
    }

    public static void Add(string category, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        entries.Add(new Entry(Time.timeSinceLevelLoad, Normalize(category), message.Trim()));
    }

    public static void Clear()
    {
        entries.Clear();
    }

    public static string BuildDisplayText()
    {
        if (entries.Count == 0)
        {
            return "Логи пока пустые.";
        }

        StringBuilder builder = new StringBuilder(entries.Count * 80);
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            builder.Append('[');
            builder.Append(FormatTime(entry.Time));
            builder.Append("] ");
            builder.Append(entry.Category);
            builder.Append(": ");
            builder.Append(entry.Message);

            if (i < entries.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private static string Normalize(string category)
    {
        return string.IsNullOrWhiteSpace(category) ? "Событие" : category.Trim();
    }

    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds - minutes * 60f;
        return minutes.ToString("00", CultureInfo.InvariantCulture) + ":" + remainingSeconds.ToString("00.0", CultureInfo.InvariantCulture);
    }
}
