'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.Concurrent
Imports System.Diagnostics.CodeAnalysis
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.ImagePipeline
    Friend NotInheritable Class ImagePipeline
        Private Const QueueBoundedCapacity As Integer = 4
        Private Const LoadBalancingDegreeOfConcurrency As Integer = 2
        Private Const MaxNumberOfImages As Integer = 500
        Private Const GaussianNoiseAmount As Double = 50.0

#Region "Image Pipeline Top Level Loop"

        ''' <summary>
        ''' Runs the image pipeline example. The program goes through the jpg images located in the SourceDir
        ''' directory and performs a series of steps: it resizes each image and adds a black border and then applies
        ''' a Gaussian noise filter operation to give the image a grainy effect. Finally, the program invokes 
        ''' a user-provided delegate to the image (for example, to display the image on the user interface).
        ''' 
        ''' Images are processed in sequential order. That is, the display delegate will be invoked in exactly the same
        ''' order as the images appear in the file system.
        ''' </summary>
        ''' <param name="displayFn">A delegate that is invoked for each image at the end of the pipeline, for example, to 
        ''' display the image in the user interface.</param>
        ''' <param name="token">A token that can signal an external cancellation request.</param>
        ''' <param name="algorithmChoice">The method of calculation. 0=sequential, 1=pipeline, 2=load balanced pipeline</param>
        ''' <param name="errorFn">A delegate that will be invoked if this method or any of its parallel subtasks observe an exception during their execution.</param>
        Private Sub New()
        End Sub
        <SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")>
        Public Shared Sub ImagePipelineMainLoop(ByVal displayFn As Action(Of ImageInfo), ByVal token As CancellationToken, ByVal algorithmChoice As Integer, ByVal errorFn As Action(Of Exception))
            Try
                Dim sourceDir As String = Directory.GetCurrentDirectory()

                ' Ensure that frames are presented in sequence before invoking the user-provided display function.
                Dim imagesSoFar As Integer = 0
                Dim safeDisplayFn As Action(Of ImageInfo) = Sub(info)
                                                                If info.SequenceNumber <> imagesSoFar Then
                                                                    Throw New InvalidOperationException("Images processed out of order. Saw " & info.SequenceNumber.ToString() & " , expected " & imagesSoFar)
                                                                End If

                                                                displayFn(info)
                                                                imagesSoFar += 1
                                                            End Sub

                ' Create a cancellation handle for inter-task signaling of exceptions. This cancellation
                ' handle is also triggered by the incoming token that indicates user-requested
                ' cancellation.
                Using cts As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token)
                    Dim fileNames As IEnumerable(Of String) = SampleUtilities.GetImageFilenames(sourceDir, MaxNumberOfImages)
                    Select Case algorithmChoice
                        Case 0
                            RunSequential(fileNames, sourceDir, safeDisplayFn, cts)
                        Case 1
                            RunPipelined(fileNames, sourceDir, QueueBoundedCapacity, safeDisplayFn, cts)
                        Case 2
                            RunLoadBalancedPipeline(fileNames, sourceDir, QueueBoundedCapacity, safeDisplayFn, cts, LoadBalancingDegreeOfConcurrency)
                        Case Else
                            Throw New InvalidOperationException("Invalid algorithm choice.")
                    End Select
                End Using
            Catch ae As AggregateException
                errorFn(If(ae.InnerExceptions.Count = 1, ae.InnerExceptions(0), ae))
            Catch e As Exception
                errorFn(e)
            End Try
        End Sub

#End Region

