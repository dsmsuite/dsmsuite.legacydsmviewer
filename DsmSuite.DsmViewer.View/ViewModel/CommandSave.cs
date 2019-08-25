using System;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.Model.Exceptions;

namespace DsmSuite.DsmViewer.View.ViewModel
{
    class CommandSave : ICommand
    {
        private readonly IDsmModel _model;
        bool _done;

        public CommandSave(IDsmModel model)
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
            return _model.ModelFilename;
        }
    }
}
