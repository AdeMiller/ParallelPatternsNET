'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.Concurrent
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.BasicParallelLoops
    Public Class ParallelForExample
        Public LoopBodyComplexity As Integer = 50
        Public NumberOfSteps As Integer = 10000000
        Public VerifyResult As Boolean = True
        Public SimulateInternalError As Boolean = False

        Private Class ParallelForExampleException
            Inherits Exception
        End Class

#Region "Harness"

        Public Sub DoParallelFor()
            Console.WriteLine("Parallel For Examples (LoopBodyComplexity={0}, NumberOfSteps={1})", LoopBodyComplexity, NumberOfSteps)

            RunParallelForExample(AddressOf Chapter2Example01, "Sequential for")
            RunParallelForExample(AddressOf Chapter2Example02, "Simple Parallel.For")
            RunParallelForExample(AddressOf Chapter2Example03, "PLINQ ForAll")

            RunParallelForExample(AddressOf Chapter2Example04a, "PLINQ 1")
            RunParallelForExample(AddressOf Chapter2Example04b, "PLINQ 2")

            RunParallelForExample(AddressOf Chapter2Example06, "Partitioned")
            RunParallelForExample(AddressOf Chapter2Example07, "Partitioned with fixed ranges")

            Console.WriteLine()
        End Sub

        Public Sub DoParallelForEach()
            Console.WriteLine("Parallel ForEach Examples (LoopBodyComplexity={0}, NumberOfSteps={1})", LoopBodyComplexity, NumberOfSteps)

            RunParallelForEachExample(AddressOf Chapter2Example21, "Sequential foreach")
            RunParallelForEachExample(AddressOf Chapter2Example22, "Simple Parallel.ForEach")
            RunParallelForEachExample(AddressOf Chapter2Example23, "PLINQ ForAll")

            RunParallelForEachExample(AddressOf Chapter2Example24a, "PLINQ 1")
            RunParallelForEachExample(AddressOf Chapter2Example24b, "PLINQ 2")

            RunParallelForEachExample(AddressOf Chapter2Example27, "Partitioned with load balancing")

            Console.WriteLine()
        End Sub

        Private Sub RunParallelForExample(ByVal action As Func(Of Double()), ByVal label As String)
            ' clean up from previous run
            GC.Collect()
            Try

                Dim result() As Double = Nothing
                SampleUtilities.TimedAction(Sub()
                                                result = action.Invoke()
                                            End Sub, "  " & label)

                If VerifyResult Then
                    For i As Integer = 0 To NumberOfSteps - 1
                        EnsureResult(result(i), i)
                    Next i
                End If
            Catch ae As AggregateException
                ae.Handle(Function(e)
                              Console.WriteLine("  {0}: Failed with {1}", label, e.GetType().ToString())
                              Return True
                          End Function)
            Catch ex As ParallelForExampleException
                Console.WriteLine("  {0}: Failed with unaggregated {1}  ", label, ex.GetType().ToString())
            Catch ex As Exception
                Console.WriteLine("  {0}: Failed with unaggregated {1}  ", label, ex.GetType().ToString())
            End Try
        End Sub


        Private Sub RunParallelForEachExample(ByVal action As Func(Of Integer(), Double()), ByVal label As String)
            ' clean up from previous run
            GC.Collect()
            Try
                Dim result() As Double = Nothing
                Dim source() As Integer = Enumerable.Range(0, NumberOfSteps).ToArray()

                SampleUtilities.TimedAction(Sub()
                                                result = action(source)
                                            End Sub, "  " & label)

                If VerifyResult Then
                    For i As Integer = 0 To NumberOfSteps - 1
                        EnsureResult(result(i), i)
                    Next i
                End If
            Catch ae As AggregateException
                ae.Handle(Function(e)
                              Console.WriteLine("  {0}: Failed with {1}", label, e.GetType().ToString())
                              Return True
                          End Function)
            Catch ex As ParallelForExampleException
                Console.WriteLine("  {0}: Failed with unaggregated {1}  ", label, ex.GetType().ToString())
            Catch ex As Exception
                Console.WriteLine("  {0}: Failed with unaggregated {1}  ", label, ex.GetType().ToString())
            End Try
        End Sub


#End Region

#Region "Work Function"

        Public Sub EnsureResult(ByVal actual As Double, ByVal i As Integer)
            Dim expected As Double = DoWorkExpectedResult(i)
            If actual <> expected Then
                Throw New InvalidOperationException(String.Format("Unexpected value: actual {0}, expected {1}", actual, expected))
            End If
        End Sub

        Public Function DoWorkExpectedResult(ByVal i As Integer) As Double
            Return 2.5R * (LoopBodyComplexity + 1) * LoopBodyComplexity * i
        End Function

        Public Function DoWork(ByVal i As Integer) As Double
            Dim result As Double = 0
            For j As Integer = 1 To LoopBodyComplexity
                Dim j2 As Double = CDbl(j)
                Dim i2 As Double = CDbl(i)
                result += Math.Sqrt((9.0R * i2 * i2 + 16.0R * i * i) * j2 * j2)
            Next j

            ' simulate unexpected condition in loop body
            If i Mod 402030 = 2029 AndAlso SimulateInternalError Then
                Throw New ParallelForExampleException()
            End If

            Return Math.Round(result)
        End Function

#End Region

