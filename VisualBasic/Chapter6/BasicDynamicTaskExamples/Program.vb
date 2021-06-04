'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.Concurrent
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.BasicDynamicTasks
    Friend Class Program
        Public Class Tree(Of T)

            Public Property Data() As T
            Public Property Left() As Tree(Of T)
            Public Property Right() As Tree(Of T)
        End Class

        Shared Sub Main()
            Task.Factory.StartNew(Sub() MainTask()).Wait()
        End Sub

        Private Const N As Integer = 1 ' number of timing runs per test
        Private Const Time As Double = 0.01 ' CPU time in seconds to visit each node of the tree
        Private Const TreeSize As Integer = 2000 ' number of nodes in the tree
        Private Const TreeDensity As Double = 0.75 ' P(left child node exists), P(right child node exists) for interior nodes

        Private Shared Sub MainTask()
            Console.WriteLine("Basic Dynamic Task Samples" & vbLf)
#If (DEBUG) Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If

            Console.WriteLine("Tree Walking")
            Dim tree = MakeTree(TreeSize, TreeDensity)

            SampleUtilities.TimedAction(Sub() Chapter6Example1Sequential(tree), "tree traversal, sequential")

            SampleUtilities.TimedAction(Sub() Chapter6Example1Parallel(tree), "tree traversal, parallel")

            SampleUtilities.TimedAction(Sub() Chapter6Example1Parallel2(tree), "tree traversal, parallel - attached to parent")

            SampleUtilities.TimedAction(Sub() Chapter6Example01ParallelWhileNotEmpty(tree), "parallel while not empty - Parallel.ForEach")

            SampleUtilities.TimedAction(Sub() Chapter6Example01ParallelWhileNotEmpty2(tree), "parallel while not empty - parallel tasks")

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadKey()
        End Sub

        Private Shared Sub SequentialWalk(Of T)(ByVal tree As Tree(Of T), ByVal action As Action(Of T))
            If tree Is Nothing Then
                Return
            End If
            action(tree.Data)
            SequentialWalk(tree.Left, action)
            SequentialWalk(tree.Right, action)
        End Sub

        Private Shared Sub ParallelWalk(Of T)(ByVal tree As Tree(Of T), ByVal action As Action(Of T))
            If tree Is Nothing Then
                Return
            End If
            Dim t1 = Task.Factory.StartNew(Sub() action(tree.Data))
            Dim t2 = Task.Factory.StartNew(Sub() ParallelWalk(tree.Left, action))
            Dim t3 = Task.Factory.StartNew(Sub() ParallelWalk(tree.Right, action))
            Task.WaitAll(t1, t2, t3)
        End Sub

        Private Shared Sub ParallelWalk2(Of T)(ByVal tree As Tree(Of T), ByVal action As Action(Of T))
            If tree Is Nothing Then
                Return
            End If
            Dim t1 = Task.Factory.StartNew(Sub() action(tree.Data), TaskCreationOptions.AttachedToParent)
            Dim t2 = Task.Factory.StartNew(Sub() ParallelWalk2(tree.Left, action), TaskCreationOptions.AttachedToParent)
            Dim t3 = Task.Factory.StartNew(Sub() ParallelWalk2(tree.Right, action), TaskCreationOptions.AttachedToParent)
            Task.WaitAll(t1, t2, t3)
        End Sub

        Public Shared Function MakeTree(ByVal nodeCount As Integer, ByVal density As Double) As Tree(Of String)
            If nodeCount < 1 Then
                Throw New ArgumentOutOfRangeException("nodeCount")
            End If
            If Not (0 < density AndAlso density <= 1.0) Then
                Throw New ArgumentOutOfRangeException("density")
            End If
            Return MakeTree(nodeCount, density, 0, New Random())
        End Function

        Private Shared Function MakeTree(ByVal nodeCount As Integer, ByVal density As Double, ByVal offset As Integer, ByVal r As Random) As Tree(Of String)
            Dim flip1 = r.NextDouble() > density
            Dim flip2 = r.NextDouble() > density
            Dim newCount = nodeCount - 1
            Dim count1 As Integer = CType(If(flip1 AndAlso flip2, newCount / 2, If(flip1, newCount, 0)), Integer)
            Dim count2 As Integer = newCount - count1
            If r.NextDouble() > 0.5 Then
                Dim tmp = count1
                count1 = count2
                count2 = tmp
            End If

            Return New Tree(Of String)() With {.Data = offset.ToString(), .Left = If(count1 > 0, MakeTree(count1, density, offset + 1, r), Nothing), .Right = If(count2 > 0, MakeTree(count2, density, offset + 1 + count1, r), Nothing)}
        End Function

        Private Shared Sub Chapter6Example1Sequential(ByVal tree As Tree(Of String))
            For i As Integer = 0 To N - 1
                Dim result As New List(Of String)()
                SequentialWalk(tree, Sub(data)
                                         SampleUtilities.DoCpuIntensiveOperation(Time)
                                         result.Add(data)
                                     End Sub)
            Next i
            Console.WriteLine()
        End Sub

        Private Shared Sub Chapter6Example1Parallel(ByVal tree As Tree(Of String))
            For i As Integer = 0 To N - 1
                Dim result As New ConcurrentBag(Of String)()
                ParallelWalk(tree, Sub(data)
                                       SampleUtilities.DoCpuIntensiveOperation(Time)
                                       result.Add(data)
                                   End Sub)
            Next i
        End Sub

        Private Shared Sub Chapter6Example1Parallel2(ByVal tree As Tree(Of String))
            For i As Integer = 0 To N - 1
                Dim result As New ConcurrentBag(Of String)()
                ParallelWalk2(tree, Sub(data)
                                        SampleUtilities.DoCpuIntensiveOperation(Time)
                                        result.Add(data)
                                    End Sub)
            Next i
        End Sub

        Private Shared Sub ParallelWhileNotEmpty(Of T)(ByVal initialValues As IEnumerable(Of T), ByVal body As Action(Of T, Action(Of T)))
            Dim From = New ConcurrentQueue(Of T)(initialValues)

            Do While Not From.IsEmpty
                Dim [to] = New ConcurrentQueue(Of T)()
                Dim addMethod As Action(Of T) = AddressOf [to].Enqueue
                Parallel.ForEach(From, Sub(v) body(v, addMethod))
                From = [to]
            Loop
        End Sub

        Private Shared Sub ParallelWalk4(Of T)(ByVal tree As Tree(Of T), ByVal action As Action(Of T))
            If tree Is Nothing Then
                Return
            End If
            ParallelWhileNotEmpty(New Tree(Of T)() {tree}, Sub(item, adder)
                                                               If item.Left IsNot Nothing Then
                                                                   adder(item.Left)
                                                               End If
                                                               If item.Right IsNot Nothing Then
                                                                   adder(item.Right)
                                                               End If
                                                               action(item.Data)
                                                           End Sub)
        End Sub

        Private Shared Sub Chapter6Example01ParallelWhileNotEmpty(ByVal tree As Tree(Of String))
            For i As Integer = 0 To N - 1
                Dim result As New ConcurrentBag(Of String)()
                ParallelWalk4(tree, Sub(data)
                                        SampleUtilities.DoCpuIntensiveOperation(Time)
                                        result.Add(data)
                                    End Sub)
            Next i
        End Sub

        Private Shared Sub ParallelWhileNotEmpty2(Of T)(ByVal initialValues As IEnumerable(Of T), ByVal body As Action(Of T, Action(Of T)))
            Dim items = New ConcurrentBag(Of T)(initialValues)
            Dim taskList = New List(Of Task)()
            Dim maxTasks = Environment.ProcessorCount * 10
            Dim taskCount = 0
            Dim addMethod As Action(Of T) = Sub(v) items.Add(v)
            Do
                Dim tasks = taskList.ToArray()
                If tasks.Length > 0 Then
                    Task.WaitAll(tasks)
                    taskList.Clear()
                    taskCount = 0
                End If
                If items.IsEmpty Then
                    Exit Do
                Else
                    Dim item As T
                    Do While taskCount < maxTasks AndAlso items.TryTake(item)
                        Dim v = item
                        Dim _task = task.Factory.StartNew(Sub() body(v, addMethod))
                        taskList.Add(_task)
                        taskCount += 1
                    Loop
                End If
            Loop
        End Sub

        Private Shared Sub Walk5(Of T)(ByVal tree As Tree(Of T), ByVal action As Action(Of T))
            If tree Is Nothing Then
                Return
            End If
            ParallelWhileNotEmpty2(New Tree(Of T)() {tree}, Sub(item, adder)
                                                                If item.Left IsNot Nothing Then
                                                                    adder(item.Left)
                                                                End If
                                                                If item.Right IsNot Nothing Then
                                                                    adder(item.Right)
                                                                End If
                                                                action(item.Data)
                                                            End Sub)
        End Sub


        Private Shared Sub Chapter6Example01ParallelWhileNotEmpty2(ByVal tree As Tree(Of String))
            For i As Integer = 0 To N - 1
                Dim result As New ConcurrentBag(Of String)()
                Walk5(tree, Sub(data)
                                SampleUtilities.DoCpuIntensiveOperation(Time)
                                result.Add(data)
                            End Sub)
            Next i
        End Sub


    End Class
End Namespace
