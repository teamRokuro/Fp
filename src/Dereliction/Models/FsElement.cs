using System.Collections.ObjectModel;
using Fp;
using ReactiveUI;

namespace Dereliction.Models
{
    public abstract class FsElement : ReactiveObject
    {
        private ObservableCollection<InfoElement> _infos;

        public ObservableCollection<InfoElement> Infos
        {
            get => _infos;
            set { this.RaiseAndSetIfChanged(ref _infos, value); }
        }

        public FsElement(string name)
        {
            _name = name;
            _infos = new ObservableCollection<InfoElement>();
        }

        private string _name;

        public string Name
        {
            get => _name;
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
    }

    public class RealFsElement : FsElement
    {
        public RealFsElement(string name, string realPath) : base(name)
        {
            _realPath = realPath;
            Infos.Add(new InfoElement(realPath));
        }

        private string _realPath;

        public string RealPath
        {
            get => _realPath;
            set { this.RaiseAndSetIfChanged(ref _realPath, value); }
        }
    }

    public class DataFsElement : FsElement
    {
        public DataFsElement(string name, Data content) : base(name)
        {
            _content = content;
            Infos.Add(new InfoElement(content.ToString()!));
        }

        private Data _content;

        public Data Content
        {
            get => _content;
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }
    }

    public class InfoElement
    {
        public InfoElement(string initial)
        {
            //_value = initial;
            Value = initial;
        }

        public string Value { get; set; }

        /*private string _value;

        public string Value
        {
            get => _value;
            set { this.RaiseAndSetIfChanged(ref _value, value); }
        }*/
    }
}
