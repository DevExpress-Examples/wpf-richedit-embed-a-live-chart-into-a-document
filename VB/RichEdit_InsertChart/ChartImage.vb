Imports DevExpress.XtraRichEdit.API.Native
Imports System.Xml
Imports System.Text
Imports DevExpress.XtraRichEdit.Utils
Imports System.Drawing.Imaging
Imports DevExpress.XtraCharts
Imports System.IO
Imports DevExpress.XtraRichEdit.Model
Imports DevExpress.XtraRichEdit.Fields
Imports System.Linq
Imports System.Collections.Generic
Imports System
Imports DevExpress.Utils

Namespace RichEdit_InsertChart

	Public Class ChartImage


		Private _Input As String
		Private _ViewType As DevExpress.XtraCharts.ViewType = DevExpress.XtraCharts.ViewType.Pie
		Private _Width? As Integer = Nothing
		Private _Height? As Integer = Nothing
		Private _ShowLegend As DefaultBoolean = DefaultBoolean.False
		Private _Data As New List(Of KeyValuePair(Of String, Double))()

		Public ReadOnly Property ViewType() As DevExpress.XtraCharts.ViewType
			Get
				Return Me._ViewType
			End Get
		End Property
		Public ReadOnly Property Width() As Integer?
			Get
				Return Me._Width
			End Get
		End Property
		Public ReadOnly Property Height() As Integer?
			Get
				Return Me._Height
			End Get
		End Property
		Public ReadOnly Property ShowLegend() As DefaultBoolean
			Get
				Return Me._ShowLegend
			End Get
		End Property
		Public ReadOnly Property Data() As List(Of KeyValuePair(Of String, Double))
			Get
				Return _Data
			End Get
		End Property


		Public Sub New(ByVal input As String)
			Me._Input = input
		End Sub

		Public Sub Initialize()
			Using stringReader As New StringReader(_Input)
				Using reader As New XmlTextReader(stringReader)
					reader.DtdProcessing = DtdProcessing.Ignore
					Do While reader.Read()
						If reader.IsStartElement() Then
							Select Case reader.Name
								Case "type"
									GetChartType(reader.ReadString())
								Case "height"
									_Height = New Integer?(Integer.Parse(reader.ReadString()))
								Case "width"
									_Width = New Integer?(Integer.Parse(reader.ReadString()))
								Case "legend"
									_ShowLegend = If((reader.ReadString().ToLower() = "true"), DefaultBoolean.True, DefaultBoolean.False)
								Case "data"
									GetData(reader.ReadString())
							End Select
						End If
					Loop
				End Using
			End Using
		End Sub

		Private Sub GetData(ByVal p As String)
			For Each t As String In p.Split(","c)
				Dim value As Double

				Dim pair() As String = t.Split("|"c)

				If pair.Length = 0 Then
					Continue For
				End If

				If pair.Length = 1 Then
					If Not Double.TryParse(pair(0), value) Then
						_Data.Add(New KeyValuePair(Of String, Double)(pair(0), 0))
					Else
						_Data.Add(New KeyValuePair(Of String, Double)(String.Empty, value))
					End If
				Else
					If pair.Length = 2 Then
						If Not Double.TryParse(pair(1), value) Then
							_Data.Add(New KeyValuePair(Of String, Double)(pair(0), 0))
						Else
							_Data.Add(New KeyValuePair(Of String, Double)(pair(0), value))
						End If
					End If
				End If
			Next t
		End Sub

		Private Sub GetChartType(ByVal p As String)
			Select Case p.ToLower()
				Case "bar"
					_ViewType = DevExpress.XtraCharts.ViewType.Bar
				Case "line"
					_ViewType = DevExpress.XtraCharts.ViewType.Line
				Case "pie3d"
					_ViewType = DevExpress.XtraCharts.ViewType.Pie3D
				Case Else
					_ViewType = DevExpress.XtraCharts.ViewType.Pie
			End Select
		End Sub


		Public Function CreateChart() As Stream

			Using chart As New ChartControl()
				If _Width.HasValue Then
					chart.Width = _Width.Value
				End If
				If _Height.HasValue Then
					chart.Height = _Height.Value
				End If

				Dim undefined As Integer = 1
				Dim stream As New MemoryStream()
				Try
					Dim series As New Series("Chart", _ViewType)
					Try
						If TypeOf series.Label Is DevExpress.XtraCharts.PieSeriesLabel Then
							CType(series.Label, DevExpress.XtraCharts.PieSeriesLabel).Position = PieSeriesLabelPosition.Inside
						End If

						If _ViewType = ViewType.Pie Then
							series.Label.TextPattern = "{A}: {VP:P0}"
						End If

						If _Data Is Nothing OrElse _Data.Count = 0 Then
							series.Points.Add(New SeriesPoint("Undefined", New Double() {1}))
							series.Label.TextPattern = "{S}"

						Else
							series.Label.TextPattern = "{A}: {V:F1}"

							For i As Integer = 0 To _Data.Count - 1

								Dim argument As String = _Data(i).Key.Trim()

								If String.IsNullOrEmpty(argument) Then
									argument = "Undefined " & undefined
									undefined += 1
								End If

								series.Points.Add(New SeriesPoint(argument, New Double() {_Data(i).Value}))
							Next i
						End If

						chart.Legend.Visibility = _ShowLegend
						chart.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False

						chart.Series.AddRange(New Series() {series})
						series = Nothing

						Dim diagram As XYDiagram = TryCast(chart.Diagram, XYDiagram)
						If diagram IsNot Nothing AndAlso diagram.AxisX IsNot Nothing Then
							diagram.AxisX.Label.ResolveOverlappingOptions.AllowStagger = True
						End If


						chart.ExportToImage(stream, ImageFormat.Png)
						stream.Position = 0

						Return stream

					Catch
						If series IsNot Nothing Then
							series.Dispose()
						End If
						Throw
					End Try

				Catch
					If stream IsNot Nothing Then
						stream.Dispose()
					End If
					Throw
				End Try
			End Using
		End Function

		Public Function CreateImage() As DocumentImageSource
			Return DocumentImageSource.FromStream(CreateChart())
		End Function

	End Class
End Namespace
