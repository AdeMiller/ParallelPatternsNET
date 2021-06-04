'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Namespace Microsoft.Practices.ParallelGuideSamples.ImagePipeline
    Partial Public Class MainForm
        ''' <summary>
        ''' Required designer variable.
        ''' </summary>
        Private components As System.ComponentModel.IContainer = Nothing

        ''' <summary>
        ''' Clean up any resources being used.
        ''' </summary>
        ''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso (components IsNot Nothing) Then
                components.Dispose()
            End If
            If cts IsNot Nothing Then
                cts.Dispose()
                cts = Nothing
            End If
            MyBase.Dispose(disposing)
        End Sub

#Region "Windows Form Designer generated code"

        ''' <summary>
        ''' Required method for Designer support - do not modify
        ''' the contents of this method with the code editor.
        ''' </summary>
        Private Sub InitializeComponent()
			Me.pictureBox1 = New System.Windows.Forms.PictureBox()
			Me.quitButton = New System.Windows.Forms.Button()
			Me.textBoxPhase1AvgTime = New System.Windows.Forms.TextBox()
			Me.textBoxPhase2AvgTime = New System.Windows.Forms.TextBox()
			Me.textBoxPhase3AvgTime = New System.Windows.Forms.TextBox()
			Me.textBoxPhase4AvgTime = New System.Windows.Forms.TextBox()
			Me.textBoxFileName = New System.Windows.Forms.TextBox()
			Me.textBoxImageCount = New System.Windows.Forms.TextBox()
			Me.textBoxFps = New System.Windows.Forms.TextBox()
			Me.textBoxQueue1AvgWait = New System.Windows.Forms.TextBox()
			Me.textBoxQueue2AvgWait = New System.Windows.Forms.TextBox()
			Me.textBoxQueue3AvgWait = New System.Windows.Forms.TextBox()
			Me.textBoxQueueCount1 = New System.Windows.Forms.TextBox()
			Me.textBoxQueueCount2 = New System.Windows.Forms.TextBox()
			Me.textBoxQueueCount3 = New System.Windows.Forms.TextBox()
			Me.label1 = New System.Windows.Forms.Label()
			Me.label3 = New System.Windows.Forms.Label()
			Me.label4 = New System.Windows.Forms.Label()
			Me.label5 = New System.Windows.Forms.Label()
			Me.label6 = New System.Windows.Forms.Label()
			Me.label7 = New System.Windows.Forms.Label()
			Me.label8 = New System.Windows.Forms.Label()
			Me.label9 = New System.Windows.Forms.Label()
			Me.label11 = New System.Windows.Forms.Label()
			Me.buttonStart = New System.Windows.Forms.Button()
			Me.radioButtonSequential = New System.Windows.Forms.RadioButton()
			Me.radioButtonPipeline = New System.Windows.Forms.RadioButton()
			Me.radioButtonLoadBalanced = New System.Windows.Forms.RadioButton()
			Me.groupBox1 = New System.Windows.Forms.GroupBox()
			Me.buttonStop = New System.Windows.Forms.Button()
			Me.label12 = New System.Windows.Forms.Label()
			Me.label13 = New System.Windows.Forms.Label()
			Me.groupBox2 = New System.Windows.Forms.GroupBox()
			Me.label2 = New System.Windows.Forms.Label()
			Me.label14 = New System.Windows.Forms.Label()
			Me.label10 = New System.Windows.Forms.Label()
			CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
			Me.groupBox1.SuspendLayout()
			Me.groupBox2.SuspendLayout()
			Me.SuspendLayout()
			'
			'pictureBox1
			'
			Me.pictureBox1.Location = New System.Drawing.Point(12, 12)
			Me.pictureBox1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.pictureBox1.Name = "pictureBox1"
			Me.pictureBox1.Size = New System.Drawing.Size(319, 251)
			Me.pictureBox1.TabIndex = 0
			Me.pictureBox1.TabStop = False
			'
			'quitButton
			'
			Me.quitButton.Location = New System.Drawing.Point(619, 304)
			Me.quitButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.quitButton.Name = "quitButton"
			Me.quitButton.Size = New System.Drawing.Size(75, 23)
			Me.quitButton.TabIndex = 1
			Me.quitButton.Text = "Quit"
			Me.quitButton.UseVisualStyleBackColor = True
			'
			'textBoxPhase1AvgTime
			'
			Me.textBoxPhase1AvgTime.Location = New System.Drawing.Point(67, 38)
			Me.textBoxPhase1AvgTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxPhase1AvgTime.Name = "textBoxPhase1AvgTime"
			Me.textBoxPhase1AvgTime.ReadOnly = True
			Me.textBoxPhase1AvgTime.Size = New System.Drawing.Size(63, 20)
			Me.textBoxPhase1AvgTime.TabIndex = 2
			'
			'textBoxPhase2AvgTime
			'
			Me.textBoxPhase2AvgTime.Location = New System.Drawing.Point(67, 66)
			Me.textBoxPhase2AvgTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxPhase2AvgTime.Name = "textBoxPhase2AvgTime"
			Me.textBoxPhase2AvgTime.ReadOnly = True
			Me.textBoxPhase2AvgTime.Size = New System.Drawing.Size(63, 20)
			Me.textBoxPhase2AvgTime.TabIndex = 2
			'
			'textBoxPhase3AvgTime
			'
			Me.textBoxPhase3AvgTime.Location = New System.Drawing.Point(67, 94)
			Me.textBoxPhase3AvgTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxPhase3AvgTime.Name = "textBoxPhase3AvgTime"
			Me.textBoxPhase3AvgTime.ReadOnly = True
			Me.textBoxPhase3AvgTime.Size = New System.Drawing.Size(63, 20)
			Me.textBoxPhase3AvgTime.TabIndex = 2
			'
			'textBoxPhase4AvgTime
			'
			Me.textBoxPhase4AvgTime.Location = New System.Drawing.Point(67, 122)
			Me.textBoxPhase4AvgTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxPhase4AvgTime.Name = "textBoxPhase4AvgTime"
			Me.textBoxPhase4AvgTime.ReadOnly = True
			Me.textBoxPhase4AvgTime.Size = New System.Drawing.Size(63, 20)
			Me.textBoxPhase4AvgTime.TabIndex = 2
			'
			'textBoxFileName
			'
			Me.textBoxFileName.Location = New System.Drawing.Point(12, 267)
			Me.textBoxFileName.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxFileName.Name = "textBoxFileName"
			Me.textBoxFileName.ReadOnly = True
			Me.textBoxFileName.Size = New System.Drawing.Size(319, 20)
			Me.textBoxFileName.TabIndex = 3
			'
			'textBoxImageCount
			'
			Me.textBoxImageCount.Location = New System.Drawing.Point(349, 267)
			Me.textBoxImageCount.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxImageCount.Name = "textBoxImageCount"
			Me.textBoxImageCount.ReadOnly = True
			Me.textBoxImageCount.Size = New System.Drawing.Size(63, 20)
			Me.textBoxImageCount.TabIndex = 2
			'
			'textBoxFps
			'
			Me.textBoxFps.Location = New System.Drawing.Point(631, 267)
			Me.textBoxFps.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxFps.Name = "textBoxFps"
			Me.textBoxFps.ReadOnly = True
			Me.textBoxFps.Size = New System.Drawing.Size(63, 20)
			Me.textBoxFps.TabIndex = 2
			'
			'textBoxQueue1AvgWait
			'
			Me.textBoxQueue1AvgWait.Location = New System.Drawing.Point(496, 64)
			Me.textBoxQueue1AvgWait.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxQueue1AvgWait.Name = "textBoxQueue1AvgWait"
			Me.textBoxQueue1AvgWait.ReadOnly = True
			Me.textBoxQueue1AvgWait.Size = New System.Drawing.Size(63, 20)
			Me.textBoxQueue1AvgWait.TabIndex = 2
			Me.textBoxQueue1AvgWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
			'
			'textBoxQueue2AvgWait
			'
			Me.textBoxQueue2AvgWait.Location = New System.Drawing.Point(496, 92)
			Me.textBoxQueue2AvgWait.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxQueue2AvgWait.Name = "textBoxQueue2AvgWait"
			Me.textBoxQueue2AvgWait.ReadOnly = True
			Me.textBoxQueue2AvgWait.Size = New System.Drawing.Size(63, 20)
			Me.textBoxQueue2AvgWait.TabIndex = 2
			Me.textBoxQueue2AvgWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
			'
			'textBoxQueue3AvgWait
			'
			Me.textBoxQueue3AvgWait.Location = New System.Drawing.Point(496, 121)
			Me.textBoxQueue3AvgWait.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxQueue3AvgWait.Name = "textBoxQueue3AvgWait"
			Me.textBoxQueue3AvgWait.ReadOnly = True
			Me.textBoxQueue3AvgWait.Size = New System.Drawing.Size(63, 20)
			Me.textBoxQueue3AvgWait.TabIndex = 2
			Me.textBoxQueue3AvgWait.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
			'
			'textBoxQueueCount1
			'
			Me.textBoxQueueCount1.Location = New System.Drawing.Point(636, 63)
			Me.textBoxQueueCount1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxQueueCount1.Name = "textBoxQueueCount1"
			Me.textBoxQueueCount1.ReadOnly = True
			Me.textBoxQueueCount1.Size = New System.Drawing.Size(63, 20)
			Me.textBoxQueueCount1.TabIndex = 2
			Me.textBoxQueueCount1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
			'
			'textBoxQueueCount2
			'
			Me.textBoxQueueCount2.Location = New System.Drawing.Point(636, 91)
			Me.textBoxQueueCount2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxQueueCount2.Name = "textBoxQueueCount2"
			Me.textBoxQueueCount2.ReadOnly = True
			Me.textBoxQueueCount2.Size = New System.Drawing.Size(63, 20)
			Me.textBoxQueueCount2.TabIndex = 2
			Me.textBoxQueueCount2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
			'
			'textBoxQueueCount3
			'
			Me.textBoxQueueCount3.Location = New System.Drawing.Point(636, 119)
			Me.textBoxQueueCount3.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.textBoxQueueCount3.Name = "textBoxQueueCount3"
			Me.textBoxQueueCount3.ReadOnly = True
			Me.textBoxQueueCount3.Size = New System.Drawing.Size(63, 20)
			Me.textBoxQueueCount3.TabIndex = 2
			Me.textBoxQueueCount3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
			'
			'label1
			'
			Me.label1.AutoSize = True
			Me.label1.Location = New System.Drawing.Point(20, 41)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(31, 13)
			Me.label1.TabIndex = 4
			Me.label1.Text = "Load"
			Me.label1.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'label3
			'
			Me.label3.AutoSize = True
			Me.label3.Location = New System.Drawing.Point(564, 95)
			Me.label3.Name = "label3"
			Me.label3.Size = New System.Drawing.Size(48, 13)
			Me.label3.TabIndex = 4
			Me.label3.Text = "Queue 2"
			'
			'label4
			'
			Me.label4.AutoSize = True
			Me.label4.Location = New System.Drawing.Point(564, 123)
			Me.label4.Name = "label4"
			Me.label4.Size = New System.Drawing.Size(48, 13)
			Me.label4.TabIndex = 4
			Me.label4.Text = "Queue 3"
			'
			'label5
			'
			Me.label5.AutoSize = True
			Me.label5.Location = New System.Drawing.Point(9, 69)
			Me.label5.Name = "label5"
			Me.label5.Size = New System.Drawing.Size(39, 13)
			Me.label5.TabIndex = 4
			Me.label5.Text = "Resize"
			Me.label5.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'label6
			'
			Me.label6.AutoSize = True
			Me.label6.Location = New System.Drawing.Point(21, 97)
			Me.label6.Name = "label6"
			Me.label6.Size = New System.Drawing.Size(29, 13)
			Me.label6.TabIndex = 4
			Me.label6.Text = "Filter"
			Me.label6.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'label7
			'
			Me.label7.AutoSize = True
			Me.label7.Location = New System.Drawing.Point(564, 68)
			Me.label7.Name = "label7"
			Me.label7.Size = New System.Drawing.Size(48, 13)
			Me.label7.TabIndex = 4
			Me.label7.Text = "Queue 1"
			'
			'label8
			'
			Me.label8.AutoSize = True
			Me.label8.Location = New System.Drawing.Point(9, 122)
			Me.label8.Name = "label8"
			Me.label8.Size = New System.Drawing.Size(41, 13)
			Me.label8.TabIndex = 4
			Me.label8.Text = "Display"
			Me.label8.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'label9
			'
			Me.label9.AutoSize = True
			Me.label9.Location = New System.Drawing.Point(8, 17)
			Me.label9.Name = "label9"
			Me.label9.Size = New System.Drawing.Size(104, 13)
			Me.label9.TabIndex = 4
			Me.label9.Text = "Time Per Phase (ms)"
			'
			'label11
			'
			Me.label11.AutoSize = True
			Me.label11.Location = New System.Drawing.Point(556, 246)
			Me.label11.Name = "label11"
			Me.label11.Size = New System.Drawing.Size(103, 13)
			Me.label11.TabIndex = 5
			Me.label11.Text = "Time Per Image (ms)"
			Me.label11.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'buttonStart
			'
			Me.buttonStart.Location = New System.Drawing.Point(432, 304)
			Me.buttonStart.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.buttonStart.Name = "buttonStart"
			Me.buttonStart.Size = New System.Drawing.Size(75, 23)
			Me.buttonStart.TabIndex = 6
			Me.buttonStart.Text = "Start"
			Me.buttonStart.UseVisualStyleBackColor = True
			'
			'radioButtonSequential
			'
			Me.radioButtonSequential.AutoSize = True
			Me.radioButtonSequential.Location = New System.Drawing.Point(5, 17)
			Me.radioButtonSequential.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.radioButtonSequential.Name = "radioButtonSequential"
			Me.radioButtonSequential.Size = New System.Drawing.Size(75, 17)
			Me.radioButtonSequential.TabIndex = 7
			Me.radioButtonSequential.Text = "Sequential"
			Me.radioButtonSequential.UseVisualStyleBackColor = True
			'
			'radioButtonPipeline
			'
			Me.radioButtonPipeline.AutoSize = True
			Me.radioButtonPipeline.Location = New System.Drawing.Point(108, 17)
			Me.radioButtonPipeline.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.radioButtonPipeline.Name = "radioButtonPipeline"
			Me.radioButtonPipeline.Size = New System.Drawing.Size(68, 17)
			Me.radioButtonPipeline.TabIndex = 7
			Me.radioButtonPipeline.Text = "Pipelined"
			Me.radioButtonPipeline.UseVisualStyleBackColor = True
			'
			'radioButtonLoadBalanced
			'
			Me.radioButtonLoadBalanced.AutoSize = True
			Me.radioButtonLoadBalanced.Location = New System.Drawing.Point(201, 17)
			Me.radioButtonLoadBalanced.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.radioButtonLoadBalanced.Name = "radioButtonLoadBalanced"
			Me.radioButtonLoadBalanced.Size = New System.Drawing.Size(97, 17)
			Me.radioButtonLoadBalanced.TabIndex = 7
			Me.radioButtonLoadBalanced.Text = "Load Balanced"
			Me.radioButtonLoadBalanced.UseVisualStyleBackColor = True
			'
			'groupBox1
			'
			Me.groupBox1.Controls.Add(Me.radioButtonLoadBalanced)
			Me.groupBox1.Controls.Add(Me.radioButtonPipeline)
			Me.groupBox1.Controls.Add(Me.radioButtonSequential)
			Me.groupBox1.Location = New System.Drawing.Point(13, 287)
			Me.groupBox1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.groupBox1.Name = "groupBox1"
			Me.groupBox1.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.groupBox1.Size = New System.Drawing.Size(328, 47)
			Me.groupBox1.TabIndex = 8
			Me.groupBox1.TabStop = False
			'
			'buttonStop
			'
			Me.buttonStop.Location = New System.Drawing.Point(521, 304)
			Me.buttonStop.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.buttonStop.Name = "buttonStop"
			Me.buttonStop.Size = New System.Drawing.Size(79, 23)
			Me.buttonStop.TabIndex = 9
			Me.buttonStop.Text = "Stop"
			Me.buttonStop.UseVisualStyleBackColor = True
			'
			'label12
			'
			Me.label12.AutoSize = True
			Me.label12.Location = New System.Drawing.Point(644, 26)
			Me.label12.Name = "label12"
			Me.label12.Size = New System.Drawing.Size(39, 13)
			Me.label12.TabIndex = 10
			Me.label12.Text = "Queue"
			'
			'label13
			'
			Me.label13.AutoSize = True
			Me.label13.Location = New System.Drawing.Point(660, 43)
			Me.label13.Name = "label13"
			Me.label13.Size = New System.Drawing.Size(27, 13)
			Me.label13.TabIndex = 10
			Me.label13.Text = "Size"
			Me.label13.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'groupBox2
			'
			Me.groupBox2.Controls.Add(Me.label9)
			Me.groupBox2.Controls.Add(Me.label8)
			Me.groupBox2.Controls.Add(Me.label6)
			Me.groupBox2.Controls.Add(Me.label5)
			Me.groupBox2.Controls.Add(Me.label1)
			Me.groupBox2.Controls.Add(Me.textBoxPhase4AvgTime)
			Me.groupBox2.Controls.Add(Me.textBoxPhase3AvgTime)
			Me.groupBox2.Controls.Add(Me.textBoxPhase2AvgTime)
			Me.groupBox2.Controls.Add(Me.textBoxPhase1AvgTime)
			Me.groupBox2.Location = New System.Drawing.Point(337, 14)
			Me.groupBox2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.groupBox2.Name = "groupBox2"
			Me.groupBox2.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.groupBox2.Size = New System.Drawing.Size(152, 160)
			Me.groupBox2.TabIndex = 11
			Me.groupBox2.TabStop = False
			'
			'label2
			'
			Me.label2.AutoSize = True
			Me.label2.Location = New System.Drawing.Point(508, 26)
			Me.label2.Name = "label2"
			Me.label2.Size = New System.Drawing.Size(39, 13)
			Me.label2.TabIndex = 10
			Me.label2.Text = "Queue"
			'
			'label14
			'
			Me.label14.AutoSize = True
			Me.label14.Location = New System.Drawing.Point(492, 44)
			Me.label14.Name = "label14"
			Me.label14.Size = New System.Drawing.Size(55, 13)
			Me.label14.TabIndex = 10
			Me.label14.Text = "Wait Time"
			Me.label14.TextAlign = System.Drawing.ContentAlignment.TopRight
			'
			'label10
			'
			Me.label10.AutoSize = True
			Me.label10.Location = New System.Drawing.Point(352, 243)
			Me.label10.Name = "label10"
			Me.label10.Size = New System.Drawing.Size(41, 13)
			Me.label10.TabIndex = 12
			Me.label10.Text = "Images"
			'
			'MainForm
			'
			Me.ClientSize = New System.Drawing.Size(739, 353)
			Me.Controls.Add(Me.label10)
			Me.Controls.Add(Me.label11)
			Me.Controls.Add(Me.groupBox2)
			Me.Controls.Add(Me.label14)
			Me.Controls.Add(Me.label13)
			Me.Controls.Add(Me.label2)
			Me.Controls.Add(Me.label12)
			Me.Controls.Add(Me.buttonStop)
			Me.Controls.Add(Me.textBoxFps)
			Me.Controls.Add(Me.groupBox1)
			Me.Controls.Add(Me.buttonStart)
			Me.Controls.Add(Me.label4)
			Me.Controls.Add(Me.label7)
			Me.Controls.Add(Me.label3)
			Me.Controls.Add(Me.textBoxFileName)
			Me.Controls.Add(Me.textBoxImageCount)
			Me.Controls.Add(Me.textBoxQueue3AvgWait)
			Me.Controls.Add(Me.textBoxQueue2AvgWait)
			Me.Controls.Add(Me.textBoxQueue1AvgWait)
			Me.Controls.Add(Me.textBoxQueueCount3)
			Me.Controls.Add(Me.textBoxQueueCount2)
			Me.Controls.Add(Me.textBoxQueueCount1)
			Me.Controls.Add(Me.quitButton)
			Me.Controls.Add(Me.pictureBox1)
			Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
			Me.Name = "MainForm"
			Me.Text = "Image Pipeline"
			CType(Me.pictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
			Me.groupBox1.ResumeLayout(False)
			Me.groupBox1.PerformLayout()
			Me.groupBox2.ResumeLayout(False)
			Me.groupBox2.PerformLayout()
			Me.ResumeLayout(False)
			Me.PerformLayout()

		End Sub

#End Region

        Private pictureBox1 As PictureBox
        Private WithEvents quitButton As Button
        Private textBoxPhase1AvgTime As TextBox
        Private textBoxPhase2AvgTime As TextBox
        Private textBoxPhase3AvgTime As TextBox
        Private textBoxPhase4AvgTime As TextBox
        Private textBoxFileName As TextBox
        Private textBoxImageCount As TextBox
        Private textBoxFps As TextBox
        Private textBoxQueue1AvgWait As TextBox
        Private textBoxQueue2AvgWait As TextBox
        Private textBoxQueue3AvgWait As TextBox
        Private textBoxQueueCount1 As TextBox
        Private textBoxQueueCount2 As TextBox
        Private textBoxQueueCount3 As TextBox
        Private label1 As Label
        Private label3 As Label
        Private label4 As Label
        Private label5 As Label
        Private label6 As Label
        Private label7 As Label
        Private label8 As Label
        Private label9 As Label
        Private label11 As Label
        Private WithEvents buttonStart As Button
        Private WithEvents radioButtonSequential As RadioButton
        Private WithEvents radioButtonPipeline As RadioButton
        Private WithEvents radioButtonLoadBalanced As RadioButton
        Private groupBox1 As GroupBox
        Private WithEvents buttonStop As Button
        Private label12 As Label
        Private label13 As Label
        Private groupBox2 As GroupBox
        Private label2 As Label
        Private label14 As Label
        Private label10 As Label
    End Class
End Namespace

