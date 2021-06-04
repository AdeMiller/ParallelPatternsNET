'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Drawing.Drawing2D
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities
Imports System.IO

Namespace Microsoft.Practices.ParallelGuideSamples.ImageBlender
    Friend Class Program
        Private Shared Sub SetToGray(ByVal source As Bitmap, ByVal layer As Bitmap)
            source.CopyPixels(layer)
            layer.SetGray()
            layer.SetAlpha(128)
        End Sub

        Private Shared Sub Rotate(ByVal source As Bitmap, ByVal layer As Bitmap)
            source.CopyPixels(layer)
            layer.RotateFlip(RotateFlipType.Rotate90FlipNone)
            layer.SetAlpha(128)
        End Sub

        ' Alpha blend: call DrawImage on each layer in turn
        Private Shared Sub Blend(ByVal layer1 As Bitmap, ByVal layer2 As Bitmap, ByVal blender As Graphics)
            blender.DrawImage(layer1, 0, 0)
            blender.DrawImage(layer2, 0, 0)
        End Sub

        Private Shared Function SeqentialImageProcessing(ByVal source1 As Bitmap, ByVal source2 As Bitmap, ByVal layer1 As Bitmap, ByVal layer2 As Bitmap, ByVal blender As Graphics) As Integer
            SetToGray(source1, layer1)
            Rotate(source2, layer2)
            Blend(layer1, layer2, blender)
            Return source1.Width
        End Function

        Private Shared Function ParallelTaskImageProcessing(ByVal source1 As Bitmap, ByVal source2 As Bitmap, ByVal layer1 As Bitmap, ByVal layer2 As Bitmap, ByVal blender As Graphics) As Integer
            Dim toGray As Task = Task.Factory.StartNew(Sub() SetToGray(source1, layer1))
            Dim rotate As Task = Task.Factory.StartNew(Sub() Program.Rotate(source2, layer2))
            Task.WaitAll(toGray, rotate)
            Blend(layer1, layer2, blender)
            Return source1.Width
        End Function

        Private Shared Function ParallelInvokeImageProcessing(ByVal source1 As Bitmap, ByVal source2 As Bitmap, ByVal layer1 As Bitmap, ByVal layer2 As Bitmap, ByVal blender As Graphics) As Integer
            Parallel.Invoke(Sub() SetToGray(source1, layer1), Sub() Rotate(source2, layer2))
            Blend(layer1, layer2, blender)
            Return source1.Width
        End Function

        ''' <summary>
        ''' Parallel tasks sample
        ''' Command line arguments are:
        '''  image source directory, default: current directory
        '''  first source image file, default: flowers.jpg 
        '''  second source image file, default: dog.jpg 
        '''  blended image destination directory, default: current directory  
        ''' If any of the directories or files do not exist, the program exits without results.
        ''' </summary>
        <STAThread()> _
        Shared Sub Main(ByVal args() As String)
            Console.WriteLine("Image Blender Sample" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            Dim sourceDir As String = Directory.GetCurrentDirectory()
            Dim file1 As String = "flowers.jpg" ' don't rotate
            Dim file2 As String = "dog.jpg" ' don't set to gray
            Dim destDir As String = Directory.GetCurrentDirectory()

            If args.Length > 0 Then
                sourceDir = args(0)
            End If
            If args.Length > 1 Then
                file1 = args(1)
            End If
            If args.Length > 2 Then
                file2 = args(2)
            End If
            If args.Length > 3 Then
                destDir = args(3)
            End If

            Dim path1 As String = Path.Combine(sourceDir, file1)
            Dim path2 As String = Path.Combine(sourceDir, file2)

            SampleUtilities.CheckDirectoryExists(sourceDir)
            SampleUtilities.CheckFileExists(path1)
            SampleUtilities.CheckFileExists(path2)
            SampleUtilities.CheckDirectoryExists(destDir)

            ' Load source images
            Dim source1 = New Bitmap(path1)
            Dim source2 = New Bitmap(path2)

            ' Prepare for result image
            Dim layer1 = New Bitmap(source1.Width, source1.Height) ' new layer apparently includes alpha layer...
            Dim layer2 = New Bitmap(source2.Width, source2.Height) '... even when source does not.

            Using result = New Bitmap(source1.Width, source1.Height)
                Dim blender = Graphics.FromImage(result)
                blender.CompositingMode = CompositingMode.SourceOver ' NOT SourceCopy mode

                ' Sequential
                SampleUtilities.TimedRun(Function() SeqentialImageProcessing(source1, source2, layer1, layer2, blender), "       Sequential")

                ' restore layers to iniital condition; layer2 must be unrotated
                layer1 = New Bitmap(source1.Width, source1.Height)
                layer2 = New Bitmap(source2.Width, source2.Height)

                ' Parallel tasks
                SampleUtilities.TimedRun(Function() ParallelTaskImageProcessing(source1, source2, layer1, layer2, blender), "   Parallel tasks")

                ' restore layers to initial condition; layer2 must be unrotated
                layer1 = New Bitmap(source1.Width, source1.Height)
                layer2 = New Bitmap(source2.Width, source2.Height)

                ' Parallel invoke
                SampleUtilities.TimedRun(Function() ParallelInvokeImageProcessing(source1, source2, layer1, layer2, blender), "  Parallel invoke")

                ' Save blended image in file
                result.Save(Path.Combine(destDir, "blended.jpg"))

                ' Show blended image on screen, pause until user closes window
                Console.WriteLine("Close image window to exit program.")
                Using form = New Form() ' ensure disposal, prevent warning CA2000
                    Using pb = New PictureBox()
                        pb.SizeMode = PictureBoxSizeMode.AutoSize ' fit to image - but form is initially smaller
                        pb.Image = result
                        form.Controls.Add(pb)
                        form.ShowDialog()
                    End Using
                End Using
            End Using ' using result
        End Sub
    End Class
End Namespace
