using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace stack;

public static class SessionManager
{
    private static readonly string SessionFilePath = Path.Combine(AppContext.BaseDirectory, "session.json");
    private static readonly object _fileLock = new();

    public static List<NoteData> Load()
    {
        lock (_fileLock)
        {
            if (!File.Exists(SessionFilePath))
                return new List<NoteData>();

            try
            {
                var json = File.ReadAllText(SessionFilePath);
                return JsonSerializer.Deserialize(json, NoteDataJsonContext.Default.ListNoteData) ?? new List<NoteData>();
            }
            catch
            {
                return new List<NoteData>();
            }
        }
    }

    public static void Save(List<NoteData> notes)
    {
        lock (_fileLock)
        {
            try
            {
                var json = JsonSerializer.Serialize(notes, NoteDataJsonContext.Default.ListNoteData);
                File.WriteAllText(SessionFilePath, json);
            }
            catch
            {
            }
        }
    }

    public static void Push(NoteData note)
    {
        var notes = Load();
        var existing = notes.FirstOrDefault(n => n.Id == note.Id);
        
        note.IsStashed = true;
        note.LastAccessed = DateTime.UtcNow;

        if (existing != null)
        {
            notes[notes.IndexOf(existing)] = note;
        }
        else
        {
            notes.Add(note);
        }

        Save(notes);
    }

    public static NoteData? Pop()
    {
        var notes = Load();
        var mostRecentStashed = notes
            .Where(n => n.IsStashed)
            .OrderByDescending(n => n.LastAccessed)
            .FirstOrDefault();

        if (mostRecentStashed != null)
        {
            mostRecentStashed.IsStashed = false;
            mostRecentStashed.LastAccessed = DateTime.UtcNow;
            
            Save(notes);
            return mostRecentStashed;
        }
        return null; // Return null if stack is empty
    }

    public static NoteData? PopSpecific(Guid id)
    {
        var notes = Load();
        var note = notes.FirstOrDefault(n => n.Id == id);
        if (note != null && note.IsStashed)
        {
            note.IsStashed = false;
            note.LastAccessed = DateTime.UtcNow;
            Save(notes);
            return note;
        }
        return null;
    }

    public static void Discard(Guid id)
    {
        var notes = Load();
        var removed = notes.RemoveAll(n => n.Id == id);
        if (removed > 0)
        {
            Save(notes);
        }
    }

    public static void Update(NoteData note)
    {
        var notes = Load();
        var idx = notes.FindIndex(n => n.Id == note.Id);
        if (idx != -1)
        {
            notes[idx] = note;
        }
        else
        {
            notes.Add(note);
        }
        Save(notes);
    }
}
