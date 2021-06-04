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

Namespace Microsoft.Practices.ParallelGuideSamples.ProfilerExamples
	Friend Class Program
		Shared Sub Main(ByVal args() As String)
			Console.WriteLine("Profiler Samples" & vbLf)

			Console.WriteLine("Press any key to start run after the profiler has initiliazed...")
			Console.ReadKey()

			Console.WriteLine("Starting..." & vbLf)

			If args.Length <> 1 Then
				Help()
				Return
			End If

			Select Case args(0).Trim().ToLower()
				Case "deadlock"
					Console.WriteLine("Showing deadlock.")
					Console.WriteLine("WARNING: This program does not terminate!")
					Deadlock()
				Case "lockcontention"
					Console.WriteLine("Showing lock contention.")
					LockContention()
				Case "oversubscription"
					Console.WriteLine("Showing oversubscription.")
					Oversubscription()
				Case "loadimbalance"
					Console.WriteLine("Showing load imbalance.")
					LoadImbalance()
				Case Else
					Help()
			End Select

			Console.WriteLine(vbLf & "Run complete...")
		End Sub

		Private Shared Sub Help()
			Console.WriteLine("Usage: [deadlock|lockcontention|oversubscription|loadimbalance]")
		End Sub

		Private Shared Sub Deadlock()
			Dim obj1 As New Object()
			Dim obj2 As New Object()

            Parallel.Invoke(Sub()
                                Dim i As Integer = 0
                                Do
                                    SyncLock obj1
                                        Console.WriteLine("Got 1 at {0}", i)
                                        SyncLock obj2
                                            Console.WriteLine("Got 2 at {0}", i)
                                        End SyncLock
                                    End SyncLock
                                    i += 1
                                Loop
                            End Sub, Sub()
                                         Dim i As Integer = 0
                                         Do
                                             SyncLock obj2
                                                 Console.WriteLine("Got 2 at {0}", i)
                                                 SyncLock obj1
                                                     Console.WriteLine("Got 1 at {0}", i)
                                                 End SyncLock
                                             End SyncLock
                                             i += 1
                                         Loop
                                     End Sub
              )
        End Sub

		

		Private Shared Sub LockContention()
			Dim syncObj As New Object()

			For p As Integer = 0 To Environment.ProcessorCount - 1
						' Do work
						' Do protected work
                CType(New Thread(Sub()
                                     For i As Integer = 0 To 49
                                         For j As Integer = 0 To 999

                                         Next j
                                         SyncLock syncObj
                                             For j As Integer = 0 To 99999999

                                             Next j
                                         End SyncLock
                                     Next i
                                 End Sub), Thread).Start()
            Next p
		End Sub
		
	

		Private Shared Sub Oversubscription()
			For i As Integer = 0 To (Environment.ProcessorCount * 4) - 1
					' Do work 
                CType(New Thread(Sub()
                                     For j As Integer = 0 To 999999999

                                     Next j
                                 End Sub), Thread).Start()
			Next i
		End Sub
		
		

		Private Shared Sub LoadImbalance()
			Const loadFactor As Integer = 10
				' Do work
            ParallelEnumerable.Range(0, 100000).ForAll(Sub(i)
                                                           For j As Integer = 0 To (i * loadFactor) - 1

                                                           Next j
                                                       End Sub)
		End Sub
		

	End Class
End Namespace
