'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

'===============================================================================
'
' NOTE: Only some of the patterns discussed in Appendix B have samples 
'       associated with them.
'
'===============================================================================

Imports System.IO
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.RelatedPatterns
    Friend Class Program
        Shared Sub Main()
            Console.WriteLine("Supporting Patterns Samples" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            DecoratorExample()

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub

        Private Shared Sub DecoratorExample()
            Const maxImages As Integer = 1000

            Console.WriteLine("Loading images...")
            Dim images As IList(Of Bitmap) = LoadImages(maxImages)

            Dim serial As IImageEditor = New SerialEditor()
            SampleUtilities.TimedAction(Sub() serial.Rotate(RotateFlipType.RotateNoneFlipX, images), "Rotate, sequential")

            Dim parallel As IImageEditor = New ParallelEditor(New SerialEditor())
            SampleUtilities.TimedAction(Sub() parallel.Rotate(RotateFlipType.RotateNoneFlipX, images), "Rotate, parallel")
        End Sub

        Private Shared Function LoadImages(ByVal maxImages As Integer) As IList(Of Bitmap)
            Dim paths As IEnumerable(Of String) = SampleUtilities.GetImageFilenames(Directory.GetCurrentDirectory(), maxImages)
            Dim images As IList(Of Bitmap) = New List(Of Bitmap)()
            Dim i As Integer = 0
            For Each img In paths
                images.Add(New Bitmap(Path.Combine(img)))
                Dim tempVar As Boolean = i > maxImages
                i += 1
                If tempVar Then
                    Exit For
                End If
            Next img
            Return images
        End Function
    End Class
End Namespace