#Region "Variations (Sequential and Pipelined)"

        ''' <summary>
        ''' Run the image processing pipeline.
        ''' </summary>
        ''' <param name="fileNames">List of image file names in source directory</param>
        ''' <param name="sourceDir">Name of directory of source images</param>
        ''' <param name="displayFn">Display action</param>
        ''' <param name="cts">Cancellation token</param>
        Private Shared Sub RunSequential(ByVal fileNames As IEnumerable(Of String), ByVal sourceDir As String, ByVal displayFn As Action(Of ImageInfo), ByVal cts As CancellationTokenSource)
            Dim count As Integer = 0
            Dim clockOffset As Integer = Environment.TickCount
            Dim duration As Integer = 0
            Dim token = cts.Token
            Dim info As ImageInfo = Nothing
            Try
                For Each fileName In fileNames
                    If token.IsCancellationRequested Then
                        Exit For
                    End If

                    info = LoadImage(fileName, sourceDir, count, clockOffset)
                    ScaleImage(info)
                    FilterImage(info)
                    Dim displayStart As Integer = Environment.TickCount
                    DisplayImage(info, count + 1, displayFn, duration)
                    duration = Environment.TickCount - displayStart

                    count += 1
                    info = Nothing
                Next fileName
            Finally
                If info IsNot Nothing Then
                    info.Dispose()
                End If
            End Try
        End Sub

        ''' <summary>
        ''' Run the image processing pipeline.
        ''' </summary>
        ''' <param name="fileNames">List of image file names in source directory</param>
        ''' <param name="sourceDir">Name of directory of source images</param>
        ''' <param name="queueLength">Length of image queue</param>
        ''' <param name="displayFn">Display action</param>
        ''' <param name="cts">Cancellation token</param>
        Private Shared Sub RunPipelined(ByVal fileNames As IEnumerable(Of String), ByVal sourceDir As String, ByVal queueLength As Integer, ByVal displayFn As Action(Of ImageInfo), ByVal cts As CancellationTokenSource)
            ' Data pipes 
            Dim originalImages = New BlockingCollection(Of ImageInfo)(queueLength)
            Dim thumbnailImages = New BlockingCollection(Of ImageInfo)(queueLength)
            Dim filteredImages = New BlockingCollection(Of ImageInfo)(queueLength)
            Try
                Dim f = New TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None)
                Dim updateStatisticsFn As Action(Of ImageInfo) = Sub(info)
                                                                     info.QueueCount1 = originalImages.Count()
                                                                     info.QueueCount2 = thumbnailImages.Count()
                                                                     info.QueueCount3 = filteredImages.Count()
                                                                 End Sub

                ' Start pipelined tasks
                Dim loadTask = f.StartNew(Sub() LoadPipelinedImages(fileNames, sourceDir, originalImages, cts))

                Dim scaleTask = f.StartNew(Sub() ScalePipelinedImages(originalImages, thumbnailImages, cts))

                Dim filterTask = f.StartNew(Sub() FilterPipelinedImages(thumbnailImages, filteredImages, cts))

                Dim displayTask = f.StartNew(Sub() DisplayPipelinedImages(filteredImages.GetConsumingEnumerable(), displayFn, updateStatisticsFn, cts))

                Task.WaitAll(loadTask, scaleTask, filterTask, displayTask)
            Finally
                ' in case of exception or cancellation, there might be bitmaps
                ' that need to be disposed.
                DisposeImagesInQueue(originalImages)
                DisposeImagesInQueue(thumbnailImages)
                DisposeImagesInQueue(filteredImages)
            End Try
        End Sub

        ''' <summary>
        ''' Run a variation of the pipeline that uses a user-specified number of tasks for the filter stage.
        ''' </summary>
        ''' <param name="fileNames">List of image file names in source directory</param>
        ''' <param name="sourceDir">Name of directory of source images</param>
        ''' <param name="queueLength">Length of image queue</param>
        ''' <param name="displayFn">Display action</param>
        ''' <param name="cts">Cancellation token</param>
        ''' <param name="filterTaskCount">Number of filter tasks</param>
        Private Shared Sub RunLoadBalancedPipeline(ByVal fileNames As IEnumerable(Of String), ByVal sourceDir As String, ByVal queueLength As Integer, ByVal displayFn As Action(Of ImageInfo), ByVal cts As CancellationTokenSource, ByVal filterTaskCount As Integer)
            ' Create data pipes 
            Dim originalImages = New BlockingCollection(Of ImageInfo)(queueLength)
            Dim thumbnailImages = New BlockingCollection(Of ImageInfo)(queueLength)
            Dim filteredImageMultiplexer = New BlockingMultiplexer(Of ImageInfo)(Function(info) info.SequenceNumber, 0, queueLength)
            Dim filteredImagesCollections = CType(Array.CreateInstance(GetType(BlockingCollection(Of ImageInfo)), filterTaskCount), BlockingCollection(Of ImageInfo)())

            Try
                ' Start pipelined tasks
                Dim updateStatisticsFn As Action(Of ImageInfo) = Sub(info)
                                                                     info.QueueCount1 = originalImages.Count()
                                                                     info.QueueCount2 = thumbnailImages.Count()
                                                                     info.QueueCount3 = filteredImageMultiplexer.Count
                                                                 End Sub
                Const options As TaskCreationOptions = TaskCreationOptions.LongRunning
                Dim f = New TaskFactory(CancellationToken.None, options, TaskContinuationOptions.None, TaskScheduler.Default)
                Dim tasks() As Task = CType(Array.CreateInstance(GetType(Task), filterTaskCount + 3), Task())
                Dim taskId As Integer = 0

                tasks(taskId) = f.StartNew(Sub() LoadPipelinedImages(fileNames, sourceDir, originalImages, cts))
                taskId += 1

                tasks(taskId) = f.StartNew(Sub() ScalePipelinedImages(originalImages, thumbnailImages, cts))
                taskId += 1

                For i As Integer = 0 To filterTaskCount - 1
                    Dim tmp = i
                    filteredImagesCollections(tmp) = filteredImageMultiplexer.GetProducerQueue()
                    tasks(taskId) = f.StartNew(Sub() FilterPipelinedImages(thumbnailImages, filteredImagesCollections(tmp), cts))
                    taskId += 1
                Next i

                tasks(taskId) = f.StartNew(Sub() DisplayPipelinedImages(filteredImageMultiplexer.GetConsumingEnumerable(), displayFn, updateStatisticsFn, cts))
                taskId += 1

                Task.WaitAll(tasks)
            Finally
                ' there might be cleanup in the case of cancellation or an exception.
                DisposeImagesInQueue(originalImages)
                DisposeImagesInQueue(thumbnailImages)
                For Each filteredImages In filteredImagesCollections
                    DisposeImagesInQueue(filteredImages)
                Next filteredImages
                For Each info In filteredImageMultiplexer.GetCleanupEnumerable()
                    info.Dispose()
                Next info
            End Try
        End Sub

