using System.Collections.ObjectModel;
using Dereliction.Models;
using ReactiveUI;

namespace Dereliction.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        private ObservableCollection<FsElement> _scriptList = null!;

        public ObservableCollection<FsElement> ScriptList
        {
            get => _scriptList;
            set { this.RaiseAndSetIfChanged(ref _scriptList, value); }
        }

        public EditorViewModel()
        {
            ScriptList = new ObservableCollection<FsElement>();
            /*ScriptList.Add(new RealFsElement("Hello, there.", @"C:\Users"));
            ScriptList.Add(new RealFsElement("General Kenobi.", @"C:\Users\black"));*/
        }

        public OperationStateModel State { get; } = new();

        public void OpenFile(RealFsElement element)
        {
            // TODO file opening
        }

        public void RefreshScriptFolder()
        {
            // TODO load script folder
        }

        public void NewScript()
        {
            // TODO new script
        }

        public void SaveScript()
        {
            // TODO save script
        }
    }
}
