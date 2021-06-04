'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================


Namespace Microsoft.Practices.ParallelGuideSamples.ImageBlender
    ''' <summary>
    ''' Extension methods for System.Drawing.Bitmap
    ''' </summary>
    Public Module BitmapExtensions
        ''' <summary>
        ''' Copy all source pixels to destination.
        ''' Destination must already exist, must not be smaller than source in either dimension.
        ''' </summary>
        ''' <param name="source">source Bitmap, possibly without alpha layer</param>
        ''' <param name="destination">destination Bitmap, possibly with alpha layer</param>
        ''' <remarks>
        ''' This code uses Bitmap.GetPixel and SetPixel methods for clarity. An implementation using Bitmap.LockBits
        ''' and then directly modifying the image data may be faster, espectially for large images.
        ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> _
        Public Sub CopyPixels(ByVal source As Bitmap, ByVal destination As Bitmap)
            If source Is Nothing Then
                Throw New ArgumentNullException("source")
            End If
            If destination Is Nothing Then
                Throw New ArgumentNullException("destination")
            End If
            For y As Integer = 0 To source.Height - 1
                For x As Integer = 0 To source.Width - 1
                    Dim p = source.GetPixel(x, y)
                    destination.SetPixel(x, y, Color.FromArgb(p.R, p.G, p.B)) ' apparently preserves alpha
                Next x
            Next y
        End Sub

        ''' <summary>
        ''' Assign same alpha (transparency) to all pixels in image
        ''' </summary>
        ''' <param name="bitmap">existing image where alpha is assigned</param>
        ''' <param name="alpha">transparency to assign: 256 opaque, 0 invisible</param>
        ''' <remarks>
        ''' This code uses Bitmap.GetPixel and SetPixel methods for clarity. An implementation using Bitmap.LockBits
        ''' and then directly modifying the image data may be faster, espectially for large images.
        ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> _
        Public Sub SetAlpha(ByVal bitmap As Bitmap, ByVal alpha As Integer)
            If bitmap Is Nothing Then
                Throw New ArgumentNullException("bitmap")
            End If
            For x As Integer = 0 To bitmap.Width - 1
                For y As Integer = 0 To bitmap.Height - 1
                    Dim p = bitmap.GetPixel(x, y)
                    p = Color.FromArgb(alpha, p.R, p.G, p.B)
                    bitmap.SetPixel(x, y, p)
                Next y
            Next x
        End Sub

        ''' <summary>
        ''' Set a color image to gray
        ''' </summary>
        ''' <param name="bitmap">existing color image to set gray</param>
        ''' <remarks>
        ''' This code uses Bitmap.GetPixel and SetPixel methods for clarity. An implementation using Bitmap.LockBits
        ''' and then directly modifying the image data may be faster, espectially for large images.
        ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> _
        Public Sub SetGray(ByVal bitmap As Bitmap)
            If bitmap Is Nothing Then
                Throw New ArgumentNullException("bitmap")
            End If
            For y As Integer = 0 To bitmap.Height - 1
                For x As Integer = 0 To bitmap.Width - 1
                    Dim pixel = bitmap.GetPixel(x, y)
                    Dim luma As Integer = CInt(Fix(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11))
                    bitmap.SetPixel(x, y, Color.FromArgb(luma, luma, luma))
                Next x
            Next y
        End Sub
    End Module
End Namespace
