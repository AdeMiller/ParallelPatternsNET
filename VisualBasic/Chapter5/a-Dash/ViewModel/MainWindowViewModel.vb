'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.ComponentModel
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects

Namespace Microsoft.Practices.ParallelGuideSamples.ADash.ViewModel
    Public Enum State
        Ready
        Calculating
        Canceling
    End Enum

    Public Class MainWindowViewModel
        Implements IMainWindowViewModel, INotifyPropertyChanged, IDisposable
#Region "Private Fields"

        Private engine As IAnalysisEngine

        ' view model's current mode of operation
        Private _modelState As State = State.Ready

        ' results of analysis or null if not yet computed
        Private _nyseMarketData As StockDataCollection
        Private _nasdaqMarketData As StockDataCollection
        Private _mergedMarketData As StockDataCollection
        Private _normalizedMarketData As StockDataCollection
        Private _fedHistoricalData As StockDataCollection
        Private _normalizedHistoricalData As StockDataCollection
        Private _analyzedStockData As StockAnalysisCollection
        Private _analyzedHistoricalData As StockAnalysisCollection
        Private _modeledMarketData As MarketModel
        Private _modeledHistoricalData As MarketModel
        Private _recommendation As MarketRecommendation

        ' Command objects exposed by this view model for use by the view
        Private _calculateCommand As Command
        Private _cancelCommand As Command
        Private _closeCommand As Command
        Private _nyseCommand As Command
        Private _nasdaqCommand As Command
        Private _mergedCommand As Command
        Private _normalizedCommand As Command
        Private _fedHistoricalCommand As Command
        Private _normalizedHistoricalCommand As Command
        Private _analyzedCommand As Command
        Private _analyzedHistoricalCommand As Command
        Private _modeledCommand As Command
        Private _modeledHistoricalCommand As Command
        Private _recommendationCommand As Command

        ' status string that appears in the UI
        Private statusText As String = ""

#End Region ' Private Fields

        Public Sub New(ByVal eng As IAnalysisEngine)
            engine = eng
        End Sub

#Region "Events"

        ' Raised when a public property of this class changes
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ' Raised when the associated view window should be closed (i.e. application shutdown).
        Public Event RequestClose As EventHandler Implements IMainWindowViewModel.RequestClose

        ' Raised when the corresponding command is invoked. 
        Public Event RequestNyse As EventHandler Implements IMainWindowViewModel.RequestNyse
        Public Event RequestNasdaq As EventHandler Implements IMainWindowViewModel.RequestNasdaq
        Public Event RequestMerged As EventHandler Implements IMainWindowViewModel.RequestMerged
        Public Event RequestNormalized As EventHandler Implements IMainWindowViewModel.RequestNormalized
        Public Event RequestFedHistorical As EventHandler Implements IMainWindowViewModel.RequestFedHistorical
        Public Event RequestNormalizedHistorical As EventHandler Implements IMainWindowViewModel.RequestNormalizedHistorical
        Public Event RequestAnalyzed As EventHandler Implements IMainWindowViewModel.RequestAnalyzed
        Public Event RequestAnalyzedHistorical As EventHandler Implements IMainWindowViewModel.RequestAnalyzedHistorical
        Public Event RequestModeled As EventHandler Implements IMainWindowViewModel.RequestModeled
        Public Event RequestModeledHistorical As EventHandler Implements IMainWindowViewModel.RequestModeledHistorical
        Public Event RequestRecommendation As EventHandler Implements IMainWindowViewModel.RequestRecommendation

#End Region

