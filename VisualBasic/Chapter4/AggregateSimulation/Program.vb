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


Namespace Microsoft.Practices.ParallelGuideSamples.AggregateSimulation
    Friend Class Program
        Private Const TableSize As Integer = 40
        Private Const BucketSize As Double = 5

        Shared Sub Main()
            Console.WriteLine("Aggregation Sample" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            Const trialCount As Integer = 5000000
            Const mean As Double = 102.5
            Const stdDev As Double = 15

            Dim histogram1(), histogram2(), histogram3() As Integer

            Dim s As New Stopwatch()

            Console.WriteLine("Performing Sequential Aggregation...")
            s.Start()
            histogram1 = DoSequentialAggregation(trialCount, mean, stdDev)
            s.Stop()
            Console.WriteLine(SampleUtilities.FormattedTime(s.Elapsed))
            PrintHistogram(histogram1)

            Console.WriteLine("Performing Parallel Aggregation...")
            s.Restart()
            histogram2 = DoParallelAggregation(trialCount, mean, stdDev)
            s.Stop()
            Console.WriteLine(SampleUtilities.FormattedTime(s.Elapsed))
            PrintHistogram(histogram2)

            Console.WriteLine("Performing PLINQ Aggregation...")
            s.Restart()
            histogram3 = DoParallelAggregationPlinq(trialCount, mean, stdDev)
            s.Stop()
            Console.WriteLine(SampleUtilities.FormattedTime(s.Elapsed))
            PrintHistogram(histogram3)

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub

        Private Shared Function DoSequentialAggregation(ByVal count As Integer, ByVal mean As Double, ByVal stdDev As Double) As Integer()
            Dim generator = New Random(SampleUtilities.MakeRandomSeed())

            Dim histogram() As Integer = MakeEmptyHistogram()

            For i As Integer = 0 To count - 1
                ' get the next input value
                Dim sample = generator.NextDouble()
                If sample > 0.0 Then
                    ' MAP: perform a simulation trial for the sample value
                    Dim simulationResult = DoSimulation(sample, mean, stdDev)

                    ' REDUCE: merge the result of simulation into a histogram
                    Dim histogramBucket As Integer = CInt(Fix(Math.Floor(simulationResult / BucketSize)))
                    If 0 <= histogramBucket AndAlso histogramBucket < TableSize Then
                        histogram(histogramBucket) += 1
                    End If
                End If
            Next i
            Return histogram
        End Function

        Private Shared Function MakeEmptyHistogram() As Integer()
            Return New Integer(TableSize - 1) {}
        End Function

        Private Shared Function CombineHistograms(ByVal histogram1() As Integer, ByVal histogram2() As Integer) As Integer()
            Dim mergedHistogram = MakeEmptyHistogram()
            For i As Integer = 0 To TableSize - 1
                mergedHistogram(i) = histogram1(i) + histogram2(i)
            Next i
            Return mergedHistogram
        End Function

        ' Placeholder for a user-written simulation routine. For example, this 
        ' could be a financial simulation that explores various risk outcomes.
        ' This placeholder just transforms the value so that the outputs of
        ' simulation will follow a bell curve.
        Private Shared Function DoSimulation(ByVal sampleValue As Double, ByVal mean As Double, ByVal stdDev As Double) As Double
            Return SampleUtilities.GaussianInverse(sampleValue, mean, stdDev)
        End Function

        Private Shared Function DoParallelAggregation(ByVal count As Integer, ByVal mean As Double, ByVal stdDev As Double) As Integer()
            ' Partition the iterations
            Dim rangePartitioner = Partitioner.Create(0, count)

            Dim histogram() As Integer = MakeEmptyHistogram()

            'ForEach(Of TSource, TLocal)(Partitioner (Of TSource ) source, 
            '                            Func (Of TLocal ) localInit, 
            '                            Func (Of TSource, ParallelLoopState, TLocal, TLocal ) body, 
            '                            Action (Of TLocal ) localFinally)

            '1- the partitioner object

            '2- the local state object that will 
            'ollect results for a single partition

            '3- the body that will be invoked once for each of 
            'the partitions created by the partitioner. (The number of
            'partitions depends on the number of cores.)

            '4- The finalizer that will be run once for each partition to combine
            'results created for each partition.
            Parallel.ForEach(
                rangePartitioner,
                Function() MakeEmptyHistogram(),
                Function(range, loopState, index, localHistogram)
                    ' Make a local generator to avoid conflicts between partitions.
                    Dim generator = New Random(SampleUtilities.MakeRandomSeed())

                    ' Iterate over the range assigned by the partitioner
                    For i As Integer = range.Item1 To range.Item2 - 1
                        ' With each iteration get the next input value 
                        Dim sample = generator.NextDouble()

                        If sample > 0.0 Then
                            ' MAP: perform a simulation trial for the sample value
                            Dim simulationResult = DoSimulation(sample, mean, stdDev)

                            ' REDUCE: merge the result of simulation into the local histogram
                            Dim histogramBucket As Integer = CInt(Fix(Math.Floor(simulationResult / BucketSize)))
                            If 0 <= histogramBucket AndAlso histogramBucket < TableSize Then
                                localHistogram(histogramBucket) += 1
                            End If
                        End If
                    Next i
                    Return localHistogram
                End Function,
                 Function(localHistogram)
                     ' Use lock to enforce serial access to single, shared result
                     SyncLock histogram
                         ' MERGE: local histogram results are added into the global histogram
                         For i As Integer = 0 To TableSize - 1
                             histogram(i) += localHistogram(i)
                         Next i
                     End SyncLock
                 End Function)
            ' Parallel.ForEach

            ' return the global histogram
            Return histogram
        End Function

        Private Shared Function DoParallelAggregationPlinq(ByVal count As Integer, ByVal mean As Double, ByVal stdDev As Double) As Integer()
            'Aggregate(Of TSource, TAccumulate, TResult)(
            '            Me ParallelQuery(Of TSource) source, 
            '            Func(Of TAccumulate) seedFactory, 
            '            Func(Of TAccumulate, TSource, TAccumulate) updateAccumulatorFunc, 
            '            Func(Of TAccumulate, TAccumulate, TAccumulate) combineAccumulatorsFunc, 
            '            Func(Of TAccumulate, TResult) resultSelector)       
            ' 1- create an empty local accumulator object
            '    that includes all task-local state
            ' 2- run the simulation, adding result to local accumulator 
            ' With each iteration get the next random value 
            ' Perform a simulation trial for the sample value
            ' Put the result of simulation into the histogram of the local accumulator
            ' 3- Combine local results pairwise.
            ' 4- Extract answer from final combination
            Return ParallelEnumerable.Range(0, count).Aggregate(
                    Function() New Tuple(Of Integer(), Random)(MakeEmptyHistogram(), New Random(SampleUtilities.MakeRandomSeed())),
                    Function(localAccumulator, i)
                        ' With each iteration get the next random value 
                        Dim sample = localAccumulator.Item2.NextDouble()

                        If sample > 0.0 AndAlso sample < 1.0 Then
                            ' Perform a simulation trial for the sample value
                            Dim simulationResult = DoSimulation(sample, mean, stdDev)

                            ' Put the result of simulation into the histogram of the local accumulator
                            Dim histogramBucket As Integer = CInt(Fix(Math.Floor(simulationResult / BucketSize)))
                            If 0 <= histogramBucket AndAlso histogramBucket < TableSize Then
                                localAccumulator.Item1(histogramBucket) += 1
                            End If
                        End If
                        Return localAccumulator
                    End Function,
                    Function(localAccumulator1, localAccumulator2) New Tuple(Of Integer(), Random)(CombineHistograms(localAccumulator1.Item1, localAccumulator2.Item1), Nothing),
                    Function(finalAccumulator) finalAccumulator.Item1)
            'Aggregate
        End Function

        Private Shared Sub PrintHistogram(ByVal histogram() As Integer)
            For j As Integer = 0 To TableSize - 1
                Console.WriteLine("{0}; {1}", CInt(Fix(j * BucketSize)), histogram(j))
            Next j
        End Sub
    End Class
End Namespace
