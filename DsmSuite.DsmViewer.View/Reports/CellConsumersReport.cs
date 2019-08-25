using System;
using System.IO;
using System.Linq;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public class CellConsumersReport : BaseReport
    {
        public CellConsumersReport(IDsmModel model) : base(model)
        {
        }

        protected override void WriteReportData()
        {
            var relations = _model.FindRelations(
                _model.SelectedProviderTreeNode,
                _model.SelectedConsumerTreeNode);

            AddHeader(1, $"Consumers in relations between consumer {_model.SelectedConsumerTreeNode.NodeValue} and provider {_model.SelectedProviderTreeNode.NodeValue}");
            string[] headers = { "Nr", "Consumer name", "Consumer type" };
            XmlNode tbody = CreateTable(headers);

            int index = 1;
            foreach (var item in relations.OrderBy(x => x.Consumer.FullName).GroupBy(x => x.Consumer.FullName).Select(x => x.FirstOrDefault()).ToList())
            {
                string[] cells = { index.ToString(), item.Consumer.FullName, item.Consumer.Type };
                AddTableRow(tbody, cells);
                index++;
            }
        }
    }
}
