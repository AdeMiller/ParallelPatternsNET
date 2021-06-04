'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Globalization
Imports System.IO
Imports System.Threading

Namespace Microsoft.Practices.ParallelGuideSamples.Utilities
    ''' <summary>
    ''' Static class that contains timing and numerical utilities
    ''' </summary>
    Public NotInheritable Class SampleUtilities
#Region "Timing utilities"

        ''' <summary>
        ''' Format and print elapsed time returned by Stopwatch
        ''' </summary>
        Private Sub New()
        End Sub
        Public Shared Sub PrintTime(ByVal ts As TimeSpan)
            Console.WriteLine(FormattedTime(ts))
        End Sub

        ''' <summary>
        ''' TimeSpan pretty printer
        ''' </summary>
        ''' <param name="ts">The TimeSpan to format</param>
        ''' <returns>A formatted string</returns>
        Public Shared Function FormattedTime(ByVal ts As TimeSpan) As String
            Return String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds \ 10)
        End Function

        ''' <summary>
        ''' Executes a function and prints timing results
        ''' </summary>
        Public Shared Sub TimedAction(ByVal test As Action, ByVal label As String)
            Console.WriteLine("Starting {0}", label)
            Dim stopWatch As New Stopwatch()
            stopWatch.Start()

            test()

            stopWatch.Stop()
            Dim seqT As TimeSpan = stopWatch.Elapsed
            Console.WriteLine("{0}: {1}", label, FormattedTime(seqT))
            Console.WriteLine()
        End Sub

        ''' <summary>
        ''' Executes a function and prints timing results
        ''' </summary>
        Public Shared Sub TimedRun(Of T)(ByVal test As Func(Of T), ByVal label As String)
            Dim stopWatch As New Stopwatch()
            stopWatch.Start()

            Dim result = test()

            stopWatch.Stop()
            Dim seqT As TimeSpan = stopWatch.Elapsed
            Console.WriteLine("{0} (result={1}): {2}", label, result.ToString(), FormattedTime(seqT))
        End Sub

        ''' <summary>
        ''' Simulates a CPU-intensive operation on a single core. The operation will use approximately 100% of a
        ''' single CPU for a specified duration.
        ''' </summary>
        ''' <param name="seconds">The approximate duration of the operation in seconds</param>
        ''' <returns>true if operation completed normally; false if the user canceled the operation</returns>
        Public Shared Function DoCpuIntensiveOperation(ByVal seconds As Double) As Boolean
            Return DoCpuIntensiveOperation(seconds, CancellationToken.None, False)
        End Function

        ''' <summary>
        ''' Simulates a CPU-intensive operation on a single core. The operation will use approximately 100% of a
        ''' single CPU for a specified duration.
        ''' </summary>
        ''' <param name="seconds">The approximate duration of the operation in seconds</param>
        ''' <param name="token">A token that may signal a request to cancel the operation.</param>
        ''' <param name="throwOnCancel">true if an execption should be thrown in response to a cancellation request.</param>
        ''' <returns>true if operation completed normally; false if the user canceled the operation</returns>
        Public Shared Function DoCpuIntensiveOperation(ByVal seconds As Double, ByVal token As CancellationToken, Optional ByVal throwOnCancel As Boolean = False) As Boolean
            If token.IsCancellationRequested Then
                If throwOnCancel Then
                    token.ThrowIfCancellationRequested()
                End If
                Return False
            End If

            Dim ms As Long = CLng(Fix(seconds * 1000))
            Dim sw As New Stopwatch()
            sw.Start()
            Dim checkInterval As Long = Math.Min(20000000, CLng(Fix(20000000 * seconds)))

            ' loop to simulate a computationally intensive operation
            Dim i As Integer = 0
            Do
                i += 1

                ' periodically check to see if the user has requested cancellation 
                ' or if the time limit has passed
                If seconds = 0.0R OrElse i Mod checkInterval = 0 Then
                    If token.IsCancellationRequested Then
                        If throwOnCancel Then
                            token.ThrowIfCancellationRequested()
                        End If
                        Return False
                    End If

                    If sw.ElapsedMilliseconds > ms Then
                        Return True
                    End If
                End If
            Loop
        End Function

        ' vary to simulate I/O jitter
        Private Shared ReadOnly SleepTimeouts() As Integer = {65, 165, 110, 110, 185, 160, 40, 125, 275, 110, 80, 190, 70, 165, 80, 50, 45, 155, 100, 215, 85, 115, 180, 195, 135, 265, 120, 60, 130, 115, 200, 105, 310, 100, 100, 135, 140, 235, 205, 10, 95, 175, 170, 90, 145, 230, 365, 340, 160, 190, 95, 125, 240, 145, 75, 105, 155, 125, 70, 325, 300, 175, 155, 185, 255, 210, 130, 120, 55, 225, 120, 65, 400, 290, 205, 90, 250, 245, 145, 85, 140, 195, 215, 220, 130, 60, 140, 150, 90, 35, 230, 180, 200, 165, 170, 75, 280, 150, 260, 105}

        ''' <summary>
        ''' Simulates an I/O-intensive operation on a single core. The operation will use only a small percent of a
        ''' single CPU's cycles; however, it will block for the specified number of seconds.
        ''' </summary>
        ''' <param name="seconds">The approximate duration of the operation in seconds</param>
        ''' <param name="token">A token that may signal a request to cancel the operation.</param>
        ''' <param name="throwOnCancel">true if an execption should be thrown in response to a cancellation request.</param>
        ''' <returns>true if operation completed normally; false if the user canceled the operation</returns>
        Public Shared Function DoIoIntensiveOperation(ByVal seconds As Double, ByVal token As CancellationToken, Optional ByVal throwOnCancel As Boolean = False) As Boolean
            If token.IsCancellationRequested Then
                Return False
            End If
            Dim ms As Integer = CInt(Fix(seconds * 1000))
            Dim sw As New Stopwatch()
            sw.Start()
            Dim timeoutCount As Integer = SleepTimeouts.Length

            ' loop to simulate i/o intensive operation
            Dim i As Integer = (Math.Abs(sw.GetHashCode()) Mod timeoutCount)
            Do
                Dim timeout As Integer = SleepTimeouts(i)
                i += 1
                i = i Mod timeoutCount

                ' simulate i/o latency
                Thread.Sleep(timeout)

                ' Has the user requested cancellation? 
                If token.IsCancellationRequested Then
                    If throwOnCancel Then
                        token.ThrowIfCancellationRequested()
                    End If
                    Return False
                End If

                ' Is the computation finished?
                If sw.ElapsedMilliseconds > ms Then
                    Return True
                End If
            Loop
        End Function

#End Region

#Region "File utilities"

        ''' <summary>
        ''' Check whether directory exists, if not write message and exit immediately.
        ''' </summary>
        ''' <param name="dirName">Directory name</param>
        Public Shared Sub CheckDirectoryExists(ByVal dirName As String)
            If Not Directory.Exists(dirName) Then
                Console.WriteLine("Directory does not exist: {0}", dirName)
                Environment.Exit(0)
            End If
        End Sub

        ''' <summary>
        ''' Check whether file exists, if not write message and exit immediately.
        ''' (can't use this method to check whether directory exists)
        ''' </summary>
        ''' <param name="path">Fully qualified file name including directory</param>
        Public Shared Sub CheckFileExists(ByVal path As String)
            If Not File.Exists(path) Then
                Console.WriteLine("File does not exist: {0}", path)
                Environment.Exit(0)
            End If
        End Sub

        ''' <summary>
        ''' Repeatedly loop through all of the files in the source directory. This
        ''' enumerable has an infinite number of values.
        ''' </summary>
        ''' <param name="sourceDir"></param>
        ''' <param name="maxImages"></param>
        ''' <returns></returns>
        Public Shared Function GetImageFilenames(ByVal sourceDir As String, ByVal maxImages As Integer) As IEnumerable(Of String)
            Return New ImageFilenamesCycleCollection(sourceDir, maxImages)
        End Function

        ''' <summary>
        ''' Get names of image files in directory
        ''' </summary>
        ''' <param name="sourceDir">Name of directory</param>
        ''' <param name="maxImages">Maximum number of image file names to return</param>
        ''' <returns>List of image file names in directory (basenames not including directory path)</returns>
        Private Shared Function GetImageFilenamesList(ByVal sourceDir As String, ByVal maxImages As Integer) As IEnumerable(Of String)
            Dim fileNames As New List(Of String)()
            Dim dirInfo = New DirectoryInfo(sourceDir)

            For Each file As FileInfo In dirInfo.GetFiles()
                If file.Extension.ToUpper(CultureInfo.InvariantCulture) = ".JPG" Then ' LIMITATION - only handles jpg, not gif, png etc.
                    fileNames.Add(file.Name)
                End If
            Next
            Return fileNames.Take(Math.Min(maxImages, fileNames.Count)).OrderBy(Function(f) f).ToList()
        End Function

#End Region

#Region "Numerical Routines"

        ''' <summary>
        ''' Return array of floats for indices 0 .. count-1
        ''' </summary>
        Public Shared Function Range(ByVal count As Integer) As Double()
            If count < 0 Then
                Throw New ArgumentOutOfRangeException("count")
            End If

            Dim x(count - 1) As Double
            For i As Integer = 0 To count - 1
                x(i) = i
            Next i
            Return x
        End Function

        ''' <summary>
        ''' Linear regression with x-values given implicity by the y-value indices
        ''' </summary>
        ''' <param name="ordinateValues">A series of two or more values</param>
        ''' <returns>A trend line</returns>
        Public Shared Function Fit(ByVal ordinateValues() As Double) As Trend
            If ordinateValues Is Nothing Then
                Throw New ArgumentNullException("ordinateValues")
            End If
            ' special case - x values are just the indices of the y's
            Return Fit(Range(ordinateValues.Length), ordinateValues)
        End Function

        ''' <summary>
        ''' Linear regression of (x, y) pairs
        ''' </summary>
        ''' <param name="abscissaValues">The x values</param>
        ''' <param name="ordinateValues">The y values corresponding to each x value</param>
        ''' <returns>A trend line that best predicts each (x, y) pair</returns>
        Public Shared Function Fit(ByVal abscissaValues() As Double, ByVal ordinateValues() As Double) As Trend
            If abscissaValues Is Nothing Then
                Throw New ArgumentNullException("abscissaValues")
            End If
            If ordinateValues Is Nothing Then
                Throw New ArgumentNullException("ordinateValues")
            End If
            If abscissaValues.Length <> ordinateValues.Length Then
                Throw New ArgumentException("abscissaValues and ordinateValues must contain the same number of values.")
            End If
            If abscissaValues.Length < 2 Then
                Throw New ArgumentException("abscissaValues must contain at least two elements")
            End If

            Dim xx As Double = 0, xy As Double = 0
            Dim abscissaMean As Double = abscissaValues.Average()
            Dim ordinateMean As Double = ordinateValues.Average()

            ' calculate the sum of squared differences
            For i As Integer = 0 To abscissaValues.Length - 1
                Dim xi As Double = abscissaValues(i) - abscissaMean
                xx += xi * xi
                xy += xi * (ordinateValues(i) - ordinateMean)
            Next i

            If xx = 0.0R Then
                Throw New ArgumentException("abscissaValues must not all be coincident")
            End If
            Dim slope As Double = xy / xx
            Return New Trend With {.Slope = slope, .Intercept = ordinateMean - slope * abscissaMean}
        End Function

        ''' <summary>
        ''' Calculates an approximation of the inverse of the cumulative normal distribution.
        ''' </summary>
        ''' <param name="cumulativeDistribution">The percentile as a fraction (.50 is the fiftieth percentile). 
        ''' Must be greater than 0 and less than 1.</param>
        ''' <param name="mean">The underlying distribution's average (i.e., the value at the 50th percentile) (</param>
        ''' <param name="standardDeviation">The distribution's standard deviation</param>
        ''' <returns>The value whose cumulative normal distribution (given mean and stddev) is the percentile given as an argument.</returns>
        Public Shared Function GaussianInverse(ByVal cumulativeDistribution As Double, ByVal mean As Double, ByVal standardDeviation As Double) As Double
            If Not (0.0 < cumulativeDistribution AndAlso cumulativeDistribution < 1.0) Then
                Throw New ArgumentOutOfRangeException("cumulativeDistribution")
            End If

            Dim result As Double = GaussianInverse(cumulativeDistribution)
            Return mean + result * standardDeviation
        End Function

        ' Adaptation of Peter J. Acklam's Perl implementation. See http://home.online.no/~pjacklam/notes/invnorm/
        ' This approximation has a relative error of 1.15 × 10−9 or less. 
        Private Shared Function GaussianInverse(ByVal value As Double) As Double
            ' Lower and upper breakpoints
            Const plow As Double = 0.02425
            Const phigh As Double = 1.0 - plow

            Dim p As Double = If((phigh < value), 1.0 - value, value)
            Dim sign As Double = If((phigh < value), -1.0, 1.0)
            Dim q As Double

            If p < plow Then
                ' Rational approximation for tail
                Dim c = New Double() {-0.0077848940024302926, -0.32239645804113648, -2.4007582771618381, -2.5497325393437338, 4.3746641414649678, 2.9381639826987831}

                Dim d = New Double() {0.0077846957090414622, 0.32246712907003983, 2.445134137142996, 3.7544086619074162}
                q = Math.Sqrt(-2 * Math.Log(p))
                Return sign * (((((c(0) * q + c(1)) * q + c(2)) * q + c(3)) * q + c(4)) * q + c(5)) / ((((d(0) * q + d(1)) * q + d(2)) * q + d(3)) * q + 1)
            Else
                ' Rational approximation for central region
                Dim a = New Double() {-39.696830286653757, 220.9460984245205, -275.92851044696869, 138.357751867269, -30.66479806614716, 2.5066282774592392}

                Dim b = New Double() {-54.476098798224058, 161.58583685804089, -155.69897985988661, 66.80131188771972, -13.280681552885721}
                q = p - 0.5
                Dim r = q * q
                Return (((((a(0) * r + a(1)) * r + a(2)) * r + a(3)) * r + a(4)) * r + a(5)) * q / (((((b(0) * r + b(1)) * r + b(2)) * r + b(3)) * r + b(4)) * r + 1)
            End If
        End Function

#End Region

#Region "Other Utilities"

        ''' <summary>
        ''' Creates a seed that does not depend on the system clock. A unique value will be created with each invocation.
        ''' </summary>
        ''' <returns>An integer that can be used to seed a random generator</returns>
        ''' <remarks>This method is thread safe.</remarks>
        Public Shared Function MakeRandomSeed() As Integer
            Return Guid.NewGuid().ToString().GetHashCode()
        End Function
#End Region

        ''' <summary>
        ''' A class that supports iterating files in the target directory repeatedly.
        ''' </summary>
        Public Class ImageFilenamesCycleCollection
            Implements IEnumerable(Of String)
            Implements IEnumerator(Of String)

            Public Sub New(ByVal sourceDir As String, ByVal maxImages As Integer)
                _filenames = CType(GetImageFilenamesList(sourceDir, maxImages), List(Of String))
            End Sub

            Private _filenames As List(Of String) = Nothing

            Public ReadOnly Property FileName() As List(Of String)
                Get
                    Return _filenames
                End Get
            End Property

            Private indexPos As Integer = -1

            Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of String) Implements System.Collections.Generic.IEnumerable(Of String).GetEnumerator
                Return Me
            End Function

            Public ReadOnly Property Current As String Implements System.Collections.Generic.IEnumerator(Of String).Current
                Get
                    Return _filenames(indexPos)
                End Get
            End Property

            Public Function MoveNext() As Boolean Implements System.Collections.Generic.IEnumerator(Of String).MoveNext
                If indexPos = (_filenames.Count - 1) Then
                    indexPos = 0
                Else
                    indexPos = indexPos + 1
                End If
                Return True
            End Function

            Public Sub Reset() Implements System.Collections.IEnumerator.Reset
                indexPos = -1
            End Sub

#Region "IDisposable Support"
            Private disposedValue As Boolean ' To detect redundant calls

            ' IDisposable
            Protected Overridable Sub Dispose(ByVal disposing As Boolean)
                If Not Me.disposedValue Then
                    If disposing Then
                        ' TODO: dispose managed state (managed objects).
                        _filenames.Clear()
                        _filenames = Nothing
                    End If
                End If
                Me.disposedValue = True
            End Sub

            ' This code added by Visual Basic to correctly implement the disposable pattern.
            Public Sub Dispose() Implements IDisposable.Dispose
                ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub
#End Region


            Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
                Return GetEnumerator()
            End Function


            Public ReadOnly Property Current1 As Object Implements System.Collections.IEnumerator.Current
                Get
                    Return Current
                End Get
            End Property
        End Class

    End Class
End Namespace
