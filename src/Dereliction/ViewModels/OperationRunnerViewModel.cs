using System.Collections.ObjectModel;
using Avalonia.VisualTree;
using Dereliction.Models;
using Dereliction.Views;
using ReactiveUI;

namespace Dereliction.ViewModels
{
    public class OperationRunnerViewModel : ViewModelBase
    {
        private ObservableCollection<FsElement> _inputs = null!;

        public ObservableCollection<FsElement> Inputs
        {
            get => _inputs;
            set { this.RaiseAndSetIfChanged(ref _inputs, value); }
        }

        private ObservableCollection<FsElement> _outputs = null!;

        public ObservableCollection<FsElement> Outputs
        {
            get => _outputs;
            set { this.RaiseAndSetIfChanged(ref _outputs, value); }
        }

        public OperationRunnerViewModel()
        {
            Inputs = new ObservableCollection<FsElement>();
            Outputs = new ObservableCollection<FsElement>();
            /*Inputs.Add(new RealFsElement("R1", @"C:\Users"));
            Inputs.Add(new RealFsElement("R2", @"C:\Users"));
            Outputs.Add(new RealFsElement("D1", @"C:\Users"));
            Outputs.Add(new RealFsElement("D2", @"C:\Users"));*/
        }

        public void SetInput()
        {
            // TODO set input
        }

        public void RunScript(MainWindow w)
        {
            var editorView = w.FindDescendantOfType<EditorView>();
            System.Console.WriteLine(editorView.GetBody());
            // TODO execute script
        }
    }
}
