'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks

Namespace Microsoft.Practices.ParallelGuideSamples.ParallelSort
    Public NotInheritable Class Sort
        Public Shared Threshold As Integer = 150 ' array length to use InsertionSort instead of SequentialQuickSort

        Private Sub New()
        End Sub
        Public Shared Sub InsertionSort(ByVal array() As Integer, ByVal [from] As Integer, ByVal [to] As Integer)
            For i As Integer = From + 1 To [to] - 1
                Dim a = array(i)
                Dim j As Integer = i - 1
                Do While j >= From AndAlso array(j) > a
                    array(j + 1) = array(j)
                    j -= 1
                Loop
                array(j + 1) = a
            Next i
        End Sub

        Private Shared Sub Swap(ByVal array() As Integer, ByVal i As Integer, ByVal j As Integer)
            Dim temp = array(i)
            array(i) = array(j)
            array(j) = temp
        End Sub

        Private Shared Function Partition(ByVal array() As Integer, ByVal [from] As Integer, ByVal [to] As Integer, ByVal pivot As Integer) As Integer
            ' Pre: from <= pivot < to (other than that, pivot is arbitrary)
            Dim arrayPivot = array(pivot) ' pivot value
            Swap(array, pivot, [to] - 1) ' move pivot value to end for now, after this pivot not used
            Dim newPivot = From ' new pivot
            For i As Integer = From To [to] - 2 ' be careful to leave pivot value at the end
                ' Invariant: from <= newpivot <= i < to - 1 && 
                ' forall from <= j <= newpivot, array[j] <= arrayPivot && forall newpivot < j <= i, array[j] > arrayPivot
                If array(i) <= arrayPivot Then
                    Swap(array, newPivot, i) ' move value smaller than arrayPivot down to newpivot
                    newPivot += 1
                End If
            Next i
            Swap(array, newPivot, [to] - 1) ' move pivot value to its final place
            Return newPivot ' new pivot
            ' Post: forall i <= newpivot, array[i] <= array[newpivot]  && forall i > ...
        End Function

        Public Shared Sub SequentialQuickSort(ByVal array() As Integer)
            SequentialQuickSort(array, 0, array.Length)
        End Sub

        Private Shared Sub SequentialQuickSort(ByVal array() As Integer, ByVal [from] As Integer, ByVal [to] As Integer)
            If [to] - From <= Threshold Then
                InsertionSort(array, From, [to])
            Else
                Dim pivot As Integer = CType(from + ([to] - from) / 2, Integer) ' could be anything, use middle
                pivot = Partition(array, From, [to], pivot)
                ' Assert: forall i < pivot, array[i] <= array[pivot]  && forall i > ...
                SequentialQuickSort(array, From, pivot)
                SequentialQuickSort(array, pivot + 1, [to])
            End If
        End Sub

        Public Shared Sub ParallelQuickSort(ByVal array() As Integer)
            ParallelQuickSort(array, 0, array.Length, CInt(Fix(Math.Log(Environment.ProcessorCount, 2))) + 4)
        End Sub

        Private Shared Sub ParallelQuickSort(ByVal array() As Integer, ByVal [from] As Integer, ByVal [to] As Integer, ByVal depthRemaining As Integer)
            If [to] - From <= Threshold Then
                InsertionSort(array, From, [to])
            Else
                Dim pivot As Integer = CType(from + ([to] - from) / 2, Integer) ' could be anything, use middle
                pivot = Partition(array, From, [to], pivot)
                If depthRemaining > 0 Then
                    Parallel.Invoke(Sub() ParallelQuickSort(array, from, pivot, depthRemaining - 1), Sub() ParallelQuickSort(array, pivot + 1, [to], depthRemaining - 1))
                Else
                    ParallelQuickSort(array, From, pivot, 0)
                    ParallelQuickSort(array, pivot + 1, [to], 0)
                End If
            End If
        End Sub
    End Class
End Namespace
