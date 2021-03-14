using System;
using System.IO;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Dereliction.Models;
using Dereliction.ViewModels;

namespace Dereliction.Views
{
    public class EditorView : UserControl
    {
        private EditorViewModel ViewModel => DataContext as EditorViewModel ?? throw new ApplicationException();
        private readonly TextEditor _textEditor;
        //private CompletionWindow? _completionWindow;
        //private OverloadInsightWindow? _insightWindow;

        public EditorView()
        {
            IHighlightingDefinition csScriptDarkHighlighting;
            using (Stream? s =
                typeof(EditorView).Assembly.GetManifestResourceStream("Dereliction.CSharp-Mode-ScriptDark.xshd"))
            {
                if (s == null) throw new InvalidOperationException("Could not find embedded resource");
                using XmlReader reader = new XmlTextReader(s);
                csScriptDarkHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            HighlightingManager.Instance.RegisterHighlighting("C#ScriptDark", new[] {".csx"}, csScriptDarkHighlighting);

            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = true;
            //_textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            //_textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            //_textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy();

            AddHandler(
                PointerWheelChangedEvent,
                (_, i) =>
                {
                    if ((i.KeyModifiers & KeyModifiers.Control) != 0)
                        _textEditor.FontSize = Math.Clamp(_textEditor.FontSize + Math.Sign(i.Delta.Y) * 2, 2, 140);
                },
                RoutingStrategies.Bubble,
                true);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public string GetBody() => _textEditor.Document.Text;

        private void FsTreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1 && e.AddedItems[0] is RealFsElement fse)
                ViewModel.OpenFile(fse);
        }

        /*void textEditor_TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }

            _insightWindow?.Hide();

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        void textEditor_TextArea_TextEntered(object? sender, TextInputEventArgs e)
        {
            if (e.Text == ".")
            {
                _completionWindow = new CompletionWindow(_textEditor.TextArea);
                _completionWindow.Closed += (_, _) => _completionWindow = null;

                var data = _completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));


                _completionWindow.Show();
            }
            else if (e.Text == "(")
            {
                _insightWindow = new OverloadInsightWindow(_textEditor.TextArea);
                _insightWindow.Closed += (_, _) => _insightWindow = null;

                _insightWindow.Provider = new MyOverloadProvider(new[]
                {
                    ("Method1(int, string)", "Method1 description"), ("Method2(int)", "Method2 description")
                });

                _insightWindow.Show();
            }
        }

        private class MyOverloadProvider : IOverloadProvider
        {
            private readonly IList<(string header, string content)> _items;
            private int _selectedIndex;

            public MyOverloadProvider(IList<(string header, string content)> items)
            {
                _items = items;
                SelectedIndex = 0;
            }

            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    // ReSharper disable ExplicitCallerInfoArgument
                    OnPropertyChanged(nameof(CurrentHeader));
                    OnPropertyChanged(nameof(CurrentContent));
                    // ReSharper restore ExplicitCallerInfoArgument
                }
            }

            public int Count => _items.Count;
            public string? CurrentIndexText => null;
            public object CurrentHeader => _items[SelectedIndex].header;
            public object CurrentContent => _items[SelectedIndex].content;

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class MyCompletionData : ICompletionData
        {
            public MyCompletionData(string text)
            {
                Text = text;
            }

            public IBitmap? Image => null;

            public string Text { get; }

            // Use this property if you want to show a fancy UIElement in the list.
            public object Content => Text;

            public object Description => "Description for " + Text;

            public double Priority { get; } = 0;

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }
        }*/
    }
}