#End Region

#Region "The Pipeline Phases"

        ''' <summary>
        ''' Image pipeline phase 1: Load images from disk and put them a queue.
        ''' </summary>
        Private Shared Sub LoadPipelinedImages(ByVal fileNames As IEnumerable(Of String), ByVal sourceDir As String, ByVal original As BlockingCollection(Of ImageInfo), ByVal cts As CancellationTokenSource)
            Dim count As Integer = 0
            Dim clockOffset As Integer = Environment.TickCount
            Dim token = cts.Token
            Dim info As ImageInfo = Nothing
            Try
                For Each fileName In fileNames
                    If token.IsCancellationRequested Then
                        Exit For
                    End If
                    info = LoadImage(fileName, sourceDir, count, clockOffset)
                    original.Add(info, token)
                    count += 1
                    info = Nothing
                Next fileName
            Catch e As Exception
                ' in case of exception, signal shutdown to other pipeline tasks
                cts.Cancel()
                If Not (TypeOf e Is OperationCanceledException) Then
                    Throw
                End If
            Finally
                original.CompleteAdding()
                If info IsNot Nothing Then
                    info.Dispose()
                End If
            End Try
        End Sub

        ''' <summary>
        ''' Image pipeline phase 2: Scale to thumbnail size and render picture frame.
        ''' </summary>
        Private Shared Sub ScalePipelinedImages(ByVal originalImages As BlockingCollection(Of ImageInfo), ByVal thumbnailImages As BlockingCollection(Of ImageInfo), ByVal cts As CancellationTokenSource)
            Dim token = cts.Token
            Dim info As ImageInfo = Nothing
            Try
                For Each infoTmp In originalImages.GetConsumingEnumerable()
                    info = infoTmp
                    If token.IsCancellationRequested Then
                        Exit For
                    End If
                    ScaleImage(info)
                    thumbnailImages.Add(info, token)
                    info = Nothing
                Next infoTmp
            Catch e As Exception
                cts.Cancel()
                If Not (TypeOf e Is OperationCanceledException) Then
                    Throw
                End If
            Finally
                thumbnailImages.CompleteAdding()
                If info IsNot Nothing Then
                    info.Dispose()
                End If
            End Try
        End Sub

        ''' <summary>
        ''' Image pipeline phase 3: Filter images (give them a speckled appearance by adding Gaussian noise)
        ''' </summary>
        Private Shared Sub FilterPipelinedImages(ByVal thumbnailImages As BlockingCollection(Of ImageInfo), ByVal filteredImages As BlockingCollection(Of ImageInfo), ByVal cts As CancellationTokenSource)
            Dim info As ImageInfo = Nothing
            Try
                Dim token = cts.Token
                For Each infoTmp As ImageInfo In thumbnailImages.GetConsumingEnumerable()
                    info = infoTmp
                    If token.IsCancellationRequested Then
                        Exit For
                    End If
                    FilterImage(info)
                    filteredImages.Add(info, token)
                    info = Nothing
                Next infoTmp
            Catch e As Exception
                cts.Cancel()
                If Not (TypeOf e Is OperationCanceledException) Then
                    Throw
                End If
            Finally
                filteredImages.CompleteAdding()
                If info IsNot Nothing Then
                    info.Dispose()
                End If
            End Try
        End Sub

        ''' <summary>
        ''' Image pipeline phase 4: Invoke the user-provided delegate (for example, to display the result in a UI)
        ''' </summary>
        Private Shared Sub DisplayPipelinedImages(ByVal filteredImages As IEnumerable(Of ImageInfo), ByVal displayFn As Action(Of ImageInfo), ByVal updateStatisticsFn As Action(Of ImageInfo), ByVal cts As CancellationTokenSource)
            Dim count As Integer = 1
            Dim duration As Integer = 0
            Dim token = cts.Token
            Dim info As ImageInfo = Nothing
            Try
                For Each infoTmp As ImageInfo In filteredImages
                    info = infoTmp
                    If token.IsCancellationRequested Then
                        Exit For
                    End If
                    Dim displayStart As Integer = Environment.TickCount
                    updateStatisticsFn(info)
                    DisplayImage(info, count, displayFn, duration)
                    duration = Environment.TickCount - displayStart

                    count = count + 1
                    info = Nothing
                Next infoTmp
            Catch e As Exception
                cts.Cancel()
                If Not (TypeOf e Is OperationCanceledException) Then
                    Throw
                End If
            Finally
                If info IsNot Nothing Then
                    info.Dispose()
                End If
            End Try
        End Sub

