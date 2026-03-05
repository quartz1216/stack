using System;

namespace stack;

public class NoteData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 400;
    public double Height { get; set; } = 400;
    public bool IsStashed { get; set; }
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
}
