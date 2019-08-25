using System;
using System.IO;
using System.Linq;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public class CellProvidersReport : BaseReport
    {
        public CellProvidersReport(IDsmModel model) : base(model)
        {
        }

        protected override void WriteReportData()
        {
            var relations = _model.FindRelations(
                _model.SelectedProviderTreeNode,
                _model.SelectedConsumerTreeNode);

            AddHeader(1, $"Providers in relations between consumer {_model.SelectedConsumerTreeNode.NodeValue} and provider {_model.SelectedProviderTreeNode.NodeValue}");
            string[] headers = { "Nr", "Provider name", "Provider type" };
            XmlNode tbody = CreateTable(headers);

            int index = 1;
            foreach (var item in relations.OrderBy(x => x.Provider.FullName).GroupBy(x => x.Provider.FullName).Select(x => x.FirstOrDefault()).ToList())
            {
                string[] cells = { index.ToString(), item.Provider.FullName, item.Provider.Type };
                AddTableRow(tbody, cells);
                index++;
            }
        }
    }
}
