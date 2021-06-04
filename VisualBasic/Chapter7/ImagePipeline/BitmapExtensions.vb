'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.ImagePipeline
    ''' <summary>
    ''' Extension methods for System.Drawing.Bitmap
    ''' </summary>
    Public Module BitmapExtensions
        ''' <summary>
        ''' Creates an image with a border from this image.
        ''' </summary>
        ''' <param name="source">Color image (Bitmap)</param>
        ''' <param name="borderWidth">Width of border</param>
        ''' <returns>Image with border</returns>
        ''' <remarks>
        ''' This code uses Bitmap.GetPixel and SetPixel methods for clarity. An implementation using Bitmap.LockBits
        ''' and then directly modifying the image data may be faster, espectially for large images.
        ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> _
        Public Function AddBorder(ByVal source As Bitmap, ByVal borderWidth As Integer) As Bitmap
            If source Is Nothing Then
                Throw New ArgumentNullException("source")
            End If
            Dim _bitmap As Bitmap = Nothing
            Dim tempBitmap As Bitmap = Nothing
            Try
                Dim width As Integer = source.Width
                Dim height As Integer = source.Height
                tempBitmap = New Bitmap(width, height)
                For y As Integer = 0 To height - 1
                    Dim yFlag As Boolean = (y < borderWidth OrElse (height - y) < borderWidth)
                    For x As Integer = 0 To width - 1
                        Dim xFlag As Boolean = (x < borderWidth OrElse (width - x) < borderWidth)
                        If xFlag OrElse yFlag Then
                            Dim distance = Math.Min(y, Math.Min(height - y, Math.Min(x, width - x)))
                            Dim percent = distance / CDbl(borderWidth)
                            Dim percent2 = percent * percent
                            Dim pixel = source.GetPixel(x, y)
                            Dim color = System.Drawing.Color.FromArgb(CInt(Fix(pixel.R * percent2)), CInt(Fix(pixel.G * percent2)), CInt(Fix(pixel.B * percent2)))
                            tempBitmap.SetPixel(x, y, color)
                        Else
                            tempBitmap.SetPixel(x, y, source.GetPixel(x, y))
                        End If
                    Next x
                Next y
                _bitmap = tempBitmap
                tempBitmap = Nothing
            Finally
                If tempBitmap IsNot Nothing Then
                    tempBitmap.Dispose()
                End If
            End Try
            Return _bitmap
        End Function

        ''' <summary>
        ''' Inserts Gaussian noise into a bitmap.
        ''' </summary>
        ''' <param name="source">Bitmap to be processed</param>
        ''' <param name="amount">Standard deviation of perturbation for each color channel.</param>
        ''' <returns>New, speckled bitmap</returns>
        ''' <remarks>
        ''' This code uses Bitmap.GetPixel and SetPixel methods for clarity. An implementation using Bitmap.LockBits
        ''' and then directly modifying the image data may be faster, espectially for large images.
        ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> _
        Public Function AddNoise(ByVal source As Bitmap, ByVal amount As Double) As Bitmap
            If source Is Nothing Then
                Throw New ArgumentNullException("source")
            End If
            Dim _bitmap As Bitmap = Nothing
            Dim tempBitmap As Bitmap = Nothing
            Try
                Dim generator = New GaussianRandom(0.0, amount, SampleUtilities.MakeRandomSeed())
                tempBitmap = New Bitmap(source.Width, source.Height)
                For y As Integer = 0 To tempBitmap.Height - 1
                    For x As Integer = 0 To tempBitmap.Width - 1
                        Dim pixel = source.GetPixel(x, y)
                        Dim newPixel As Color = AddPixelNoise(pixel, generator)
                        tempBitmap.SetPixel(x, y, newPixel)
                    Next x
                Next y
                _bitmap = tempBitmap
                tempBitmap = Nothing
            Finally
                If tempBitmap IsNot Nothing Then
                    tempBitmap.Dispose()
                End If
            End Try
            Return _bitmap
        End Function

        Private Function AddPixelNoise(ByVal pixel As Color, ByVal generator As GaussianRandom) As Color
            Dim newR As Integer = CInt(Fix(pixel.R)) + generator.NextInteger()
            Dim newG As Integer = CInt(Fix(pixel.G)) + generator.NextInteger()
            Dim newB As Integer = CInt(Fix(pixel.B)) + generator.NextInteger()
            Dim r As Integer = Math.Max(0, Math.Min(newR, 255))
            Dim g As Integer = Math.Max(0, Math.Min(newG, 255))
            Dim b As Integer = Math.Max(0, Math.Min(newB, 255))
            Return Color.FromArgb(r, g, b)
        End Function
    End Module
End Namespace