#End Region

#Region "Operations for Individual Images"

        <SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")>
        Private Shared Function LoadImage(ByVal fname As String, ByVal sourceDir As String, ByVal count As Integer, ByVal clockOffset As Integer) As ImageInfo
            Dim startTick As Integer = Environment.TickCount
            Dim info As ImageInfo = Nothing
            Dim _bitmap As New Bitmap(Path.Combine(sourceDir, fname))
            Try
                _bitmap.Tag = fname

                info = New ImageInfo(count, fname, _bitmap, clockOffset)
                info.PhaseStartTick(0) = startTick - clockOffset
                _bitmap = Nothing
            Finally
                If _bitmap IsNot Nothing Then
                    _bitmap.Dispose()
                End If
            End Try

            If info IsNot Nothing Then
                info.PhaseEndTick(0) = Environment.TickCount - clockOffset
            End If
            Return info
        End Function

        Private Shared Sub ScaleImage(ByVal info As ImageInfo)
            Dim startTick As Integer = Environment.TickCount
            Dim orig = info.OriginalImage
            info.OriginalImage = Nothing
            Const scale As Integer = 200
            Dim isLandscape = (orig.Width > orig.Height)
            Dim newWidth = If(isLandscape, scale, scale * orig.Width \ orig.Height)
            Dim newHeight = If((Not isLandscape), scale, scale * orig.Height \ orig.Width)
            Dim _bitmap As New Bitmap(orig, newWidth, newHeight)
            Try
                Dim bitmap2 As Bitmap = _bitmap.AddBorder(15)
                Try
                    bitmap2.Tag = orig.Tag
                    info.ThumbnailImage = bitmap2
                    info.PhaseStartTick(1) = startTick - info.ClockOffset
                    bitmap2 = Nothing
                Finally
                    If bitmap2 IsNot Nothing Then
                        bitmap2.Dispose()
                    End If
                End Try
            Finally
                _bitmap.Dispose()
                orig.Dispose()
            End Try
            info.PhaseEndTick(1) = Environment.TickCount - info.ClockOffset
        End Sub

        Private Shared Sub FilterImage(ByVal info As ImageInfo)
            Dim startTick As Integer = Environment.TickCount
            Dim sc = info.ThumbnailImage
            info.ThumbnailImage = Nothing
            Dim _bitmap As Bitmap = sc.AddNoise(GaussianNoiseAmount)

            Try
                _bitmap.Tag = sc.Tag
                info.FilteredImage = _bitmap
                info.PhaseStartTick(2) = startTick - info.ClockOffset

                _bitmap = Nothing
            Finally
                If _bitmap IsNot Nothing Then
                    _bitmap.Dispose()
                End If
                sc.Dispose()
            End Try
            info.PhaseEndTick(2) = Environment.TickCount - info.ClockOffset
        End Sub

        Private Shared Sub DisplayImage(ByVal info As ImageInfo, ByVal count As Integer, ByVal displayFn As Action(Of ImageInfo), ByVal duration As Integer)
            Dim startTick As Integer = Environment.TickCount
            info.ImageCount = count
            info.PhaseStartTick(3) = startTick - info.ClockOffset
            info.PhaseEndTick(3) = If(duration > 0, startTick - info.ClockOffset + duration, Environment.TickCount - info.ClockOffset)
            displayFn(info)
        End Sub

#End Region

#Region "Cleanup methods used by error handling"

        ' Ensure that the queue contents is disposed. You could also implement this by 
        ' subclassing BlockingCollection<> and providing an IDisposable implmentation.
        Private Shared Sub DisposeImagesInQueue(ByVal queue As BlockingCollection(Of ImageInfo))
            If queue IsNot Nothing Then
                queue.CompleteAdding()
                For Each info In queue
                    info.Dispose()
                Next info
            End If
        End Sub

#End Region
    End Class
End Namespace
