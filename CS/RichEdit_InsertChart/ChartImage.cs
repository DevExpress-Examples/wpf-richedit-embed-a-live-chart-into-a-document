namespace RichEdit_InsertChart{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DevExpress.XtraRichEdit.Fields;
    using DevExpress.XtraRichEdit.Model;
    using System.IO;
    using DevExpress.XtraCharts;
    using System.Drawing.Imaging;
    using DevExpress.XtraRichEdit.Utils;
    using System.Text;
    using System.Xml;
    using DevExpress.XtraRichEdit.API.Native;

    public class ChartImage {


        string _Input;
        DevExpress.XtraCharts.ViewType _ViewType = DevExpress.XtraCharts.ViewType.Pie;
        Nullable<int> _Width = null;
        Nullable<int> _Height = null;
        DevExpress.Utils.DefaultBoolean _ShowLegend = DevExpress.Utils.DefaultBoolean.False;
        List<KeyValuePair<string, double>> _Data = new List<KeyValuePair<string, double>>();

        public DevExpress.XtraCharts.ViewType ViewType { get { return this._ViewType; } }
        public Nullable<int> Width { get { return this._Width; } }
        public Nullable<int> Height { get { return this._Height; } }
        public DevExpress.Utils.DefaultBoolean ShowLegend { get { return this._ShowLegend; } }
        public List<KeyValuePair<string, double>> Data { get { return _Data; } }      
        
        
        public ChartImage(string input)
        {
            this._Input = input;
        }

        public void Initialize()
        {
            using (StringReader stringReader = new StringReader(_Input))
            using (XmlTextReader reader = new XmlTextReader(stringReader)) {
                reader.DtdProcessing = DtdProcessing.Ignore;
                while (reader.Read()) {
                    if (reader.IsStartElement()) {
                        switch (reader.Name) {
                            case "type":
                                GetChartType(reader.ReadString());
                                break;
                            case "height":
                                _Height = new int?(int.Parse(reader.ReadString()));
                                break;
                            case "width":
                                _Width = new int?(int.Parse(reader.ReadString()));
                                break;
                            case "legend":
                                _ShowLegend = (reader.ReadString().ToLower() == "true") ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.False;
                                break;
                            case "data":
                                GetData(reader.ReadString());
                                break;
                        }
                    }
                }
            }
        }

        private void GetData(string p)
        {
            foreach (string t in p.Split(',')) {
                double value;

                string[] pair = t.Split('|');

                if (pair.Length == 0) {
                    continue;
                }

                if (pair.Length == 1) {
                    if (!double.TryParse(pair[0], out value)) {
                        _Data.Add(new KeyValuePair<string, double>(pair[0], 0));
                    }
                    else {
                        _Data.Add(new KeyValuePair<string, double>(String.Empty, value));
                    }
                }
                else
                    if (pair.Length == 2) {
                        if (!double.TryParse(pair[1], out value)) {
                            _Data.Add(new KeyValuePair<string, double>(pair[0], 0));
                        }
                        else {
                            _Data.Add(new KeyValuePair<string, double>(pair[0], value));
                        }
                    }
            }
        }

        private void GetChartType(string p)
        {
            switch (p.ToLower()) {
                case "bar":
                    _ViewType = DevExpress.XtraCharts.ViewType.Bar;
                    break;
                case "line":
                    _ViewType = DevExpress.XtraCharts.ViewType.Line;
                    break;
                case "pie3d":
                    _ViewType = DevExpress.XtraCharts.ViewType.Pie3D;
                    break;
                case "pie":
                default:
                    _ViewType = DevExpress.XtraCharts.ViewType.Pie;
                    break;
            }
        }


        public Stream CreateChart() {

            using (ChartControl chart = new ChartControl()) {
                if (_Width.HasValue) {
                    chart.Width = _Width.Value;
                }
                if (_Height.HasValue) {
                    chart.Height = _Height.Value;
                }

                int undefined = 1;
                MemoryStream stream = new MemoryStream();
                try {
                    Series series = new Series("Chart", _ViewType);
                    try {
                        if (series.Label is DevExpress.XtraCharts.PieSeriesLabel) {
                            ((DevExpress.XtraCharts.PieSeriesLabel)series.Label).Position = PieSeriesLabelPosition.Inside;
                        }

                        if (_ViewType == ViewType.Pie) {
                            series.Label.TextPattern = "{A}: {VP:P0}";
                        }

                        if (_Data == null || _Data.Count == 0) {
                            series.Points.Add(new SeriesPoint("Undefined", new double[] { 1 }));
                            series.Label.TextPattern = "{S}";

                        } else {
                            series.Label.TextPattern = "{A}: {V:F1}";

                            for (int i = 0; i < _Data.Count; i++) {

                                string argument = _Data[i].Key.Trim();

                                if (String.IsNullOrEmpty(argument)) {
                                    argument = "Undefined " + undefined;
                                    undefined++;
                                }

                                series.Points.Add(new SeriesPoint(argument, new double[] {
                                    _Data[i].Value }));
                            }
                        }

                        chart.Legend.Visibility = _ShowLegend;
                        chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;

                        chart.Series.AddRange(new Series[] { series });
                        series = null;

                        XYDiagram diagram = chart.Diagram as XYDiagram;
                        if (diagram != null && diagram.AxisX != null)
                            diagram.AxisX.Label.ResolveOverlappingOptions.AllowStagger = true; ;

                        chart.ExportToImage(stream, ImageFormat.Png);
                        stream.Position = 0;

                        return stream;

                    } catch {
                        if (series != null) {
                            series.Dispose();
                        }
                        throw;
                    }

                } catch {
                    if (stream != null) {
                        stream.Dispose();
                    }
                    throw;
                }
            }
        }

        public DocumentImageSource CreateImage()
        {
            return DocumentImageSource.FromStream(CreateChart());
        }

    }
}
