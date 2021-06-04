'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.ADash.ViewModel

Namespace Microsoft.Practices.ParallelGuideSamples.ADash
    Partial Public Class App
        Inherits Application
        Private viewModel As IMainWindowViewModel

        Protected Overrides Sub OnStartup(ByVal e As StartupEventArgs)
            MyBase.OnStartup(e)

            ' Create the View.
            Dim window As New MainWindow()

            ' Create Model.
            Dim engine As IAnalysisEngine = New AnalysisEngine()

            ' Create ViewModel for main window View, pass it a reference to the Model.
            viewModel = New MainWindowViewModel(engine)

            ' When the ViewModel asks to be closed, close the window.
            Dim handler As EventHandler = Nothing
            handler = Sub()
                          RemoveHandler viewModel.RequestClose, handler
                          viewModel.Dispose()
                          window.Close()
                      End Sub
            AddHandler viewModel.RequestClose, handler

            ' When the ViewModel asks to view result data, show a message box.
            ' This is sample behavior. A more realistic approach would be to
            ' open a new data tab and allow the user to scroll through the results.
            AddHandler viewModel.RequestNyse, Sub()
                                                  MessageBox.Show("View Nyse market data", "Nyse")
                                              End Sub
            AddHandler viewModel.RequestNasdaq, Sub()
                                                    MessageBox.Show("View Nasdaq market data", "Nasdaq")
                                                End Sub
            AddHandler viewModel.RequestMerged, Sub()
                                                    MessageBox.Show("View merged market data", "Merged")
                                                End Sub
            AddHandler viewModel.RequestNormalized, Sub()
                                                        MessageBox.Show("View normalized market data", "Normalized")
                                                    End Sub
            AddHandler viewModel.RequestFedHistorical, Sub()
                                                           MessageBox.Show("View Fed historical data", "Fed")
                                                       End Sub
            AddHandler viewModel.RequestNormalizedHistorical, Sub()
                                                                  MessageBox.Show("View normalized Fed historical data", "Normalized Fed")
                                                              End Sub
            AddHandler viewModel.RequestAnalyzed, Sub()
                                                      MessageBox.Show("View market data analysis", "Analysis")
                                                  End Sub
            AddHandler viewModel.RequestAnalyzedHistorical, Sub()
                                                                MessageBox.Show("View historical analysis", "Historical analysis")
                                                            End Sub
            AddHandler viewModel.RequestModeled, Sub()
                                                     MessageBox.Show("View market model", "Model")
                                                 End Sub
            AddHandler viewModel.RequestModeledHistorical, Sub()
                                                               MessageBox.Show("View historical model", "Analysis")
                                                           End Sub
            AddHandler viewModel.RequestRecommendation, Sub()
                                                            MessageBox.Show("View comparision and recommendation", "Comparison")
                                                        End Sub

            ' Set the ViewModel as the window's data context to connect the ViewModel to the View.
            ' This allows UI property bindings to retrieve their values from properties supplied by the ViewModel.
            window.ViewModel = viewModel

            ' Display the View.
            window.Show()
        End Sub

        Protected Overrides Sub OnExit(ByVal e As ExitEventArgs)
            If viewModel IsNot Nothing Then
                viewModel.Dispose()
            End If
            MyBase.OnExit(e)
        End Sub
    End Class
End Namespace