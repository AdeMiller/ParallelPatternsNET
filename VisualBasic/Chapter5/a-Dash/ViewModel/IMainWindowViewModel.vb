'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects

Namespace Microsoft.Practices.ParallelGuideSamples.ADash.ViewModel
    Public Interface IMainWindowViewModel
        Inherits IDisposable
        ' Events

        Event RequestAnalyzed As EventHandler
        Event RequestAnalyzedHistorical As EventHandler
        Event RequestClose As EventHandler
        Event RequestFedHistorical As EventHandler
        Event RequestMerged As EventHandler
        Event RequestModeled As EventHandler
        Event RequestModeledHistorical As EventHandler
        Event RequestNasdaq As EventHandler
        Event RequestNormalized As EventHandler
        Event RequestNormalizedHistorical As EventHandler
        Event RequestNyse As EventHandler
        Event RequestRecommendation As EventHandler

        ' Publicly Readable Data Properties

        ReadOnly Property StatusTextBoxText() As String
        ReadOnly Property IsCancelEnabled() As Boolean
        ReadOnly Property IsCalculateEnabled() As Boolean

        ReadOnly Property NyseMarketData() As StockDataCollection
        ReadOnly Property NasdaqMarketData() As StockDataCollection
        ReadOnly Property MergedMarketData() As StockDataCollection
        ReadOnly Property NormalizedMarketData() As StockDataCollection
        ReadOnly Property FedHistoricalData() As StockDataCollection
        ReadOnly Property NormalizedHistoricalData() As StockDataCollection
        ReadOnly Property AnalyzedStockData() As StockAnalysisCollection
        ReadOnly Property AnalyzedHistoricalData() As StockAnalysisCollection
        ReadOnly Property ModeledMarketData() As MarketModel
        ReadOnly Property ModeledHistoricalData() As MarketModel
        ReadOnly Property Recommendation() As MarketRecommendation

        ' Commands

        ReadOnly Property CloseCommand() As ICommand
        ReadOnly Property CalculateCommand() As ICommand
        ReadOnly Property CancelCommand() As ICommand

        ReadOnly Property NyseCommand() As ICommand
        ReadOnly Property NasdaqCommand() As ICommand
        ReadOnly Property MergedCommand() As ICommand
        ReadOnly Property NormalizedCommand() As ICommand
        ReadOnly Property FedHistoricalCommand() As ICommand
        ReadOnly Property NormalizedHistoricalCommand() As ICommand
        ReadOnly Property AnalyzedCommand() As ICommand
        ReadOnly Property AnalyzedHistoricalCommand() As ICommand
        ReadOnly Property ModeledCommand() As ICommand
        ReadOnly Property ModeledHistoricalCommand() As ICommand
        ReadOnly Property RecommendationCommand() As ICommand
    End Interface
End Namespace
