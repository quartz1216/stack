using System.Linq;
using System.Windows;

namespace stack;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var notes = SessionManager.Load();

        if (notes.Count > 0)
        {
            // Restore all notes (stashed and non-stashed)
            foreach (var note in notes)
            {
                note.IsStashed = false;
                var window = new MainWindow(note);
                window.Show();
            }
            SessionManager.Save(notes);
        }
        else
        {
            var window = new MainWindow(null);
            window.Show();
        }
    }
}
