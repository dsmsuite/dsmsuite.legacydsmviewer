using System;
using System.IO;
using System.Linq;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public class RelationsReport : BaseReport
    {
        public RelationsReport(IDsmModel model) : base(model)
        {
        }

        protected override void WriteReportData()
        {
            var relations = _model.FindRelations(
                _model.SelectedProviderTreeNode,
                _model.SelectedConsumerTreeNode);

            AddHeader(1, $"Relations between consumer {_model.SelectedConsumerTreeNode.NodeValue} and provider {_model.SelectedProviderTreeNode.NodeValue}");
            string[] headers = { "Nr", "Consumer name", "Provider name", "Relation type", "Weight", "Cyclic" };
            XmlNode tbody = CreateTable(headers);

            int index = 1;
            foreach (var item in relations.OrderBy(x => x.Provider.FullName).ThenBy(x => x.Consumer.FullName))
            {
                string cyclic = item.IsCyclic ? "yes" : "-";
                string[] cells = { index.ToString(), item.Consumer.FullName, item.Provider.FullName, item.Type, item.Weight.ToString(), cyclic };
                AddTableRow(tbody, cells);
                index++;
            }
        }
    }
}
