using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DsmSuite.DsmViewer.Model.Model;

namespace DsmSuite.DsmViewer.View.Reports
{
    public abstract class BaseReport
    {
        protected readonly IDsmModel _model;
        private XmlDocument _xhtml;
        private XmlNode _root;

        protected BaseReport(IDsmModel model)
        {
            _model = model;
        }

        public void WriteReport(StreamWriter sw)
        {
            WriteHtmlHeader(sw);
            WriteHtmlContent(sw);
            WriteHtmlFooter(sw);
        }
        
        private void WriteHtmlHeader(StreamWriter sw)
        {
            sw.WriteLine("<html><head><style>");
            WriteHtmlStyles(sw);
            sw.WriteLine("</style></head><body>");
        }

        private void WriteHtmlContent(StreamWriter sw)
        {
            try
            {
                CreateHtmlReport();
                WriteReportData();
                sw.WriteLine(_xhtml.OuterXml);
            }
            catch (Exception ex)
            {
                sw.WriteLine(ex.Message);
            }
        }
        
        private void WriteHtmlFooter(StreamWriter sw)
        {
            sw.WriteLine("</body>");
            sw.Flush();
        }

        protected void CreateHtmlReport()
        {
            _xhtml = new XmlDocument();
            _root = _xhtml.CreateNode(XmlNodeType.Element, "div", null);
            _xhtml.AppendChild(_root);
        }

        protected void AddHeader(int level, string text)
        {
            _root.AppendChild(_xhtml.CreateElement($"h{level}"))
                 .AppendChild(_xhtml.CreateTextNode(text));
        }

        protected XmlNode CreateTable(IEnumerable<string> headerTexts)
        {
            var thead = _root.AppendChild(_xhtml.CreateElement("table"))
                             .AppendChild(_xhtml.CreateElement("thead"));

            foreach (string headerText in headerTexts)
            {
                thead.AppendChild(_xhtml.CreateElement("th")).AppendChild(_xhtml.CreateTextNode(headerText));
            }
            return thead.AppendChild(_xhtml.CreateElement("tbody"));
        }

        protected XmlNode CreateTable()
        {
            var thead = _root.AppendChild(_xhtml.CreateElement("table"))
                             .AppendChild(_xhtml.CreateElement("thead"));
            return thead.AppendChild(_xhtml.CreateElement("tbody"));
        }

        protected void AddTableRow(XmlNode tbody, IEnumerable<string> cellTexts)
        {
            var tr = tbody.AppendChild(_xhtml.CreateElement("tr"));
            foreach(string cellText in cellTexts)
            {
                tr.AppendChild(_xhtml.CreateElement("td")).AppendChild(_xhtml.CreateTextNode(cellText));
            }
        }

        protected virtual void WriteHtmlStyles(StreamWriter sw)
        {
            sw.WriteLine("body    { font-family:  Arial, Helvetica; font-size: 0.9em; color: #444444; }");
            sw.WriteLine("b       { color: #000000; font-weight: bold; }");
            sw.WriteLine("a       { color: #000080; }");
            sw.WriteLine("a:hover { color: #0000c0; }");
            sw.WriteLine("h1      { font-size: 1em; }");
            sw.WriteLine("h2      { font-size: .95em; }");
            sw.WriteLine("h3      { font-size: .9em; }");
            sw.WriteLine("table	{ width:100%;font-size: 0.9em; }");
            sw.WriteLine("th { text-align:left;}");
            sw.WriteLine("ul	{ font-size: 90%; }");
        }

        protected abstract void WriteReportData();
    }
}
