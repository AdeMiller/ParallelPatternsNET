'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks

Namespace Microsoft.Practices.ParallelGuideSamples.BasicParallelLoops
    Public Class CustomIteratorExample
        Public Sub Example()
            Dim myTree As New Tree(Of Integer)(5) With {.Left = New Tree(Of Integer)(2) With {.Left = New Tree(Of Integer)(1), .Right = New Tree(Of Integer)(3)}, .Right = New Tree(Of Integer)(7)}

            Console.WriteLine("Traverse tree with custom iterator:")
            Parallel.ForEach(myTree.Iterator(), Function(node) As Object
                                                    Console.WriteLine("  Node.Data = {0}", node.Data)
                                                    Return Nothing
                                                End Function)
            Console.WriteLine()
        End Sub
    End Class

    Public Class Tree(Of T)
        Public Left As Tree(Of T), Right As Tree(Of T)
        Public Data As T

        Public Sub New(ByVal data As T)
            Me.Data = data
        End Sub

        Public Function Iterator() As IEnumerable(Of Tree(Of T))
            Dim returnValue As New List(Of Tree(Of T))
            Dim queue = New Queue(Of Tree(Of T))()
            queue.Enqueue(Me)
            Do While queue.Count > 0
                Dim node = queue.Dequeue()
                returnValue.Add(node)
                If node.Left IsNot Nothing Then
                    queue.Enqueue(node.Left)
                End If
                If node.Right IsNot Nothing Then
                    queue.Enqueue(node.Right)
                End If
            Loop
            Return returnValue
        End Function
    End Class
End Namespace
