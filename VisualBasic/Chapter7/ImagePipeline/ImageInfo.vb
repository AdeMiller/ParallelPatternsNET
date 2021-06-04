'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Diagnostics.CodeAnalysis

Namespace Microsoft.Practices.ParallelGuideSamples.ImagePipeline
    Public Class ImageInfo
        Implements IDisposable
        ' Image data

        Private privateSequenceNumber As Integer
        Public Property SequenceNumber() As Integer
            Get
                Return privateSequenceNumber
            End Get
            Private Set(ByVal value As Integer)
                privateSequenceNumber = value
            End Set
        End Property
        Private privateFileName As String
        Public Property FileName() As String
            Get
                Return privateFileName
            End Get
            Private Set(ByVal value As String)
                privateFileName = value
            End Set
        End Property
        Public Property OriginalImage() As Bitmap
        Public Property ThumbnailImage() As Bitmap
        Public Property FilteredImage() As Bitmap

        ' Image pipeline performance data

        Private privateClockOffset As Integer
        Public Property ClockOffset() As Integer
            Get
                Return privateClockOffset
            End Get
            Private Set(ByVal value As Integer)
                privateClockOffset = value
            End Set
        End Property
        Private privatePhaseStartTick As Integer()
        <SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")>
        Public Property PhaseStartTick() As Integer()
            Get
                Return privatePhaseStartTick
            End Get
            Private Set(ByVal value As Integer())
                privatePhaseStartTick = value
            End Set
        End Property
        Private privatePhaseEndTick As Integer()
        <SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")>
        Public Property PhaseEndTick() As Integer()
            Get
                Return privatePhaseEndTick
            End Get
            Private Set(ByVal value As Integer())
                privatePhaseEndTick = value
            End Set
        End Property

        Public Property QueueCount1() As Integer
        Public Property QueueCount2() As Integer
        Public Property QueueCount3() As Integer

        Public Property ImageCount() As Integer
        Public Property FramesPerSecond() As Double

        Public Sub New(ByVal sequenceNumber As Integer, ByVal fileName As String, ByVal originalImage As Bitmap, ByVal clockOffset As Integer)
            Me.SequenceNumber = sequenceNumber
            Me.FileName = fileName
            Me.OriginalImage = originalImage
            Me.ClockOffset = clockOffset

            PhaseStartTick = CType(Array.CreateInstance(GetType(Integer), 4), Integer())
            PhaseEndTick = CType(Array.CreateInstance(GetType(Integer), 4), Integer())
        End Sub

#Region "IDisposable Members"

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If OriginalImage IsNot Nothing Then
                    OriginalImage.Dispose()
                    OriginalImage = Nothing
                End If
                If ThumbnailImage IsNot Nothing Then
                    ThumbnailImage.Dispose()
                    ThumbnailImage = Nothing
                End If
                If FilteredImage IsNot Nothing Then
                    FilteredImage.Dispose()
                    FilteredImage = Nothing
                End If
            End If
        End Sub

#End Region
    End Class
End Namespace
