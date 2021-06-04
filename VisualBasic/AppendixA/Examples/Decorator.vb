'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks

Namespace Microsoft.Practices.ParallelGuideSamples.RelatedPatterns
#Region "Calculate example"

    Public Interface IImageEditor
        Sub Rotate(ByVal rotation As RotateFlipType, ByVal images As IEnumerable(Of Bitmap))
    End Interface

    Public Class SerialEditor
        Implements IImageEditor
        Public Sub Rotate(ByVal rotation As RotateFlipType, ByVal images As IEnumerable(Of Bitmap)) Implements IImageEditor.Rotate
            For Each b As Bitmap In images
                b.RotateFlip(rotation)
            Next b
        End Sub
    End Class

    Public Class ParallelEditor
        Implements IImageEditor
        Private decorated As IImageEditor

        Public Sub New(ByVal decorated As IImageEditor)
            Me.decorated = decorated
        End Sub

        ' Modified behavior
        Public Sub Rotate(ByVal rotation As RotateFlipType, ByVal images As IEnumerable(Of Bitmap)) Implements IImageEditor.Rotate
            If decorated Is Nothing Then
                Return
            End If
            Parallel.ForEach(images, Sub(b) b.RotateFlip(rotation))
        End Sub

        ' Additional behavior...
    End Class

#End Region
End Namespace
