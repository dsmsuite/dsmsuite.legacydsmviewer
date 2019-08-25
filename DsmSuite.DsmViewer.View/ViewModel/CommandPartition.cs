using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.ViewModel
{
    class CommandPartition : ICommand
    {
        readonly IDsmModel _model;

        public CommandPartition(IDsmModel model)
        {
            _model = model;
        }

        public void Execute(ProgressUpdateDelegate updateFunction)
        {
            if (_model.SelectedTreeNode != null)
            {
                _model.Partition();
            }
        }

        public bool Completed => true;
    }
}