#Region "Publicly Readable Data Properties"

        Public Property ModelState() As State
            Get
                Return _modelState
            End Get

            Private Set(ByVal value As State)
                _modelState = value

                ' issue notification of property change, including derived properties
                ' and commands whose "CanExecute" status depend on model state.
                OnPropertyChanged("IsCancelEnabled")
                OnPropertyChanged("IsCalculateEnabled")
                _calculateCommand.NotifyExecuteChanged()
                _cancelCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_StatusTextBoxText() As String Implements IMainWindowViewModel.StatusTextBoxText
            Get
                Return statusText
            End Get
        End Property

        Public Property StatusTextBoxText() As String
            Get
                Return statusText
            End Get
            Private Set(ByVal value As String)
                statusText = value
                OnPropertyChanged("StatusTextBoxText")
            End Set
        End Property
        Public ReadOnly Property IsCancelEnabled() As Boolean Implements IMainWindowViewModel.IsCancelEnabled
            Get
                Return (_modelState <> State.Ready)
            End Get
        End Property

        Public ReadOnly Property IsCalculateEnabled() As Boolean Implements IMainWindowViewModel.IsCalculateEnabled
            Get
                Return (ModelState = State.Ready)
            End Get
        End Property

        Public Property NyseMarketData() As StockDataCollection
            Get
                Return _nyseMarketData
            End Get
            Private Set(ByVal value As StockDataCollection)
                _nyseMarketData = value
                OnPropertyChanged("NyseMarketData")
                _nyseCommand.NotifyExecuteChanged()
            End Set
        End Property
        Public ReadOnly Property IMainWindowViewModel_NyseMarketData() As StockDataCollection Implements IMainWindowViewModel.NyseMarketData
            Get
                Return NyseMarketData
            End Get
        End Property

        Public Property NasdaqMarketData() As StockDataCollection
            Get
                Return _nasdaqMarketData
            End Get
            Private Set(ByVal value As StockDataCollection)
                _nasdaqMarketData = value
                OnPropertyChanged("NasdaqMarketData")
                _nasdaqCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_NasdaqMarketData() As StockDataCollection Implements IMainWindowViewModel.NasdaqMarketData
            Get
                Return NasdaqMarketData
            End Get
        End Property

        Public Property MergedMarketData() As StockDataCollection
            Get
                Return _mergedMarketData
            End Get
            Private Set(ByVal value As StockDataCollection)
                _mergedMarketData = value
                OnPropertyChanged("MergedMarketData")
                _mergedCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_MergedMarketData() As StockDataCollection Implements IMainWindowViewModel.MergedMarketData
            Get
                Return MergedMarketData
            End Get
        End Property
        Public Property NormalizedMarketData() As StockDataCollection
            Get
                Return _normalizedMarketData
            End Get
            Private Set(ByVal value As StockDataCollection)
                _normalizedMarketData = value
                OnPropertyChanged("NormalizedMarketData")
                _normalizedCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_NormalizedMarketData() As StockDataCollection Implements IMainWindowViewModel.NormalizedMarketData
            Get
                Return NormalizedMarketData
            End Get
        End Property

        Public Property FedHistoricalData() As StockDataCollection
            Get
                Return _fedHistoricalData
            End Get
            Private Set(ByVal value As StockDataCollection)
                _fedHistoricalData = value
                OnPropertyChanged("FedHistoricalData")
                _fedHistoricalCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_FedHistoricalData() As StockDataCollection Implements IMainWindowViewModel.FedHistoricalData
            Get
                Return FedHistoricalData
            End Get
        End Property

        Public Property NormalizedHistoricalData() As StockDataCollection
            Get
                Return _normalizedHistoricalData
            End Get
            Private Set(ByVal value As StockDataCollection)
                _normalizedHistoricalData = value
                OnPropertyChanged("NormalizedHistoricalData")
                _normalizedHistoricalCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_NormalizedHistoricalData() As StockDataCollection Implements IMainWindowViewModel.NormalizedHistoricalData
            Get
                Return NormalizedHistoricalData
            End Get
        End Property

        Public Property AnalyzedStockData() As StockAnalysisCollection
            Get
                Return _analyzedStockData
            End Get
            Private Set(ByVal value As StockAnalysisCollection)
                _analyzedStockData = value
                OnPropertyChanged("AnalyzedMarketData")
                _analyzedCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_AnalyzedStockData() As StockAnalysisCollection Implements IMainWindowViewModel.AnalyzedStockData
            Get
                Return AnalyzedStockData
            End Get
        End Property

        Public Property AnalyzedHistoricalData() As StockAnalysisCollection
            Get
                Return _analyzedHistoricalData
            End Get
            Private Set(ByVal value As StockAnalysisCollection)
                _analyzedHistoricalData = value
                OnPropertyChanged("AnalyzedHistoricalData")
                _analyzedHistoricalCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_AnalyzedHistoricalData() As StockAnalysisCollection Implements IMainWindowViewModel.AnalyzedHistoricalData
            Get
                Return AnalyzedHistoricalData
            End Get
        End Property

        Public Property ModeledMarketData() As MarketModel
            Get
                Return _modeledMarketData
            End Get
            Private Set(ByVal value As MarketModel)
                _modeledMarketData = value
                OnPropertyChanged("ModeledMarketData")
                _modeledCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_ModeledMarketData() As MarketModel Implements IMainWindowViewModel.ModeledMarketData
            Get
                Return ModeledMarketData
            End Get
        End Property

        Public Property ModeledHistoricalData() As MarketModel
            Get
                Return _modeledHistoricalData
            End Get
            Private Set(ByVal value As MarketModel)
                _modeledHistoricalData = value
                OnPropertyChanged("ModeledHistoricalData")
                _modeledHistoricalCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_ModeledHistoricalData() As MarketModel Implements IMainWindowViewModel.ModeledHistoricalData
            Get
                Return ModeledHistoricalData
            End Get
        End Property

        Public Property Recommendation() As MarketRecommendation
            Get
                Return _recommendation
            End Get
            Private Set(ByVal value As MarketRecommendation)
                _recommendation = value
                OnPropertyChanged("Recommendation")
                _recommendationCommand.NotifyExecuteChanged()
            End Set
        End Property

        Public ReadOnly Property IMainWindowViewModel_Recommendation() As MarketRecommendation Implements IMainWindowViewModel.Recommendation
            Get
                Return Recommendation
            End Get
        End Property

