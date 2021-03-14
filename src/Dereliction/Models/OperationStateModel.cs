using System;
using ReactiveUI;

namespace Dereliction.Models
{
    public class OperationStateModel : ReactiveObject
    {
        private bool _busy;
        private bool _locked;
        private float _percent;
        private string _logText = string.Empty;
        private string __logText = string.Empty;

        public bool Busy
        {
            get => _busy;
            set => this.RaiseAndSetIfChanged(ref _busy, value);
        }

        public bool Locked
        {
            get => _locked;
            set => this.RaiseAndSetIfChanged(ref _locked, value);
        }

        public float Percent
        {
            get => _percent;
            set => this.RaiseAndSetIfChanged(ref _percent, value);
        }

        public string LogText
        {
            get => _logText;
            set => this.RaiseAndSetIfChanged(ref _logText, value);
        }


        public void HardUp(Func<string, string> func) => LogText = __logText = func(__logText);
    }
}
