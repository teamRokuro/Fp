using ReactiveUI;

namespace Dereliction.Models
{
    public class EditorStateModel : ReactiveObject
    {
        private bool _onDisk;
        private bool _modified;
        private string _displayName = string.Empty;
        private string _currentFile = string.Empty;

        public bool OnDisk
        {
            get => _onDisk;
            set => this.RaiseAndSetIfChanged(ref _onDisk, value);
        }

        public bool Modified
        {
            get => _modified;
            set => this.RaiseAndSetIfChanged(ref _modified, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        public string CurrentFile
        {
            get => _currentFile;
            set => this.RaiseAndSetIfChanged(ref _currentFile, value);
        }
    }
}