#End Region ' Publicly Readable Data Properties

#Region "Commands"

        Public ReadOnly Property CloseCommand() As ICommand Implements IMainWindowViewModel.CloseCommand
            Get
                If _closeCommand Is Nothing Then
                    _closeCommand = New Command(Sub(x)
                                                    OnRequestClose()
                                                End Sub)
                End If
                Return _closeCommand
            End Get
        End Property

        Public ReadOnly Property CalculateCommand() As ICommand Implements IMainWindowViewModel.CalculateCommand
            Get
                If _calculateCommand Is Nothing Then
                    _calculateCommand = New Command(Sub(x)
                                                        OnRequestCalculate()
                                                    End Sub)
                End If
                Return _calculateCommand
            End Get
        End Property

        Public ReadOnly Property CancelCommand() As ICommand Implements IMainWindowViewModel.CancelCommand
            Get
                If _cancelCommand Is Nothing Then
                    _cancelCommand = New Command(Sub(x)
                                                     OnRequestCancel()
                                                 End Sub, Function(x)
                                                              Return ModelState = State.Calculating
                                                          End Function)
                End If
                Return _cancelCommand
            End Get
        End Property

        Public ReadOnly Property NyseCommand() As ICommand Implements IMainWindowViewModel.NyseCommand
            Get
                If _nyseCommand Is Nothing Then
                    _nyseCommand = New Command(Sub(x)
                                                   [RaiseEvent](RequestNyseEvent)
                                               End Sub, Function(x)
                                                            Return _nyseMarketData IsNot Nothing
                                                        End Function)
                End If
                Return _nyseCommand
            End Get
        End Property

        Public ReadOnly Property NasdaqCommand() As ICommand Implements IMainWindowViewModel.NasdaqCommand
            Get
                If _nasdaqCommand Is Nothing Then
                    _nasdaqCommand = New Command(Sub(x)
                                                     [RaiseEvent](RequestNasdaqEvent)
                                                 End Sub, Function(x)
                                                              Return _nasdaqMarketData IsNot Nothing
                                                          End Function)
                End If

                Return _nasdaqCommand
            End Get
        End Property

        Public ReadOnly Property MergedCommand() As ICommand Implements IMainWindowViewModel.MergedCommand
            Get
                If _mergedCommand Is Nothing Then
                    _mergedCommand = New Command(Sub(x)
                                                     [RaiseEvent](RequestMergedEvent)
                                                 End Sub, Function(x)
                                                              Return _mergedMarketData IsNot Nothing
                                                          End Function)
                End If

                Return _mergedCommand
            End Get
        End Property

        Public ReadOnly Property NormalizedCommand() As ICommand Implements IMainWindowViewModel.NormalizedCommand
            Get
                If _normalizedCommand Is Nothing Then
                    _normalizedCommand = New Command(Sub(x)
                                                         [RaiseEvent](RequestNormalizedEvent)
                                                     End Sub, Function(x)
                                                                  Return _normalizedMarketData IsNot Nothing
                                                              End Function)
                End If
                Return _normalizedCommand
            End Get
        End Property

        Public ReadOnly Property FedHistoricalCommand() As ICommand Implements IMainWindowViewModel.FedHistoricalCommand
            Get
                If _fedHistoricalCommand Is Nothing Then
                    _fedHistoricalCommand = New Command(Sub(x)
                                                            [RaiseEvent](RequestFedHistoricalEvent)
                                                        End Sub, Function(x)
                                                                     Return _fedHistoricalData IsNot Nothing
                                                                 End Function)
                End If

                Return _fedHistoricalCommand
            End Get
        End Property

        Public ReadOnly Property NormalizedHistoricalCommand() As ICommand Implements IMainWindowViewModel.NormalizedHistoricalCommand
            Get
                If _normalizedHistoricalCommand Is Nothing Then
                    _normalizedHistoricalCommand = New Command(Sub(x)
                                                                   [RaiseEvent](RequestNormalizedHistoricalEvent)
                                                               End Sub, Function(x)
                                                                            Return _normalizedHistoricalData IsNot Nothing
                                                                        End Function)
                End If

                Return _normalizedHistoricalCommand
            End Get
        End Property

        Public ReadOnly Property AnalyzedCommand() As ICommand Implements IMainWindowViewModel.AnalyzedCommand
            Get
                If _analyzedCommand Is Nothing Then
                    _analyzedCommand = New Command(Sub(x)
                                                       [RaiseEvent](RequestAnalyzedEvent)
                                                   End Sub, Function(x)
                                                                Return _analyzedStockData IsNot Nothing
                                                            End Function)
                End If

                Return _analyzedCommand
            End Get
        End Property

        Public ReadOnly Property AnalyzedHistoricalCommand() As ICommand Implements IMainWindowViewModel.AnalyzedHistoricalCommand
            Get
                If _analyzedHistoricalCommand Is Nothing Then
                    _analyzedHistoricalCommand = New Command(Sub(x)
                                                                 [RaiseEvent](RequestAnalyzedHistoricalEvent)
                                                             End Sub, Function(x)
                                                                          Return _analyzedHistoricalData IsNot Nothing
                                                                      End Function)
                End If

                Return _analyzedHistoricalCommand
            End Get
        End Property

        Public ReadOnly Property ModeledCommand() As ICommand Implements IMainWindowViewModel.ModeledCommand
            Get
                If _modeledCommand Is Nothing Then
                    _modeledCommand = New Command(Sub(x)
                                                      [RaiseEvent](RequestModeledEvent)
                                                  End Sub, Function(x)
                                                               Return _modeledMarketData IsNot Nothing
                                                           End Function)
                End If

                Return _modeledCommand
            End Get
        End Property

        Public ReadOnly Property ModeledHistoricalCommand() As ICommand Implements IMainWindowViewModel.ModeledHistoricalCommand
            Get
                If _modeledHistoricalCommand Is Nothing Then
                    _modeledHistoricalCommand = New Command(Sub(x)
                                                                [RaiseEvent](RequestModeledHistoricalEvent)
                                                            End Sub, Function(x)
                                                                         Return _modeledHistoricalData IsNot Nothing
                                                                     End Function)
                End If

                Return _modeledHistoricalCommand
            End Get
        End Property

        Public ReadOnly Property RecommendationCommand() As ICommand Implements IMainWindowViewModel.RecommendationCommand
            Get
                If _recommendationCommand Is Nothing Then
                    _recommendationCommand = New Command(Sub(x)
                                                             [RaiseEvent](RequestRecommendationEvent)
                                                         End Sub, Function(x)
                                                                      Return _recommendation IsNot Nothing
                                                                  End Function)
                End If

                Return _recommendationCommand
            End Get
        End Property

