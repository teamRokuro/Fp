using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dereliction.Models;
using Dereliction.Views;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
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
            RefreshScriptFolder();
            NewFileCore(null);
        }

        public EditorStateModel EditorState { get; } = new();
        public OperationStateModel OperationState { get; } = new();

        #region Public surface

        public async Task<bool> NewFileAsync(EditorView? view)
        {
            if (EditorState.Modified && view != null && !await PromptSaveBeforeAsync(view)) return false;
            NewFileCore(view);
            return true;
        }

        public async Task OpenFileVisualAsync(EditorView? view)
        {
            string? file = await PromptSourceAsync(view);
            if (file == null) return;
            string text;
            try
            {
                text = await File.ReadAllTextAsync(file);
            }
            catch (Exception e)
            {
                await DialogOrExceptionAsync(e, view);
                return;
            }

            view?.SetBody(text);
            SetFile(file, false, true);
        }

        public async Task OpenFileAsync(RealFsElement element, EditorView? view)
        {
            string file = element.RealPath;
            string text;
            try
            {
                text = await File.ReadAllTextAsync(file);
            }
            catch (Exception e)
            {
                await DialogOrExceptionAsync(e, view);
                return;
            }

            view?.SetBody(text);
            SetFile(file, false, true);
        }

        public async Task<bool> SaveFileAsync(EditorView? view)
        {
            if (view == null) return false;
            if (!EditorState.Modified) return true;
            if (!EditorState.OnDisk)
            {
                string? name = await PromptTargetAsync(view);
                if (name == null) return false;
                UpdateEditorCore(name, default, true);
            }

            try
            {
                await File.WriteAllTextAsync(EditorState.CurrentFile, view.GetBody());
            }
            catch (Exception e)
            {
                await DialogOrExceptionAsync(e, view);
                return false;
            }

            SetModified(false);
            RefreshScriptFolder();
            return true;
        }

        public void SetModified(bool modified)
        {
            UpdateEditorCore(default, modified, default);
        }

        #endregion

        private static async Task DialogOrExceptionAsync(Exception e, IVisual? control)
        {
            var window = control?.FindAncestorOfType<Window>();
            if (control == null || window == null) throw e;
            var msgWindow = MessageBoxManager.GetMessageBoxStandardWindow(e.Message, e.ToString());
            await msgWindow.Show(window);
        }

        private static async Task<string?> PromptSourceAsync(EditorView? view)
        {
            if (view == null) return null;
            var window = view.FindAncestorOfType<Window>();
            if (window == null) throw new ApplicationException("Couldn't find root window");
            string[] res = await new OpenFileDialog
            {
                Directory = Program.WorkingDirectory,
                Filters = new List<FileDialogFilter>
                {
                    new() {Name = "dotnet-script csx file", Extensions = new List<string> {"csx"}}
                }
            }.ShowAsync(window);
            if (res == null || res.Length == 0) return null;
            return res[0];
        }

        private static async Task<string?> PromptTargetAsync(EditorView? view)
        {
            if (view == null) return null;
            var window = view.FindAncestorOfType<Window>();
            if (window == null) throw new ApplicationException("Couldn't find root window");
            return await new SaveFileDialog
            {
                Directory = Program.WorkingDirectory,
                DefaultExtension = "csx",
                InitialFileName = "untitled.csx",
                Filters = new List<FileDialogFilter>
                {
                    new() {Name = "dotnet-script csx file", Extensions = new List<string> {"csx"}}
                }
            }.ShowAsync(window);
        }

        private async Task<bool> PromptSaveBeforeAsync(EditorView? view)
        {
            if (view == null) return true;
            var window = view.FindAncestorOfType<Window>();
            if (window == null) throw new ApplicationException("Couldn't find root window");
            var msgWindow = MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentTitle = "Save before closing?",
                    ContentMessage = "Do you want to save the current file\nbefore closing?",
                    ButtonDefinitions = ButtonEnum.YesNoCancel,
                });
            return await msgWindow.ShowDialog(window) switch
            {
                ButtonResult.Yes => await SaveFileAsync(view),
                ButtonResult.No => true,
                _ => false
            };
        }

        public void RefreshScriptFolder()
        {
            ScriptList.Clear();
            if (!Directory.Exists(Program.WorkingDirectory)) return;
            foreach (var script in Directory.EnumerateFiles(Program.WorkingDirectory))
                ScriptList.Add(new RealFsElement(Path.GetFileName(script), script));
        }

        private void NewFileCore(EditorView? view)
        {
            view?.ClearBody();
            SetFile(Path.Combine(Program.WorkingDirectory, "untitled.csx"), false, false);
        }

        private void SetFile(string fileName, bool modified, bool onDisk)
        {
            UpdateEditorCore(fileName, modified, onDisk);
        }

        private void UpdateEditorCore(string? fileName, bool? modified, bool? onDisk)
        {
            if (fileName != null)
                EditorState.CurrentFile = fileName;
            if (modified != null)
                EditorState.Modified = modified.Value;
            if (onDisk != null)
                EditorState.OnDisk = onDisk.Value;
            if (fileName != null || modified != null)
            {
                string fn = Path.GetFileName(EditorState.CurrentFile);
                EditorState.DisplayName = EditorState.Modified ? $"* {fn}" : fn;
            }
        }

        public void ShowOperationView(MainWindow mainWindow) => mainWindow.ShowOperationView();
        public async Task RunScriptAsync(MainWindow mainWindow) => await mainWindow.RunScriptAsync();

        public void Quit(MainWindow mw) => mw.OnQuitClicked(mw, EventArgs.Empty);
    }
}
