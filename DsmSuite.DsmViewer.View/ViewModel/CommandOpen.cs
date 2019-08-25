using System;
using System.IO;
using System.Windows.Forms;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.Model.Exceptions;

namespace DsmSuite.DsmViewer.View.ViewModel
{
    class CommandOpen : ICommand
    {
        IDsmModel _model;
        bool _done;
        private readonly FileInfo _file;
        readonly DirectoryInfo _directory;

        public CommandOpen(IDsmModel model, DirectoryInfo startDirectory) : this(model, null, startDirectory)
        {
        }

        public CommandOpen(IDsmModel model, FileInfo file) : this(model, file, null)
        {
        }

        protected CommandOpen(IDsmModel model, FileInfo file, DirectoryInfo startDirectory)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            _model = model;
            _file = file;
            _directory = startDirectory;
        }

        public bool Completed => _done;

        public void Execute(ProgressUpdateDelegate updateFunction)
        {
            try
            {
                string filename = GetFile();

                if (filename != null)
                {
                    updateFunction?.Invoke(0, filename);

                    _model.LoadModel(filename);
                    updateFunction?.Invoke(100, "done");

                    _done = true;
                }


            }
            catch (Exception e)
            {
                throw new DsmException("Error opening Dsm file", e);
            }
        }

        string GetFile()
        {
            if (_file == null)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                if (_directory != null)
                {
                    dlg.InitialDirectory = _directory.FullName;
                }

                dlg.AddExtension = true;
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.DefaultExt = "dsm";
                dlg.Filter = "DSM project files (*.dsm)|*.dsm|All files (*.*)|*.*";
                dlg.Title = "Open DSM project";

                DialogResult result = dlg.ShowDialog();

                if (result == DialogResult.OK)
                {
                    return dlg.FileName;
                }

                return null;
            }

            return _file.FullName;
        }
    }
}
