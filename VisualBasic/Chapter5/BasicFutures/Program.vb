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
Imports System.Diagnostics.CodeAnalysis

Namespace Microsoft.Practices.ParallelGuideSamples.BasicFutures
    ''' <summary>
    ''' This program shows the simplest use case for the futures and continuations pattern.
    ''' Refer to Chapter 5 of the text.
    ''' </summary>
    Public NotInheritable Class Program
        ''' <summary>
        ''' A computationally intensive function
        ''' </summary>
        Private Sub New()
        End Sub
        Private Shared Function F1(ByVal value As Integer) As Integer
            SampleUtilities.DoCpuIntensiveOperation(2.0)
            Return value * value
        End Function

        ''' <summary>
        ''' A computationally intensive function
        ''' </summary>
        Private Shared Function F2(ByVal value As Integer) As Integer
            SampleUtilities.DoCpuIntensiveOperation(1.0)
            Return value - 2
        End Function

        ''' <summary>
        ''' A computationally intensive function
        ''' </summary>
        Private Shared Function F3(ByVal value As Integer) As Integer
            SampleUtilities.DoCpuIntensiveOperation(1.0)
            Return value + 1
        End Function

        ''' <summary>
        ''' A computationally intensive function
        ''' </summary>
        Private Shared Function F4(ByVal value1 As Integer, ByVal value2 As Integer) As Integer
            SampleUtilities.DoCpuIntensiveOperation(0.1)
            Return value1 + value2
        End Function

        ''' <summary>
        ''' Sequential example
        ''' </summary>
        Public Shared Function Example1() As Integer
            Dim a = 22

            Dim b = F1(a)
            Dim c = F2(a)
            Dim d = F3(c)
            Dim f = F4(b, d)
            Return f
        End Function

        ''' <summary>
        ''' A parallel example that uses the futures pattern for F1
        ''' </summary>
        Public Shared Function Example2() As Integer
            Dim a = 22

            Dim bf = Task(Of Integer).Factory.StartNew(Function() F1(a))
            Dim c = F2(a)
            Dim d = F3(c)
            Dim f = F4(bf.Result, d)
            Return f
        End Function

        ''' <summary>
        ''' A parallel example that uses the futures pattern for F2/F3
        ''' </summary>
        Public Shared Function Example3() As Integer
            Dim a = 22

            Dim df = Task(Of Integer).Factory.StartNew(Function() F3(F2(a)))
            Dim b = F1(a)
            Dim f = F4(b, df.Result)
            Return f
        End Function

        ''' <summary>
        ''' A parallel example that uses the futures and continations pattern.
        ''' This is to illustrate syntax only; there is no performance benefit in this case over Example 2 or 3 above.
        ''' </summary>
        Public Shared Function Example4() As Integer
            Dim a = 22

            Dim cf = Task(Of Integer).Factory.StartNew(Function() F2(a))
            Dim df = cf.ContinueWith(Function(t) F3(t.Result))
            Dim b = F1(a)
            Dim f = F4(b, df.Result)
            Return f
        End Function

        ''' <summary>
        ''' A parallel example that uses the futures pattern applied to two values.
        ''' This is for comparison only; there is no performance benefit in this case over Example 2 or 3 above.
        ''' You should pattern your own code after either Example 2 or 3, not this method.
        ''' </summary>
        Public Shared Function Example5() As Integer
            Dim a = 22
            Dim bf = Task(Of Integer).Factory.StartNew(Function() F1(a))
            Dim df = Task(Of Integer).Factory.StartNew(Function() F3(F2(a)))
            Dim f = F4(bf.Result, df.Result)
            Return f
        End Function

        <SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")> _
        Public Shared Function Example6() As Integer
            Dim a = 22

            Dim futureD As Task(Of Integer) = Task(Of Integer).Factory.StartNew(Function() F3(F2(a)))
            Try
                Dim b As Integer = F1(a)
                Dim f As Integer = F4(b, futureD.Result)
                Return f
            Catch ' any exception
                Console.WriteLine("Saw exception")
                Return -1
            End Try
        End Function

        Public Shared Sub Example7(ByVal myTextBox As TextBox)
            Dim a = 22

            Dim futureB = Task.Factory.StartNew(Of Integer)(Function() F1(a))
            Dim futureD = Task.Factory.StartNew(Of Integer)(Function() F3(F2(a)))

            Dim futureF = Task.Factory.ContinueWhenAll(Of Integer, Integer)(New Task(Of Integer)() {futureB, futureD}, Function(tasks) F4(futureB.Result, futureD.Result))
            futureF.ContinueWith(Sub(t)
                                     myTextBox.Dispatcher.Invoke(CType(Sub()
                                                                           myTextBox.Text = t.Result.ToString()
                                                                       End Sub, Action))
                                 End Sub)
        End Sub

        Public Shared Sub Example8()
            Dim cb As New AsyncCallback(Sub(iar1)

                                        End Sub)
            Dim a As Action = Sub()
                                  Console.WriteLine("Hello")
                              End Sub
            Dim t1 = Task.Factory.FromAsync(a.BeginInvoke(cb, Nothing), AddressOf a.EndInvoke)
            t1.Wait()
        End Sub


        Shared Sub Main()
            ' Note: for consistent timing results, run these without the debugger. 
            ' Observe CPU usage using the task manager. On a multicore machine, the sequential 
            ' version will use less CPU and execute more slowly than the parallel versions.

            Console.WriteLine("Basic Futures Samples" & vbLf)
#If (DEBUG) Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            Console.WriteLine("Starting...")

            ' timed comparison between sequential and two ways of using the futures pattern
            SampleUtilities.TimedRun(AddressOf Example1, "Sequential")
            SampleUtilities.TimedRun(AddressOf Example2, "Parallel, using F1 future")
            SampleUtilities.TimedRun(AddressOf Example3, "Parallel, using F2/F3 future")

            ' additional variants for comparison
            Console.WriteLine()
            Console.WriteLine("Other variants, for comparison--")
            SampleUtilities.TimedRun(AddressOf Example4, "Parallel, using F2 future and F3 continuation")
            SampleUtilities.TimedRun(AddressOf Example5, "Parallel, using F1 and F2/F3 future")
            SampleUtilities.TimedRun(AddressOf Example6, "Parallel, with try/catch block")

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub
    End Class
End Namespace