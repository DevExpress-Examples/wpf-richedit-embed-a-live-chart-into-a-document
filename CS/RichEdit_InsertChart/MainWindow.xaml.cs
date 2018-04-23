using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit;

namespace RichEdit_InsertChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            richEditControl1.LoadDocument("Chart.rtf");
        }

        private void richEditControl1_CalculateDocumentVariable(object sender, DevExpress.XtraRichEdit.CalculateDocumentVariableEventArgs e)
        {
            if (e.VariableName == "CHART") {
                ChartImage chart = new ChartImage(e.Arguments[0].Value.ToString());
                chart.Initialize();
                DocumentImageSource image = chart.CreateImage();
                RichEditDocumentServer srv = new RichEditDocumentServer();
                srv.Document.Images.Append(image);
                e.Value = srv.Document;
                e.Handled = true;
            }
        }
    }
}