#End Region ' Commands

#Region "Command Implementations"

        ' helper
        Private Sub [RaiseEvent](ByVal handler As EventHandler)
            If handler IsNot Nothing Then
                handler(Me, EventArgs.Empty)
            End If
        End Sub

        Private Sub OnRequestCalculate()
            ' Initialize the result properties to null
            ResetResultProperties()

            ' Place the view model into calculation mode
            ModelState = State.Calculating

            ' Update the property containing the status text
            StatusTextBoxText = "...calculating..."

            ' Start the analysis
            Dim tasks As AnalysisTasks = engine.DoAnalysisParallel()

            ' Add continuations so that view model properties are updated when each subtask completes
            AddButtonContinuations(tasks)
        End Sub

        Private Sub AddButtonContinuation(Of T)(ByVal task As Task(Of T), ByVal action As Action(Of Task(Of T)))
            task.ContinueWith(action, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext())

        End Sub
        ''' <summary>
        ''' Adds continuations to analysis tasks so that the view model's properties are updated when 
        ''' each task has results available for display.
        ''' </summary>
        ''' <param name="tasks">The task record</param>
        Private Sub AddButtonContinuations(ByVal tasks As AnalysisTasks)
            AddButtonContinuation(tasks.LoadNyseData, Sub(t)
                                                          NyseMarketData = t.Result
                                                      End Sub)

            AddButtonContinuation(tasks.LoadNasdaqData, Sub(t)
                                                            NasdaqMarketData = t.Result
                                                        End Sub)

            AddButtonContinuation(tasks.LoadFedHistoricalData, Sub(t)
                                                                   FedHistoricalData = t.Result
                                                               End Sub)

            AddButtonContinuation(tasks.MergeMarketData, Sub(t)
                                                             MergedMarketData = t.Result
                                                         End Sub)

            AddButtonContinuation(tasks.NormalizeHistoricalData, Sub(t)
                                                                     NormalizedHistoricalData = t.Result
                                                                 End Sub)

            AddButtonContinuation(tasks.NormalizeMarketData, Sub(t)
                                                                 NormalizedMarketData = t.Result
                                                             End Sub)

            AddButtonContinuation(tasks.AnalyzeHistoricalData, Sub(t)
                                                                   AnalyzedHistoricalData = t.Result
                                                               End Sub)

            AddButtonContinuation(tasks.AnalyzeMarketData, Sub(t)
                                                               AnalyzedStockData = t.Result
                                                           End Sub)

            AddButtonContinuation(tasks.ModelHistoricalData, Sub(t)
                                                                 ModeledHistoricalData = t.Result
                                                             End Sub)

            AddButtonContinuation(tasks.ModelMarketData, Sub(t)
                                                             ModeledMarketData = t.Result
                                                         End Sub)

            AddButtonContinuation(tasks.CompareModels, Sub(t)
                                                           Me.Recommendation = t.Result
                                                           Me.StatusTextBoxText = If((Me.Recommendation Is Nothing), "Canceled", Me.Recommendation.Value)
                                                           Me.ModelState = State.Ready
                                                       End Sub)

            tasks.ErrorHandler.ContinueWith(Sub(t)
                                                If t.Status = TaskStatus.Faulted Then
                                                    Me.StatusTextBoxText = "Error"
                                                End If
                                                Me.ModelState = State.Ready
                                            End Sub, TaskScheduler.FromCurrentSynchronizationContext())
        End Sub


        Private Sub ResetResultProperties()
            NyseMarketData = Nothing
            NasdaqMarketData = Nothing
            MergedMarketData = Nothing
            NormalizedMarketData = Nothing
            FedHistoricalData = Nothing
            NormalizedHistoricalData = Nothing
            AnalyzedStockData = Nothing
            AnalyzedHistoricalData = Nothing
            ModeledMarketData = Nothing
            ModeledHistoricalData = Nothing
            Recommendation = Nothing
        End Sub

        Private Sub OnRequestClose()
            engine.TryCancelAnalysis()
            [RaiseEvent](RequestCloseEvent)
        End Sub

        Private Sub OnRequestCancel()
            engine.TryCancelAnalysis()
            ModelState = State.Canceling
            StatusTextBoxText = "Canceling..."
        End Sub

#End Region ' Command Implementations

#Region "INotifyPropertyChanged Implementation"

        Protected Sub OnPropertyChanged(ByVal name As String)
            Dim handler As PropertyChangedEventHandler = PropertyChangedEvent
            If handler IsNot Nothing Then
                handler(Me, New PropertyChangedEventArgs(name))
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
                If engine IsNot Nothing Then
                    engine.Dispose()
                    engine = Nothing
                End If
            End If
        End Sub

#End Region
    End Class
End Namespace
