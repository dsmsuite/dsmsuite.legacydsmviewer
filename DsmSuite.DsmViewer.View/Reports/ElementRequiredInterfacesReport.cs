using System;
using System.IO;
using System.Linq;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public class ElementRequiredInterfacesReport : BaseReport
    {
        public ElementRequiredInterfacesReport(IDsmModel model) : base(model)
        {
        }

        protected override void WriteReportData()
        {
            var relations = _model.FindConsumerRelations(_model.SelectedTreeNode);

            AddHeader(1, $"Required interface of {_model.SelectedTreeNode.NodeValue}");
            string[] headers = { "Nr", "Required name", "Required type" };
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
