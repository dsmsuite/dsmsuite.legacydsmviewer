using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public class OverviewReport : BaseReport
    {
        public OverviewReport(IDsmModel model) : base(model)
        {
        }

        protected override void WriteReportData()
        {
            AddHeader(1, "Overview");
            AddHeader(2, "Meta data");

                foreach (string groupName in _model.GetMetaDataGroupNames())
                {
                    AddHeader(3, groupName);

                    XmlNode tbody = CreateTable();
                    foreach (KeyValuePair<string,string> kv in _model.GetMetaData(groupName))
                    {
                        string[] cells = {kv.Key, ": " + kv.Value};
                        AddTableRow(tbody, cells);
                    }
                }
        }

        protected override void WriteHtmlStyles(StreamWriter sw)
        {
            base.WriteHtmlStyles(sw);
            sw.WriteLine("td      { width: 600px; }");
        }
    }
}
