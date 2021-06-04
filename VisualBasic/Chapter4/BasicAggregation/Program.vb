'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.Utilities
Imports System.Threading.Tasks
Imports System.Collections.Concurrent

Namespace Microsoft.Practices.ParallelGuideSamples.BasicAggregation
    Friend Class Program
        Private Const SequenceSize As Integer = 100000000
        Shared Sub Main()
            Task.Factory.StartNew(Sub() MainTask()).Wait()
        End Sub

        Private Shared Sub MainTask()
            Console.WriteLine("Basic Aggregation Samples" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            Dim sequence = SampleUtilities.Range(SequenceSize)

            SampleUtilities.TimedAction(Function() Chapter4Sample01Sequential(sequence), "calculate sum, sequential for loop")
            GC.Collect()
            SampleUtilities.TimedAction(Function() Chapter4Sample01IncorrectParallel(sequence), "calculate sum, incorrectly coded parallel loop")
            GC.Collect()
            SampleUtilities.TimedAction(Function() Chapter4Sample02Linq(sequence), "calculate sum, LINQ (sequential)")
            GC.Collect()
            SampleUtilities.TimedAction(Function() Chapter4Sample02Plinq(sequence), "calculate sum, PLINQ (parallel)")
            GC.Collect()
            SampleUtilities.TimedAction(Function() Chapter4Sample03Plinq(sequence), "custom aggregation (product) PLINQ (parallel)")
            GC.Collect()
            SampleUtilities.TimedAction(Function() Chapter4Sample01Parallel(sequence), "calculate sum, parallel for each")
            GC.Collect()
            SampleUtilities.TimedAction(Function() Chapter4Sample01ParallelPartitions(sequence), "calculate sum, parallel partitions")
            GC.Collect()

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub

        ' general transformation before calculating aggregate sum
        Private Shared Function Normalize(ByVal x As Double) As Double
            Return x
        End Function

        Private Shared Function Chapter4Sample01Sequential(ByVal sequence() As Double) As Double
            Dim sum As Double = 0.0R
            For i As Integer = 0 To sequence.Length - 1
                sum += Normalize(sequence(i))
            Next i
            Return sum
        End Function

        ' WARNING: BUGGY CODE. Do not copy this method.
        ' This version will run *much slower* than the sequential version
        Private Shared Function Chapter4Sample01IncorrectParallel(ByVal sequence() As Double) As Double
            Dim lockObject As New Object()
            Dim sum As Double = 0.0R

            ' BUG -- Do not use Parallel.For 
            ' BUG -- Do not use locking inside of a parallel loop for aggregation
            ' BUG -- Do not use shared variable for parallel aggregation
            Parallel.For(0, sequence.Length, Sub(i)
                                                 SyncLock lockObject
                                                     sum += Normalize(sequence(i))
                                                 End SyncLock
                                             End Sub)
            Return sum
        End Function

        Private Shared Function Chapter4Sample02Linq(ByVal sequence() As Double) As Double
            Return (
                From x In sequence
                Select Normalize(x)).Sum()
        End Function

        Private Shared Function Chapter4Sample02Plinq(ByVal sequence() As Double) As Double
            Return (
                From x In sequence.AsParallel()
                Select Normalize(x)).Sum()
        End Function

        Private Shared Function Chapter4Sample03Plinq(ByVal sequence() As Double) As Double
            Return (
                From x In sequence.AsParallel()
                Select Normalize(x)).Aggregate(1.0R, Function(y1, y2) y1 * y2)
        End Function

        Private Shared Function Chapter4Sample01Parallel(ByVal sequence() As Double) As Double
            Dim lockObject As New Object()
            Dim sum As Double = 0.0R

            'ForEach(Of TSource, TLocal)(
            '                 IEnumerable (Of TSource ) source, 
            '                 Func (Of TLocal ) localInit, 
            '                 Func (Of TSource, ParallelLoopState, TLocal, TLocal ) body, 
            '                 Action (Of TLocal ) localFinally)
            ' 1- The values to be aggregated
            ' 2- The local initial partial result
            ' 3- The loop body
            ' 4- The final step of each local context            
            ' Enforce serial access to single, shared result
            Parallel.ForEach(
                sequence,
                 Function() 0.0R,
                 Function(x, loopState, partialResult) Normalize(x) + partialResult,
                 Function(localPartialSum)
                     ' Enforce serial access to single, shared result
                     SyncLock lockObject
                         sum += localPartialSum
                     End SyncLock
                 End Function
                )
            Return sum
        End Function

        Private Shared Function Chapter4Sample01ParallelPartitions(ByVal sequence() As Double) As Double
            Dim lockObject As New Object()
            Dim sum As Double = 0.0R
            Dim rangePartitioner = Partitioner.Create(0, sequence.Length)

            'ForEach(Of TSource, TLocal)(
            '          Partitioner (Of TSource ) source, 
            '          Func (Of TLocal ) localInit, 
            '          Func (Of TSource, ParallelLoopState, TLocal, TLocal ) body, 
            '          Action (Of TLocal ) localFinally)
            ' 1- the input intervals
            ' 2- The local initial partial result
            ' 3- The loop body for each interval
            ' 4- The final step of each local context
            ' Use lock to enforce serial access to shared result
            Parallel.ForEach(rangePartitioner,
                             Function() 0.0,
                                Function(range, loopState, initialValue)
                                    Dim partialSum As Double = initialValue
                                    For i As Integer = range.Item1 To range.Item2 - 1
                                        partialSum += Normalize(sequence(i))
                                    Next i
                                    Return partialSum
                                End Function,
                     Function(localPartialSum)
                         ' Use lock to enforce serial access to shared result
                         SyncLock lockObject
                             sum += localPartialSum
                         End SyncLock
                     End Function)
            Return sum
        End Function
    End Class
End Namespace
