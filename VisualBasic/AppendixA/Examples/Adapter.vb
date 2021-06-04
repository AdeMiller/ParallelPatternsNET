'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.RelatedPatterns
    Public Interface IWithFutures
        Function Start() As Task(Of Integer)
    End Interface

    Public Class FuturesBased
        Implements IWithFutures
        Public Function Start() As Task(Of Integer) Implements IWithFutures.Start
            Return Task(Of Integer).Factory.StartNew(Function()
                                                         SampleUtilities.DoCpuIntensiveOperation(2.0)
                                                         Return 42
                                                     End Function)
        End Function
    End Class

    Public Interface IWithEvents
        Sub Start()
        Event Completed As EventHandler(Of CompletedEventArgs)
    End Interface

    Public Class EventBased
        Implements IWithEvents
        Private ReadOnly instance As IWithFutures = New FuturesBased()

        Public Sub Start() Implements IWithEvents.Start
            Dim _task As Task(Of Integer) = instance.Start()
            _task.ContinueWith(Function(t)
                                   Dim evt = CompletedEvent
                                   If evt IsNot Nothing Then
                                       evt(Me, New CompletedEventArgs(t.Result))
                                   End If
                               End Function)
        End Sub

        Public Event Completed As EventHandler(Of CompletedEventArgs) Implements IWithEvents.Completed
    End Class

    Public Class CompletedEventArgs
        Inherits EventArgs
        Private privateResult As Integer
        Public Property Result() As Integer
            Get
                Return privateResult
            End Get
            Private Set(ByVal value As Integer)
                privateResult = value
            End Set
        End Property

        Public Sub New(ByVal result As Integer)
            Me.Result = result
        End Sub
    End Class
End Namespace
