using System;
using System.IO;
using System.Linq;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public class ElementConsumersReport : BaseReport
    {
        public ElementConsumersReport(IDsmModel model) : base(model)
        {
        }

        protected override void WriteReportData()
        {
            var relations = _model.FindProviderRelations(_model.SelectedTreeNode);

            AddHeader(1, $"Consumers of {_model.SelectedTreeNode.NodeValue}");
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
