'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects

Namespace Microsoft.Practices.ParallelGuideSamples.ADash
    ''' <summary>
    ''' Task record for market analysis. 
    ''' </summary>
    ''' <remarks>
    ''' Call CompareModels.Result to retrieve the finished analysis. Intermediate results can be retrieved
    ''' from the Result method of the other properties. For example, LoadNyseData.Result returns the NYSE data.
    ''' </remarks>
    Public Class AnalysisTasks
        Public Property LoadNyseData As Task(Of StockDataCollection)
        Public Property LoadNasdaqData() As Task(Of StockDataCollection)
        Public Property MergeMarketData() As Task(Of StockDataCollection)
        Public Property NormalizeMarketData() As Task(Of StockDataCollection)
        Public Property LoadFedHistoricalData() As Task(Of StockDataCollection)
        Public Property NormalizeHistoricalData() As Task(Of StockDataCollection)
        Public Property AnalyzeMarketData() As Task(Of StockAnalysisCollection)
        Public Property AnalyzeHistoricalData() As Task(Of StockAnalysisCollection)
        Public Property ModelMarketData() As Task(Of MarketModel)
        Public Property ModelHistoricalData() As Task(Of MarketModel)
        Public Property CompareModels() As Task(Of MarketRecommendation)
        Public Property ErrorHandler() As Task
    End Class
End Namespace