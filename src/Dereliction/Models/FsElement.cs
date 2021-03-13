using Fp;
using ReactiveUI;

namespace Dereliction.Models
{
    public abstract class FsElement : ReactiveObject
    {
        public FsElement(string name)
        {
            _name = name;
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
        }

        private Data _content;

        public Data Content
        {
            get => _content;
            set { this.RaiseAndSetIfChanged(ref _content, value); }
        }
    }
}
