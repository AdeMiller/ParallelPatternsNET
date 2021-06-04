'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading
Imports System.Threading.Tasks

Namespace Microsoft.Practices.ParallelGuideSamples.ImagePipeline
    ' This sample does not use the MVP pattern to separate concerns of the view from
    ' the model. This is largerly for reasons of simplicity in the sample code.
    ' For further discussion of MVVM and MVP patterns see Appendix A. For an example
    ' of MVVM is a WPF application see the Chapter 5 code.
    '
    Partial Public Class MainForm
        Inherits Form
        Private Enum ImageMode
            Sequential
            Pipelined
            LoadBalanced
        End Enum
        Private Enum Mode
            Stopped
            Running
            Stopping
        End Enum

        Private Delegate Sub UserCallback(ByVal o As Object)
        Private ReadOnly updateBitmapDelegate As UserCallback
        Private ReadOnly cancelFinishedDelegate As UserCallback
        Private ReadOnly showErrorDelegate As UserCallback

        Private mainTask As Task = Nothing
        Private cts As CancellationTokenSource = Nothing
        Private image_mode As ImageMode = ImageMode.Pipelined
        Private _mode As Mode = Mode.Stopped
        Private ReadOnly sw As New Stopwatch()

        Private imagesSoFar As Integer = 0
        Private ReadOnly totalTime() As Integer = {0, 0, 0, 0, 0, 0, 0, 0}

        Public Sub New()
            InitializeComponent()
            cancelFinishedDelegate = New UserCallback(Sub(o)
                                                          Me.cts = Nothing
                                                          Me.mainTask = Nothing
                                                          Me._mode = Mode.Stopped
                                                          UpdateEnabledStatus()
                                                      End Sub)

            updateBitmapDelegate = New UserCallback(AddressOf SetBitmap)

            showErrorDelegate = New UserCallback(Sub(obj)
                                                     Dim e As Exception = CType(obj, Exception)
                                                     MessageBox.Show(e.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
                                                     Me.cts = Nothing
                                                     Me.mainTask = Nothing
                                                     Me._mode = Mode.Stopped
                                                     UpdateEnabledStatus()
                                                 End Sub)

            UpdateEnabledStatus()
        End Sub

        Private Sub UpdateEnabledStatus()
            radioButtonSequential.Enabled = (_mode = Mode.Stopped)
            radioButtonPipeline.Enabled = (_mode = Mode.Stopped)
            radioButtonLoadBalanced.Enabled = (_mode = Mode.Stopped)
            buttonStart.Enabled = (_mode = Mode.Stopped)
            buttonStop.Enabled = (_mode = Mode.Running)
            quitButton.Enabled = (_mode = Mode.Stopped)
            radioButtonSequential.Checked = (image_mode = ImageMode.Sequential)
            radioButtonPipeline.Checked = (image_mode = ImageMode.Pipelined)
            radioButtonLoadBalanced.Checked = (image_mode = ImageMode.LoadBalanced)
        End Sub

        Private Sub SetBitmap(ByVal info As Object)
            Dim priorImage = Me.pictureBox1.Image
            Dim imageInfo = CType(info, ImageInfo)
            Me.pictureBox1.Image = imageInfo.FilteredImage
            imageInfo.FilteredImage = Nothing
            Me.imagesSoFar += 1

            ' calculate duration of each phase
            For i As Integer = 0 To 3
                Me.totalTime(i) += imageInfo.PhaseEndTick(i) - imageInfo.PhaseStartTick(i)
            Next i
            ' infer queue wait times by comparing phase(n+1) start with phase(n) finish timestamp
            Me.totalTime(4) += imageInfo.PhaseStartTick(1) - imageInfo.PhaseEndTick(0)
            Me.totalTime(5) += imageInfo.PhaseStartTick(2) - imageInfo.PhaseEndTick(1)
            Me.totalTime(6) += imageInfo.PhaseStartTick(3) - imageInfo.PhaseEndTick(2)

            Me.textBoxPhase1AvgTime.Text = (Me.totalTime(0) \ Me.imagesSoFar).ToString()
            Me.textBoxPhase2AvgTime.Text = (Me.totalTime(1) \ Me.imagesSoFar).ToString()
            Me.textBoxPhase3AvgTime.Text = (Me.totalTime(2) \ Me.imagesSoFar).ToString()
            Me.textBoxPhase4AvgTime.Text = (Me.totalTime(3) \ Me.imagesSoFar).ToString()

            Me.textBoxQueue1AvgWait.Text = (Me.totalTime(4) \ Me.imagesSoFar).ToString()
            Me.textBoxQueue2AvgWait.Text = (Me.totalTime(5) \ Me.imagesSoFar).ToString()
            Me.textBoxQueue3AvgWait.Text = (Me.totalTime(6) \ Me.imagesSoFar).ToString()

            Me.textBoxQueueCount1.Text = imageInfo.QueueCount1.ToString()
            Me.textBoxQueueCount2.Text = imageInfo.QueueCount2.ToString()
            Me.textBoxQueueCount3.Text = imageInfo.QueueCount3.ToString()

            Me.textBoxFileName.Text = imageInfo.FileName
            Me.textBoxImageCount.Text = imageInfo.ImageCount.ToString()

            Dim elapsedTime As Long = Me.sw.ElapsedMilliseconds
            Me.textBoxFps.Text = String.Format("{0: 0}", elapsedTime / imageInfo.ImageCount)

            If priorImage IsNot Nothing Then
                priorImage.Dispose()
            End If

            If imageInfo.SequenceNumber <> imagesSoFar - 1 Then
                Dim msg = String.Format("Program error-- images are out of order. Expected {0} but received {1}", imagesSoFar - 1, imageInfo.SequenceNumber)
                MessageBox.Show(msg, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
                Application.Exit()
            End If
        End Sub

        Private Sub quitButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles quitButton.Click
            Application.Exit()
        End Sub

        Private Sub buttonStart_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonStart.Click
            If mainTask Is Nothing Then
                _mode = Mode.Running
                UpdateEnabledStatus()
                Dim updateFn As Action(Of ImageInfo) = Sub(bm As ImageInfo) Me.Invoke(Me.updateBitmapDelegate, CObj(bm))
                Dim errorFn As Action(Of Exception) = Sub(ex As Exception) Me.Invoke(Me.showErrorDelegate, CObj(ex))
                cts = New CancellationTokenSource()
                Dim enumVal As Integer = CInt(Fix(image_mode))
                Me.sw.Restart()
                imagesSoFar = 0
                For i As Integer = 0 To totalTime.Length - 1
                    totalTime(i) = 0
                Next i
                mainTask = Task.Factory.StartNew(Sub() ImagePipeline.ImagePipelineMainLoop(updateFn, cts.Token, enumVal, errorFn), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
            End If
        End Sub

        Private Sub radioButtonSequential_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonSequential.CheckedChanged
            If radioButtonSequential.Checked Then
                image_mode = ImageMode.Sequential
            End If
        End Sub

        Private Sub radioButtonPipeline_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonPipeline.CheckedChanged
            If radioButtonPipeline.Checked Then
                image_mode = ImageMode.Pipelined
            End If
        End Sub

        Private Sub radioButtonLoadBalanced_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonLoadBalanced.CheckedChanged
            If radioButtonLoadBalanced.Checked Then
                image_mode = ImageMode.LoadBalanced
            End If
        End Sub

        Private Sub buttonStop_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonStop.Click
            If cts IsNot Nothing Then
                _mode = Mode.Stopping
                UpdateEnabledStatus()
                cts.Cancel()

                Task.Factory.StartNew(Sub()
                                          mainTask.Wait()
                                          Me.Invoke(cancelFinishedDelegate, Me)
                                      End Sub)
            End If
        End Sub
    End Class
End Namespace
