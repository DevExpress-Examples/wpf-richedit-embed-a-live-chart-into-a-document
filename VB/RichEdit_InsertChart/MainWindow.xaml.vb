Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports DevExpress.XtraRichEdit.API.Native
Imports DevExpress.XtraRichEdit

Namespace RichEdit_InsertChart
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Partial Public Class MainWindow
        Inherits Window

        Public Sub New()
            InitializeComponent()
            richEditControl1.LoadDocument("Chart.rtf")
        End Sub

        Private Sub richEditControl1_CalculateDocumentVariable(ByVal sender As Object, ByVal e As DevExpress.XtraRichEdit.CalculateDocumentVariableEventArgs)
            If e.VariableName = "CHART" Then
                Dim chart As New ChartImage(e.Arguments(0).Value.ToString())
                chart.Initialize()
                Dim image As DocumentImageSource = chart.CreateImage()
                Dim srv As New RichEditDocumentServer()
				srv.Document.Images.Append(image)
                e.Value = srv.Document
                e.Handled = True
            End If
        End Sub
    End Class
End Namespace
