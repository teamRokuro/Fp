using System;
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
            // TODO
            ScriptList.Add(new RealFsElement("Hello, there.", @"C:\Users"));
            ScriptList.Add(new RealFsElement("General Kenobi.", @"C:\Users\black"));
        }

        public EditorStateModel State { get; } = new();

        public void OpenFile(RealFsElement element)
        {
            // TODO
            Console.WriteLine(element.Name);
        }
    }
}
