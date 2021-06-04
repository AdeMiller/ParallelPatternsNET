'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks

Namespace Microsoft.Practices.ParallelGuideSamples.BasicParallelLoops
    Friend Class Program
        Shared Sub Main()
            Task.Factory.StartNew(Sub()
                                      MainOnThreadpoolThread()
                                  End Sub).Wait()
        End Sub

        Private Shared Sub MainOnThreadpoolThread()
            Console.WriteLine("Basic Parallel Loops Samples" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
            Const verify As Boolean = True
#Else
            Const verify As Boolean = False
#End If
            Dim example = New CustomIteratorExample()
            example.Example()

            Dim examples = New ParallelForExample() {New ParallelForExample() With {.LoopBodyComplexity = 10000000, .NumberOfSteps = 10, .VerifyResult = verify}, New ParallelForExample() With {.LoopBodyComplexity = 1000000, .NumberOfSteps = 100, .VerifyResult = verify}, New ParallelForExample() With {.LoopBodyComplexity = 10000, .NumberOfSteps = 10000, .VerifyResult = verify}, New ParallelForExample() With {.LoopBodyComplexity = 100, .NumberOfSteps = 1000000, .VerifyResult = verify}, New ParallelForExample() With {.LoopBodyComplexity = 10, .NumberOfSteps = 10000000, .VerifyResult = verify}}

            For Each e1 In examples
                e1.DoParallelFor()
            Next e1

            For Each e2 In examples
                e2.DoParallelForEach()
            Next e2

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub
    End Class
End Namespace
