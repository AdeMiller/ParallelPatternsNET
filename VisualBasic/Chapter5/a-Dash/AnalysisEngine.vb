'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.ADash
    ''' <summary>
    ''' Component for data analysis
    ''' </summary>
    Public Class AnalysisEngine
        Implements IAnalysisEngine
        ' internal scaling factor
        Private ReadOnly speedFactor As Double

        Private cts As CancellationTokenSource = Nothing

        Public Sub New()
            Me.New(1.0R)
        End Sub

        Public Sub New(ByVal speed As Double)
            speedFactor = speed
        End Sub

#Region "CreateSampleData"

        Private Shared Function MakeNyseSecurityInfo() As IList(Of StockData)
            Return GenerateSecurities("NYSE", 100)
        End Function

        Private Shared Function MakeNasdaqSecurityInfo() As IList(Of StockData)
            Return GenerateSecurities("NASDAQ", 100)
        End Function

        Private Shared Function MakeFedSecurityInfo() As IList(Of StockData)
            Return GenerateSecurities("", 100)
        End Function

        Private Shared Function GenerateSecurities(ByVal exchange As String, ByVal size As Integer) As IList(Of StockData)
            Dim result = New List(Of StockData)()
            For i As Integer = 0 To size - 1
                result.Add(New StockData(exchange & " Stock " & i, New Double() {0.0R, 1.0R, 2.0R}))
            Next i
            Return result
        End Function

#End Region

#Region "Analysis Helper Methods"

        Private Function LoadNyseData() As StockDataCollection
            SampleUtilities.DoIoIntensiveOperation(2.5, cts.Token)
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            End If
            Return New StockDataCollection(MakeNyseSecurityInfo())
        End Function

        Private Function LoadNasdaqData() As StockDataCollection
            SampleUtilities.DoIoIntensiveOperation(2.0 * speedFactor, cts.Token)
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            End If

            Return New StockDataCollection(MakeNasdaqSecurityInfo())
        End Function

        Private Function LoadFedHistoricalData() As StockDataCollection
            SampleUtilities.DoIoIntensiveOperation(3.0 * speedFactor, cts.Token)
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            End If

            Return New StockDataCollection(MakeFedSecurityInfo())
        End Function

        Private Function MergeMarketData(ByVal allMarketData As IEnumerable(Of StockDataCollection)) As StockDataCollection
            SampleUtilities.DoCpuIntensiveOperation(2.0 * speedFactor, cts.Token)
            Dim securities = New List(Of StockData)()

            If Not cts.Token.IsCancellationRequested Then
                For Each md As StockDataCollection In allMarketData
                    securities.AddRange(md)
                Next md
            End If

            If cts.Token.IsCancellationRequested Then
                Return Nothing
            Else
                Return New StockDataCollection(securities)
            End If
        End Function

        ''' <summary>
        ''' Normalize stock data.
        ''' </summary>
        Private Function NormalizeData(ByVal marketData As StockDataCollection) As StockDataCollection
            SampleUtilities.DoCpuIntensiveOperation(2.0 * speedFactor, cts.Token)
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            Else
                Return New StockDataCollection(marketData)
            End If
        End Function

        Private Function AnalyzeData(ByVal data As StockDataCollection) As StockAnalysisCollection
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            End If
            Return MarketAnalyzer.Run(data)
        End Function

        Private Function RunModel(ByVal data As StockAnalysisCollection) As MarketModel
            SampleUtilities.DoCpuIntensiveOperation(2.0 * speedFactor, cts.Token)
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            Else
                Return MarketModeler.Run(data)
            End If
        End Function

        Private Function CompareModels(ByVal models As IEnumerable(Of MarketModel)) As MarketRecommendation
            SampleUtilities.DoCpuIntensiveOperation(2.0 * speedFactor, cts.Token)
            If cts.Token.IsCancellationRequested Then
                Return Nothing
            Else
                Return ModelComparer.Run(models.ToArray())
            End If
        End Function

#End Region