#Region "For Examples"

        ' Sequential for loop
        Public Function Chapter2Example01() As Double()
            Dim result(NumberOfSteps - 1) As Double

            For i As Integer = 0 To NumberOfSteps - 1
                result(i) = DoWork(i)
            Next i
            Return result
        End Function

        ' LINQ 1 (sequential)
        Public Function Chapter2Example01b() As Double()
            Return Enumerable.Range(0, NumberOfSteps).Select(Function(i) DoWork(i)).ToArray()
        End Function

        ' LINQ 2 (sequential)
        Public Function Chapter2Example01c() As Double()
            Return ( _
                From i In Enumerable.Range(0, NumberOfSteps) _
                Select DoWork(i)).ToArray()
        End Function

        Public Function Chapter2Example02() As Double()
            Dim result(NumberOfSteps - 1) As Double

            Parallel.For(0, NumberOfSteps, Sub(i)
                                               result(i) = DoWork(i)
                                           End Sub)
            Return result
        End Function

        Public Function Chapter2Example03() As Double()
            Dim result(NumberOfSteps - 1) As Double


            ParallelEnumerable.Range(0, NumberOfSteps).ForAll(Sub(i)
                                                                  result(i) = DoWork(i)
                                                              End Sub)

            Return result
        End Function

        Public Function Chapter2Example04a() As Double()
            Return ( _
                From i In ParallelEnumerable.Range(0, NumberOfSteps).AsOrdered() _
                Select DoWork(i)).ToArray()
        End Function

        Public Function Chapter2Example04b() As Double()
            Return ParallelEnumerable.Range(0, NumberOfSteps).AsOrdered().Select(Function(i) DoWork(i)).ToArray()

        End Function

        ' optimized for small units of work, each of the same duration
        ' avoids false sharing
        ' not appropriate if iteration steps of unequal duration
        Public Function Chapter2Example06() As Double()
            Dim result(NumberOfSteps - 1) As Double

            Parallel.ForEach(Partitioner.Create(0, NumberOfSteps), Sub(range)
                                                                       For i As Integer = range.Item1 To range.Item2 - 1
                                                                           result(i) = DoWork(i)
                                                                       Next i
                                                                   End Sub)
            Return result
        End Function

        Public Function Chapter2Example07() As Double()
            Dim result(NumberOfSteps - 1) As Double
            Dim rangeSize As Integer = NumberOfSteps \ (Environment.ProcessorCount * 10)

            Parallel.ForEach(Partitioner.Create(0, NumberOfSteps, If(rangeSize >= 1, rangeSize, 1)), Sub(range)
                                                                                                         For i As Integer = range.Item1 To range.Item2 - 1
                                                                                                             result(i) = DoWork(i)
                                                                                                         Next i
                                                                                                     End Sub)
            Return result
        End Function


#End Region

#Region "ForEachExamples"

        ' Sequential for loop
        Public Function Chapter2Example21(ByVal source() As Integer) As Double()
            Dim result(source.Length - 1) As Double

            For Each i In source
                result(i) = DoWork(i)
            Next i
            Return result
        End Function

        ' LINQ 1 (sequential)
        Public Function Chapter2Example21b(ByVal source() As Integer) As Double()
            Return source.Select(Function(i) DoWork(i)).ToArray()
        End Function

        ' LINQ 2 (sequential)
        Public Function Chapter2Example21c(ByVal source() As Integer) As Double()
            Return ( _
                From i In source _
                Select DoWork(i)).ToArray()
        End Function

        Public Function Chapter2Example22(ByVal source() As Integer) As Double()
            Dim result(source.Length - 1) As Double

            Parallel.ForEach(source, Sub(i)
                                         result(i) = DoWork(i)
                                     End Sub)
            Return result
        End Function

        Public Function Chapter2Example23(ByVal source() As Integer) As Double()
            Dim result(source.Length - 1) As Double

            source.AsParallel().ForAll(Sub(i)
                                           result(i) = DoWork(i)
                                       End Sub)

            Return result
        End Function

        Public Function Chapter2Example24a(ByVal source() As Integer) As Double()
            Return ( _
                From i In source.AsParallel().AsOrdered() _
                Select DoWork(i)).ToArray()
        End Function

        Public Function Chapter2Example24b(ByVal source() As Integer) As Double()
            Return source.AsParallel().AsOrdered().Select(Function(i) DoWork(i)).ToArray()

        End Function

        ' partitioner with load balancing
        Public Function Chapter2Example27(ByVal source() As Integer) As Double()
            Dim result(source.Length - 1) As Double

            Parallel.ForEach(Partitioner.Create(source, True), Sub(i)
                                                                   result(i) = DoWork(i)
                                                               End Sub)
            Return result
        End Function

#End Region

#Region "Custom Iterator Example"

        ' using task-local state for Parallel.For iteration
        Public Function Chapter2Example40() As Double()
            Dim result(NumberOfSteps - 1) As Double

            Parallel.For(0, NumberOfSteps, New ParallelOptions(), Function()
                                                                      Return New Random()
                                                                  End Function, Function(i, loopState, random)
                                                                                    result(i) = random.NextDouble()
                                                                                    Return random
                                                                                End Function, Sub(x)
                                                                                              End Sub)

            Return result
        End Function

        ' using task-local state for iteration, with partitioner
        Public Function Chapter2Example41() As Double()
            Dim result(NumberOfSteps - 1) As Double

            Parallel.ForEach(Partitioner.Create(0, NumberOfSteps), New ParallelOptions(), Function()
                                                                                              Return New Random()
                                                                                          End Function, Function(range, loopState, random)
                                                                                                            For i As Integer = range.Item1 To range.Item2 - 1
                                                                                                                result(i) = random.NextDouble()
                                                                                                            Next i
                                                                                                            Return random
                                                                                                        End Function, Sub(x)

                                                                                                                      End Sub)

            Return result
        End Function

#End Region
    End Class
End Namespace
