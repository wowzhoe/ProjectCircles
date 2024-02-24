using UnityEngine;
using System.Collections.Generic;

public static class Log
{
    private static bool ENABLE = Debug.isDebugBuild;

    private const bool ENABLE_NOTES = true;
    private const bool ENABLE_WARNINGS = true;
    private const bool ENABLE_ERRORS = true;


    static public void Note(object message)
    {
        if (ENABLE && ENABLE_NOTES)
            Debug.Log(message);
    }
    static public void Warning(object message)
    {
        if (ENABLE && ENABLE_WARNINGS)
            Debug.LogWarning(message);
    }
    static public void Error(object message)
    {
        if (ENABLE && ENABLE_ERRORS)
            Debug.LogError(message);
    }
}

public class VisualLog
{
    public class LogEntry
    {
        public  string  msg;
        public  Color   color;
        public  float   time;
    };

    static private Color NoteColor    = Color.green;
    static private Color WarningColor = Color.yellow;
    static private Color ErrorColor   = Color.red;

    static private float NoteTime     = 2.0f;
    static private float WarningTime  = 1.0f;
    static private float ErrorTime    = 3.0f;

    static public List<LogEntry> entries = new List<LogEntry>();


    static public void Initialize()
    {
        entries = new List<LogEntry>();
    }


    static public void Update()
    {
        if (entries == null)
            return;

        entries.ForEach(e => e.time -= Time.deltaTime);
        entries.RemoveAll(e => e.time < 0.0f);
    }


    static private void add_entry(string s, Color color, float time)
    {
        LogEntry e = new LogEntry();
        e.msg   = s;
        e.color = color;
        e.time  = time;

        entries.Add(e);
    }


    static public void Note(string s)
    {
        Log.Note(s);
        add_entry(s, NoteColor, NoteTime);
    }


    static public void Warning(string s)
    {
        Log.Warning(s);
        add_entry(s, WarningColor, WarningTime);
    }


    static public void Error(string s)
    {
        Log.Error(s);
        add_entry(s, ErrorColor, ErrorTime);
    }


    static public void Visualize()
    {
        if (entries == null)
            return;

        Color old_color = GUI.color;

        GUI.color = Color.green;
        int start_y = 10;

        foreach(LogEntry e in entries)
        {
            Rect rc = new Rect(10, start_y, 300, 20);

            GUI.color = e.color;
            GUI.Label(rc, e.msg);

            start_y += 12;
        }

        GUI.color = old_color;
    }
}