#Region "Analysis Public Methods"

        ''' <summary>
        ''' Creates a market recommendation using a fully sequential operation
        ''' </summary>
        ''' <returns>A market recommendation</returns>
        Public Function DoAnalysisSequential() As MarketRecommendation Implements IAnalysisEngine.DoAnalysisSequential
            Dim nyseData As StockDataCollection = LoadNyseData()
            Dim nasdaqData As StockDataCollection = LoadNasdaqData()
            Dim mergedMarketData As StockDataCollection = MergeMarketData(New StockDataCollection() {nyseData, nasdaqData})
            Dim normalizedMarketData As StockDataCollection = NormalizeData(mergedMarketData)
            Dim fedHistoricalData As StockDataCollection = LoadFedHistoricalData()
            Dim normalizedHistoricalData As StockDataCollection = NormalizeData(fedHistoricalData)
            Dim analyzedStockData As StockAnalysisCollection = AnalyzeData(normalizedMarketData)
            Dim modeledMarketData As MarketModel = RunModel(analyzedStockData)
            Dim analyzedHistoricalData As StockAnalysisCollection = AnalyzeData(normalizedHistoricalData)
            Dim modeledHistoricalData As MarketModel = RunModel(analyzedHistoricalData)
            Dim recommendation As MarketRecommendation = CompareModels(New MarketModel() {modeledMarketData, modeledHistoricalData})
            Return recommendation
        End Function

        ''' <summary>
        ''' Initiates market analysis using parallel computation.
        ''' </summary>        
        ''' <returns>Task record that may be queried for results of the analysis</returns>
        ''' <remarks>Compare with the DoAnalysisSequential method</remarks>
        Public Function DoAnalysisParallel() As AnalysisTasks Implements IAnalysisEngine.DoAnalysisParallel
            Dim factory As TaskFactory = Task.Factory

            If cts IsNot Nothing Then
                cts.Dispose()
                cts = Nothing
            End If
            cts = New CancellationTokenSource()

            Dim loadNyseData As Task(Of StockDataCollection) = Task(Of StockDataCollection).Factory.StartNew(Function() Me.LoadNyseData(), TaskCreationOptions.LongRunning)

            Dim loadNasdaqData As Task(Of StockDataCollection) = Task(Of StockDataCollection).Factory.StartNew(Function() Me.LoadNasdaqData(), TaskCreationOptions.LongRunning)

            Dim mergeMarketData As Task(Of StockDataCollection) = factory.ContinueWhenAll(Of StockDataCollection, StockDataCollection)(New Task(Of StockDataCollection)() {loadNyseData, loadNasdaqData}, Function(tasks) Me.MergeMarketData( _
                From t In tasks _
                Select t.Result))

            Dim normalizeMarketData As Task(Of StockDataCollection) = mergeMarketData.ContinueWith(Function(t) NormalizeData(t.Result))

            Dim loadFedHistoricalData As Task(Of StockDataCollection) = Task(Of StockDataCollection).Factory.StartNew(Function() Me.LoadFedHistoricalData(), TaskCreationOptions.LongRunning)

            Dim normalizeHistoricalData As Task(Of StockDataCollection) = loadFedHistoricalData.ContinueWith(Function(t) NormalizeData(t.Result))

            Dim analyzeMarketData As Task(Of StockAnalysisCollection) = normalizeMarketData.ContinueWith(Function(t) AnalyzeData(t.Result))

            Dim modelMarketData As Task(Of MarketModel) = analyzeMarketData.ContinueWith(Function(t) RunModel(t.Result))

            Dim analyzeHistoricalData As Task(Of StockAnalysisCollection) = normalizeHistoricalData.ContinueWith(Function(t) AnalyzeData(t.Result))

            Dim modelHistoricalData As Task(Of MarketModel) = analyzeHistoricalData.ContinueWith(Function(t) RunModel(t.Result))

            Dim compareModels As Task(Of MarketRecommendation) = factory.ContinueWhenAll(Of MarketModel, MarketRecommendation)(New Task(Of MarketModel)() {modelMarketData, modelHistoricalData}, Function(tasks) Me.CompareModels( _
                From t In tasks _
                Select t.Result))

            Dim errorHandler As Task = CreateErrorHandler(loadNyseData, loadNasdaqData, loadFedHistoricalData, mergeMarketData, normalizeHistoricalData, normalizeMarketData, analyzeHistoricalData, analyzeMarketData, modelHistoricalData, modelMarketData, compareModels)

            Return New AnalysisTasks() With {.LoadNyseData = loadNyseData, .LoadNasdaqData = loadNasdaqData, .MergeMarketData = mergeMarketData, .NormalizeMarketData = normalizeMarketData, .LoadFedHistoricalData = loadFedHistoricalData, .NormalizeHistoricalData = normalizeHistoricalData, .AnalyzeMarketData = analyzeMarketData, .AnalyzeHistoricalData = analyzeHistoricalData, .ModelMarketData = modelMarketData, .ModelHistoricalData = modelHistoricalData, .CompareModels = compareModels, .ErrorHandler = errorHandler}
        End Function

        Private Function CreateErrorHandler(ByVal ParamArray tasks() As Task) As Task
            Return Task.Factory.ContinueWhenAll(tasks, Sub(t)
                                                           Try
                                                               Task.WaitAll(tasks)
                                                           Catch e As AggregateException
                                                               Console.WriteLine(e.Flatten())
                                                           End Try
                                                       End Sub)
        End Function


        Public Sub TryCancelAnalysis() Implements IAnalysisEngine.TryCancelAnalysis
            If cts IsNot Nothing Then
                cts.Cancel()
            End If
        End Sub

#End Region

#Region "IDisposable Members"

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If cts IsNot Nothing Then
                    cts.Dispose()
                    cts = Nothing
                End If
            End If
        End Sub

#End Region
    End Class
End Namespace
