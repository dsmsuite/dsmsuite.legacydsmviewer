using System;
using System.Windows.Forms;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.Model.Exceptions;

namespace DsmSuite.DsmViewer.View.ViewModel
{
    class CommandSaveAs : ICommand
    {
        private readonly IDsmModel _model;
        bool _done;

        public CommandSaveAs(IDsmModel model)
        {
            _model = model;
        }

        public bool Completed => _done;

        public void Execute(ProgressUpdateDelegate updateFunction)
        {
            try
            {
                string filename = GetFile();

                if (filename != null)
                {
                    _model.SaveModel(filename, _model.IsCompressed);
                    _done = true;
                }
            }
            catch (Exception e)
            {
                throw new DsmException("Unable to save project", e);
            }
        }

        string GetFile()
        {
            string filename = null;

            SaveFileDialog dlg = new SaveFileDialog
            {
                AddExtension = true,
                OverwritePrompt = false,
                DefaultExt = "dsm",
                Filter = "DSM project files (*.dsm)|*.dsm",
                Title = "Save DSM project"
            };

            DialogResult result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                filename = dlg.FileName;
            }

            return filename;
        }
    }
}
