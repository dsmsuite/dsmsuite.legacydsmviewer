using System;
using System.IO;
using DsmSuite.DsmViewer.View.Dialogs;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.View.Reports;

namespace DsmSuite.DsmViewer.View.ViewModel
{
    class CommandReport : ICommand
    {
        readonly IDsmModel _model;
        private bool _done;

        public CommandReport(IDsmModel model)
        {
            _model = model;
        }

        public bool Completed => _done;

        public void Execute(ProgressUpdateDelegate updateFunction)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                OverviewReport report = new OverviewReport(_model);
                report.WriteReport(sw);

                HtmlViewer viewer = new HtmlViewer(ms);
                viewer.Show();

                _done = true;
            }
            catch (Exception ex)
            {
                ErrorDialog.Show(ex.ToString());
            }
        }
    }
}
