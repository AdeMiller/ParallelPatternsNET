'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Globalization
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.ParallelSort
    Public NotInheritable Class Program
        ''' <summary>
        ''' Create array of given length and populate with random integers 
        ''' </summary>
        Private Sub New()
        End Sub
        Public Shared Function MakeArray(ByVal length As Integer, ByVal seed As Integer) As Integer()
            Const max As Integer = 1000000
			Dim r = New Random(seed)
            Dim a = New Integer(length - 1) {}
            Dim i As Integer = 0
            Do While i < length
                a(i) = r.Next(max)
                i += 1
            Loop
            Return a
        End Function

        ''' <summary>
        ''' Print the first and last few elements in given array
        ''' </summary>
        Private Shared Sub PrintElements(ByVal array() As Integer, ByVal count As Integer)
            Console.Write("[")
            For i As Integer = 0 To count \ 2 - 1
                Console.Write("{0} ", array(i))
            Next i
            Console.Write("... ")
            For i As Integer = array.Length - count \ 2 To array.Length - 1
                Console.Write("{0} ", array(i))
            Next i
            Console.WriteLine("], {0} elements", array.Length)
        End Sub

        ''' <summary>
        ''' Command line arguments are:
        '''   length - of array to sort
        '''   threshold -  array length to use InsertionSort instead of SequentialQuickSort
        ''' </summary>
        Shared Sub Main(ByVal args() As String)
            Console.WriteLine("Sort Sample" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If

            Dim length As Integer = 40000000 ' default
			Dim seed As Integer = 1	' seed for reproducible runs
            If args.Length > 0 Then
                length = Int32.Parse(args(0), CultureInfo.CurrentCulture)
            End If
            If args.Length > 1 Then
                Sort.Threshold = Int32.Parse(args(1), CultureInfo.CurrentCulture)
            End If

            Console.WriteLine()
            Dim a = MakeArray(length, seed)
            PrintElements(a, 8)
            SampleUtilities.TimedRun(Function() As Integer
                                         Sort.SequentialQuickSort(a)
                                         Return a.Length
                                     End Function, "  Sequential QuickSort")
            PrintElements(a, 8)

            Console.WriteLine()
            a = MakeArray(length, seed)
            PrintElements(a, 8)
            SampleUtilities.TimedRun(Function() As Integer
                                         Sort.ParallelQuickSort(a)
                                         Return a.Length
                                     End Function, "      Parallel QuickSort")
            PrintElements(a, 8)

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadKey()
        End Sub

    End Class
End Namespace
