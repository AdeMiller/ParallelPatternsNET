'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================
Imports System
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities
Imports System.Threading
Imports System.IO

Namespace Microsoft.Practices.ParallelGuideSamples.BasicParallelTasks
    Friend Class Program
        Private Const TaskSeconds As Double = 1.0

        Shared Sub Main()
            Task.Factory.StartNew(Sub() MainTask()).Wait()
        End Sub

        Private Shared Sub MainTask()
            Console.WriteLine("Basic Parallel Tasks Samples" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            SampleUtilities.TimedAction(AddressOf Chapter3Sample01Sequential, "2 steps, sequential")
            SampleUtilities.TimedAction(AddressOf Chapter3Sample01ParallelTask, "2 steps (Task.Wait), parallel")
            SampleUtilities.TimedAction(AddressOf Chapter3Sample01ParallelInvoke, "2 steps, parallel invoke")

            SampleUtilities.TimedAction(AddressOf Chapter3Sample03, "Speculative Execution")
            SampleUtilities.TimedAction(AddressOf Chapter3Sample04_1, "Task.WaitAny")

            ExampleOfIncorrectClosure()
            ExampleOfCorrectClosure()
            ExampleOfIncorrectDispose()
            ExampleOfCorrectDispose()

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub

        Private Shared Sub Chapter3Sample01Sequential()
            DoLeft()
            DoRight()
        End Sub

        Private Shared Sub Chapter3Sample01ParallelTask()
            Dim t1 As Task = Task.Factory.StartNew(AddressOf DoLeft)
            Dim t2 As Task = Task.Factory.StartNew(AddressOf DoRight)

            Task.WaitAll(t1, t2)
        End Sub

        Private Shared Sub Chapter3Sample01ParallelInvoke()
            Parallel.Invoke(AddressOf DoLeft, AddressOf DoRight)
        End Sub

        Private Shared Sub Chapter3Sample03()
            Chapter3Sample03_1()
        End Sub

        Private Shared Sub Chapter3Sample03_1()
            SpeculativeInvoke(AddressOf SearchLeft, AddressOf SearchRight, AddressOf SearchCenter)
        End Sub

        Public Shared Sub SpeculativeInvoke(ByVal ParamArray actions() As Action(Of CancellationToken))
            Dim cts = New CancellationTokenSource()
            Dim token = cts.Token
            Dim tasks = ( _
                From a In actions _
                Select Task.Factory.StartNew(Sub() a(token), token)).ToArray()
            Task.WaitAny(tasks)
            cts.Cancel()
            Try
                Task.WaitAll(tasks)
            Catch ae As AggregateException
                ae.Flatten().Handle(Function(e) TypeOf e Is OperationCanceledException)
            Finally
                If cts IsNot Nothing Then
                    cts.Dispose()
                End If
            End Try
        End Sub

        Private Shared Sub DoLeft()
            SampleUtilities.DoCpuIntensiveOperation(TaskSeconds * 0.2)
        End Sub

        Private Shared Sub DoRight()
            SampleUtilities.DoCpuIntensiveOperation(TaskSeconds * 0.3)
        End Sub

        Private Shared Sub DoCenter()
            SampleUtilities.DoCpuIntensiveOperation(TaskSeconds * 0.3)
        End Sub

        Private Shared Sub SearchCenter(ByVal token As CancellationToken)
            token.ThrowIfCancellationRequested()
            SampleUtilities.DoCpuIntensiveOperation(TaskSeconds * 0.5, token)
            token.ThrowIfCancellationRequested()
        End Sub

        Private Shared Sub SearchLeft(ByVal token As CancellationToken)
            token.ThrowIfCancellationRequested()
            SampleUtilities.DoCpuIntensiveOperation(TaskSeconds * 0.2, token)
            token.ThrowIfCancellationRequested()
        End Sub

        Private Shared Sub SearchRight(ByVal token As CancellationToken)
            token.ThrowIfCancellationRequested()
            SampleUtilities.DoCpuIntensiveOperation(TaskSeconds * 0.3, token)
            token.ThrowIfCancellationRequested()
        End Sub

        Private Shared Sub Chapter3Sample04_1()
            Dim taskIndex = -1
            Dim tasks() As Task = {Task.Factory.StartNew(AddressOf DoLeft),
                                   Task.Factory.StartNew(AddressOf DoRight),
                                   Task.Factory.StartNew(AddressOf DoCenter)}
            Dim allTasks() As Task = tasks

            Do While tasks.Length > 0
                taskIndex = Task.WaitAny(tasks)
                Console.WriteLine("Finished task {0}.", taskIndex + 1)
                tasks = tasks.Where(Function(t) t IsNot tasks(taskIndex)).ToArray()
            Loop

            Try
                Task.WaitAll(allTasks)
            Catch ae As AggregateException
                ae.Handle(Function(e) As Boolean
                              ' Modify DoCenter to throw an InvalidOperationException to see this message.
                              If TypeOf e Is InvalidOperationException Then
                                  Console.WriteLine("Saw expected exception.")
                                  Return True
                              Else
                                  Return False
                              End If
                          End Function)
            End Try
        End Sub

        ' This is an example of incorrect code. It produces the following warning on compilation:

        ' warning BC42324: Using the iteration variable in a lambda expression may have unexpected results.  
        ' Instead, create a local variable within the loop and assign it the value of the iteration variable.

        Private Shared Sub ExampleOfIncorrectClosure()
            Console.WriteLine("Incorrectly written closure returns unexpected values:")
            Dim tasks(3) As Task

            For i As Integer = 0 To 3
                tasks(i) = Task.Factory.StartNew(Sub() Console.WriteLine(i))
            Next i

            Task.WaitAll(tasks)
        End Sub

        Private Shared Sub ExampleOfCorrectClosure()
            Console.WriteLine("Correctly written closure returns expected values:")
            Dim tasks(3) As Task

            For i As Integer = 0 To 3
                Dim tmp = i
                tasks(i) = Task.Factory.StartNew(Sub() Console.WriteLine(tmp))
            Next i

            Task.WaitAll(tasks)
        End Sub

        Private Shared Sub ExampleOfIncorrectDispose()
            Try
                Dim t As Task(Of String)
                Using file = New StringReader("text")
                    t = Task(Of String).Factory.StartNew(Function() file.ReadLine())
                End Using
                ' WARNING: BUGGY CODE, file has been disposed
                Console.WriteLine(t.Result)
            Catch ae As AggregateException
                ae.Handle(Function(e) As Boolean
                              If TypeOf e Is ObjectDisposedException Then
                                  Console.WriteLine("Saw expected error: {0}", e.Message)
                                  Return True
                              Else
                                  Return False
                              End If
                          End Function)
            End Try
        End Sub

        Private Shared Sub ExampleOfCorrectDispose()
            Dim file As StringReader = Nothing
            Try
                file = New StringReader("text")
                Dim t As Task(Of String) = Task(Of String).Factory.StartNew(Function() file.ReadLine())

                Console.WriteLine("Saw correct output: {0}", t.Result)
            Finally
                If file IsNot Nothing Then
                    file.Dispose()
                End If
            End Try
        End Sub
    End Class
End Namespace
