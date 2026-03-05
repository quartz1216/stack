using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace stack;

public partial class MainWindow : Window
{
    private NoteData _note;
    private bool _isDiscarding = false;
    private bool _isPushing = false;

    public MainWindow(NoteData? note = null)
    {
        InitializeComponent();

        _note = note ?? new NoteData();
        
        if (note != null)
        {
            this.Left = note.X;
            this.Top = note.Y;
            this.Width = note.Width > 0 ? note.Width : 400;
            this.Height = note.Height > 0 ? note.Height : 400;
            Editor.Text = note.Text;
        }

        // Setup Event Handlers
        this.Loaded += (s, e) => Editor.Focus();
        this.Closing += MainWindow_Closing;
        this.LocationChanged += (s, e) => SaveState();
        this.SizeChanged += (s, e) => SaveState();
        Editor.LostFocus += (s, e) => SaveState();
        Editor.TextChanged += (s, e) => { SaveState(); UpdateTitle(); };

        // Shift+Scroll for horizontal scrolling
        Editor.PreviewMouseWheel += (s, e) =>
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                var scrollViewer = FindScrollViewer(Editor);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                    e.Handled = true;
                }
            }
        };

        // Setup Key Bindings
        SetupHotkeys();

        // Set initial title
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        string preview = Editor.Text.Replace("\r", "").Replace("\n", " ").Trim();
        if (preview.Length > 10) preview = preview.Substring(0, 10);
        this.Title = string.IsNullOrEmpty(preview) ? "Stack" : $"{preview} - Stack";
    }

    private static System.Windows.Controls.ScrollViewer? FindScrollViewer(System.Windows.DependencyObject parent)
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is System.Windows.Controls.ScrollViewer sv) return sv;
            var result = FindScrollViewer(child);
            if (result != null) return result;
        }
        return null;
    }

    private void SetupHotkeys()
    {
        // Add InputBindings
        InputBindings.Add(new KeyBinding(new RelayCommand(New_Click), Key.N, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(Push_Click), Key.W, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(Pop_Click), Key.R, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(Discard_Click), Key.Q, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(Wrap_Click), Key.J, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(Minimize_Click), Key.H, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(Maximize_Click), Key.M, ModifierKeys.Control));

        // Alt key toggles menu visibility
        this.PreviewKeyDown += (s, e) =>
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                if (MainMenu.Visibility == Visibility.Collapsed)
                    MainMenu.Visibility = Visibility.Visible;
                else
                    MainMenu.Visibility = Visibility.Collapsed;
            }
            if (e.Key == Key.Escape)
            {
                MainMenu.Visibility = Visibility.Collapsed;
                Editor.Focus();
            }
        };
    }

    private void SaveState()
    {
        if (_isDiscarding || _note.IsStashed) return;

        _note.Text = Editor.Text;
        if (this.WindowState == WindowState.Normal)
        {
            _note.X = this.Left;
            _note.Y = this.Top;
            _note.Width = this.Width;
            _note.Height = this.Height;
        }
        
        SessionManager.Update(_note);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_isDiscarding) return;

        SaveState();
        if (!_isPushing)
        {
            // Normal close (X button, Alt+F4): Push to stash
            SessionManager.Push(_note);
        }
    }

    private void New_Click(object? sender, RoutedEventArgs? e) => New_Click();
    private void New_Click()
    {
        new MainWindow(null).Show();
    }

    private void Push_Click(object? sender, RoutedEventArgs? e) => Push_Click();
    private void Push_Click()
    {
        _isPushing = true;
        SaveState();
        SessionManager.Push(_note);
        _isDiscarding = true; // Prevent double-save in Closing handler
        this.Close();
    }

    private void Pop_Click(object? sender, RoutedEventArgs? e) => Pop_Click();
    private void Pop_Click()
    {
        var poppedNote = SessionManager.Pop();
        if (poppedNote != null)
        {
            var window = new MainWindow(poppedNote);
            window.Show();
        }
    }

    private void Discard_Click(object? sender, RoutedEventArgs? e) => Discard_Click();
    private void Discard_Click()
    {
        _isDiscarding = true;
        SessionManager.Discard(_note.Id);
        this.Close();
    }

    private void Wrap_Click(object? sender, RoutedEventArgs? e) => Wrap_Click();
    private void Wrap_Click()
    {
        Editor.TextWrapping = Editor.TextWrapping == TextWrapping.Wrap ? TextWrapping.NoWrap : TextWrapping.Wrap;
    }

    private void Minimize_Click(object? sender, RoutedEventArgs? e) => Minimize_Click();
    private void Minimize_Click()
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object? sender, RoutedEventArgs? e) => Maximize_Click();
    private void Maximize_Click()
    {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void MenuList_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        MenuList.Items.Clear();
        var notes = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderByDescending(System.Linq.Enumerable.Where(SessionManager.Load(), n => n.IsStashed), n => n.LastAccessed));
        
        if (notes.Count == 0)
        {
            MenuList.Items.Add(new System.Windows.Controls.MenuItem { Header = "Empty", IsEnabled = false });
            return;
        }

        for (int i = 0; i < notes.Count; i++)
        {
            var note = notes[i];
            string preview = note.Text.Replace("\r", "").Replace("\n", " ");
            if (preview.Length > 10) preview = preview.Substring(0, 10);
            if (string.IsNullOrWhiteSpace(preview)) preview = "(Empty)";

            var item = new System.Windows.Controls.MenuItem
            {
                Header = $"{i + 1} - {preview}"
            };
            item.Click += (s, args) => 
            {
                var popped = SessionManager.PopSpecific(note.Id);
                if (popped != null)
                {
                    new MainWindow(popped).Show();
                }
            };
            MenuList.Items.Add(item);
        }
    }

    private void Readme_Click(object? sender, RoutedEventArgs? e)
    {
        string helpText = "Help not found.";
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("stack.README.md");
        if (stream != null)
        {
            using var reader = new System.IO.StreamReader(stream);
            helpText = reader.ReadToEnd();
        }

        var readmeNote = new NoteData
        {
            Text = helpText,
            Width = 400,
            Height = 400
        };
        new MainWindow(readmeNote).Show();
    }
}

// Simple RelayCommand for KeyBindings
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}