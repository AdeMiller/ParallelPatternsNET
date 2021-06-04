'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.Concurrent
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading.Tasks

Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.BasicPipeline
    Friend Class Program
        Private Shared Phrases() As String = {"the", "<Adjective>", "<Adjective>", "<Noun>", "jumped over the", "<Adjective>", "<Noun>", "."}
        Private Shared Adjectives() As String = {"quick", "brown", "lazy"}
        Private Shared Nouns() As String = {"fox", "dog"}
        Private Const TargetSentence As String = "The quick brown fox jumped over the lazy dog."
        Private Const SuccessString As String = "Surprise!!!"

        Private Const NumberOfSentences As Integer = 1000
        Private Const BufferSize As Integer = 32
        Private Shared StageTime() As Double = {0.0025, 0.0025, 0.0025, 0.0025}
        Private Const PathForSequentialResults As String = ".\Chapter7Sequential.txt"
        Private Const PathForPipelineResults As String = ".\Chapter7Pipeline.txt"

        Shared Sub Main()
            Console.WriteLine("Basic Pipeline Samples" & vbLf)
#If (DEBUG) Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If
            Dim seed As Integer = Environment.TickCount

            SampleUtilities.TimedAction(Sub() Chapter7Example01Sequential(seed), "Write sentences, sequential")
            SampleUtilities.TimedAction(Sub() Chapter7Example01Pipeline(seed), "Write sentences, pipeline")
            CheckResults()

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadKey()
        End Sub

        Private Shared Function PhraseSource(ByVal seed As Integer) As IEnumerable(Of String)
            Dim r As New Random(seed)
            Dim returnValue As New List(Of String)

            For i As Integer = 0 To NumberOfSentences - 1
                For Each line In Phrases
                    If line = "<Adjective>" Then
                        returnValue.Add(Adjectives(r.Next(0, Adjectives.Length)))
                    ElseIf line = "<Noun>" Then
                        returnValue.Add(Nouns(r.Next(0, Nouns.Length)))
                    Else
                        returnValue.Add(line)
                    End If
                Next line
            Next i
            Return returnValue
        End Function

        Private Shared Sub Chapter7Example01Sequential(ByVal seed As Integer)
            Dim isFirstPhrase = True
            Dim sentenceBuilder = New StringBuilder()
            Dim sentenceCount As Integer = 1
            Using outfile As New StreamWriter(PathForSequentialResults)
                Console.Write("Begin Sequential Sentence Builder")
                For Each phrase In PhraseSource(seed)
                    Stage1AdditionalWork()
                    Stage2AdditionalWork()
                    Dim capitalizedPhrase As String = If(isFirstPhrase, (phrase.Substring(0, 1).ToUpper() & phrase.Substring(1)), phrase)
                    Stage3AdditionalWork()
                    If (Not isFirstPhrase) AndAlso phrase <> "." Then
                        sentenceBuilder.Append(" ")
                    End If
                    sentenceBuilder.Append(capitalizedPhrase)
                    isFirstPhrase = False
                    If phrase = "." Then
                        Dim sentence As String = sentenceBuilder.ToString()
                        If sentence = TargetSentence Then
                            sentence = sentence & "       " & SuccessString
                        End If
                        sentenceBuilder.Clear()
                        isFirstPhrase = True
                        Stage4AdditionalWork()
                        outfile.WriteLine(sentenceCount.ToString() & " " & sentence)
                        OutputProgress(sentenceCount)
                        sentenceCount += 1
                    End If
                Next phrase
                Console.WriteLine("End")
            End Using
        End Sub

        Private Shared Sub Chapter7Example01Pipeline(ByVal seed As Integer)
            Console.Write("Begin Pipelined Sentence Builder")

            Dim buffer1 = New BlockingCollection(Of String)(BufferSize)
            Dim buffer2 = New BlockingCollection(Of String)(BufferSize)
            Dim buffer3 = New BlockingCollection(Of String)(BufferSize)

            Dim f = New TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None)

            ' Stage 1: Read strings and merge into sentences
            Dim stage1 = f.StartNew(Sub() ReadStrings(buffer1, seed))

            ' Stage 2: Correct case
            Dim stage2 = f.StartNew(Sub() CorrectCase(buffer1, buffer2))

            ' Stage 3: Merge into sentences
            Dim stage3 = f.StartNew(Sub() CreateSentences(buffer2, buffer3))

            ' Stage 4: Write output
            Dim stage4 = f.StartNew(Sub() WriteSentences(buffer3))

            Task.WaitAll(stage1, stage2, stage3, stage4)
            Console.WriteLine("End")
        End Sub

        Private Shared Sub ReadStrings(ByVal output As BlockingCollection(Of String), ByVal seed As Integer)
            Try
                For Each phrase In PhraseSource(seed)
                    Stage1AdditionalWork()
                    output.Add(phrase)
                Next phrase
            Finally
                output.CompleteAdding()
            End Try
        End Sub

        Private Shared Sub Stage1AdditionalWork()
            SampleUtilities.DoCpuIntensiveOperation(StageTime(0) / Phrases.Length)
        End Sub
        Private Shared Sub Stage2AdditionalWork()
            SampleUtilities.DoCpuIntensiveOperation(StageTime(1) / Phrases.Length)
        End Sub
        Private Shared Sub Stage3AdditionalWork()
            SampleUtilities.DoCpuIntensiveOperation(StageTime(2) / Phrases.Length)
        End Sub
        Private Shared Sub Stage4AdditionalWork()
            SampleUtilities.DoCpuIntensiveOperation(StageTime(3))
        End Sub

        Private Shared Sub CorrectCase(ByVal input As BlockingCollection(Of String), ByVal output As BlockingCollection(Of String))
            Try
                Dim isFirstPhrase As Boolean = True
                For Each phrase In input.GetConsumingEnumerable()
                    Stage2AdditionalWork()
                    If isFirstPhrase Then
                        Dim capitalized = phrase.Substring(0, 1).ToUpper() & phrase.Substring(1)
                        isFirstPhrase = False
                        output.Add(capitalized)
                    Else
                        output.Add(phrase)
                        If phrase = "." Then
                            isFirstPhrase = True
                        End If
                    End If
                Next phrase
            Finally
                output.CompleteAdding()
            End Try
        End Sub

        Private Shared Sub CreateSentences(ByVal input As BlockingCollection(Of String), ByVal output As BlockingCollection(Of String))
            Try
                Dim sentenceBuilder As New StringBuilder()
                Dim isFirstPhrase As Boolean = True
                For Each phrase In input.GetConsumingEnumerable()
                    Stage3AdditionalWork()
                    If (Not isFirstPhrase AndAlso phrase <> ".") Then
                        sentenceBuilder.Append(" ")
                    End If
                    sentenceBuilder.Append(phrase)
                    isFirstPhrase = False
                    If phrase = "." Then
                        Dim sentence = sentenceBuilder.ToString()
                        sentenceBuilder.Clear()
                        output.Add(sentence)
                        isFirstPhrase = True
                    End If
                Next phrase
            Finally
                output.CompleteAdding()
            End Try
        End Sub

        Private Shared Sub WriteSentences(ByVal input As BlockingCollection(Of String))
            Using outfile As New StreamWriter(PathForPipelineResults)
                Dim sentenceCount = 1
                For Each sentence In input.GetConsumingEnumerable()
                    Dim printSentence = sentence
                    Stage4AdditionalWork()
                    If printSentence = TargetSentence Then
                        printSentence = printSentence & "       " & SuccessString
                    End If
                    outfile.WriteLine(sentenceCount.ToString() & " " & printSentence)
                    OutputProgress(sentenceCount)
                    sentenceCount += 1
                Next sentence
            End Using
        End Sub

        Private Shared Sub CheckResults()
            Dim file1ChecksumTask = Task.Factory.StartNew(Of String)(Function() GetFileChecksum(PathForSequentialResults))
            Dim file2Checksum = GetFileChecksum(PathForPipelineResults)
            Dim file1Checksum = file1ChecksumTask.Result

            Console.WriteLine(String.Format("Results written to files ""{0}"" and ""{1}""", PathForSequentialResults, PathForPipelineResults))
            If file1Checksum Is Nothing OrElse file2Checksum Is Nothing Then
                Console.WriteLine("PROGRAM ERROR! Couldn't calculate file checksum.")
                Return
            End If
            If file1Checksum.Equals(file2Checksum) Then
                Console.WriteLine("Sequential and pipeline results were verified to be equal.")
            Else
                Console.WriteLine("PROGRAM ERROR! Sequential and pipeline results don't match.")
            End If
        End Sub

        Private Shared Function GetFileChecksum(ByVal fileName As String) As String
            Dim result() As Byte
            Using _fileStream As New FileStream(fileName, FileMode.Open)
                Dim _md5 As MD5 = New MD5CryptoServiceProvider()
                result = _md5.ComputeHash(_fileStream)
            End Using
            Return Convert.ToBase64String(result, 0, result.Length)
        End Function

        Private Shared Sub OutputProgress(ByVal sentenceCount As Integer)
            If sentenceCount Mod Math.Max(1, (NumberOfSentences \ 10)) = 0 Then
                Console.Write(".")
            End If
        End Sub
    End Class
End Namespace
