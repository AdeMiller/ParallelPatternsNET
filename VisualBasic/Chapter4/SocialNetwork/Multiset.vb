'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.ObjectModel
Imports SubscriberID = System.Int32

Imports IDMultiset = System.Collections.Generic.Dictionary(Of System.Int32, Integer)
Imports IDMultisetItem = System.Collections.Generic.KeyValuePair(Of System.Int32, Integer)
Imports IDMultisetItemList = System.Collections.ObjectModel.Collection(Of System.Collections.Generic.KeyValuePair(Of System.Int32, Integer)) 'to sort, must use list of Multiset items not Multiset itself


Namespace Microsoft.Practices.ParallelGuideSamples.SocialNetwork


    ''' <summary>
    ''' Multiset of IDs, represented as dictionary where Key is ID and Value is its multiplicity.
    ''' </summary>
    Friend NotInheritable Class Multiset

        ''' <summary>
        ''' Initialize Multiset from HashSet, each ID has multiplicity 1
        ''' </summary>
        ''' <param name="friends"></param>
        ''' <returns></returns>
        Private Sub New()
        End Sub
        Public Shared Function Create(ByVal friends As HashSet(Of SubscriberID)) As IDMultiset
            Dim multiset = New IDMultiset()
            For Each [friend] As SubscriberID In friends
                multiset([friend]) = 1
            Next [friend]
            Return multiset
        End Function

        ''' <summary>
        ''' Multiset union.
        ''' </summary>
        ''' <param name="multiset1">First Multiset</param>
        ''' <param name="multiset2">Second Multiset</param>
        ''' <returns>Union of Multiset1 and Multiset2</returns>
        Public Shared Function Union(ByVal multiset1 As IDMultiset, ByVal multiset2 As IDMultiset) As IDMultiset
            If multiset1.Count < multiset2.Count Then
                Return Union(multiset2, multiset1)
            Else
                Dim _union = New IDMultiset()
                For Each item In multiset1
                    _union.Add(item.Key, item.Value)
                Next item
                For Each item In multiset2
                    Dim [friend] = item.Key
                    Dim count = item.Value
                    _union([friend]) = If(_union.ContainsKey([friend]), _union([friend]) + count, count)
                Next item
                Return _union
            End If
        End Function

        ''' <summary>
        ''' Return sorted list of most numerous items in Multiset.
        ''' </summary>
        ''' <param name="multiset">Multiset of IDs</param>
        ''' <param name="maxItems">Maximum number of Multiset items to return</param>
        ''' <returns>List of ID/multiplicity pairs, sorted in order of decreasing multiplicity</returns>
        Public Shared Function MostNumerous(ByVal multiset As IEnumerable(Of IDMultisetItem), ByVal maxItems As Integer) As IDMultisetItemList
            ' mostNumerous is sorted by item value, mostNumerous[0] is smallest 
            Dim _mostNumerous = New IDMultisetItemList()
            For Each item As IDMultisetItem In multiset
                If _mostNumerous.Count < maxItems Then
                    InsertInOrder(item, _mostNumerous)
                Else
                    Dim lastIndex As Integer = _mostNumerous.Count - 1
                    If item.Value > _mostNumerous(lastIndex).Value Then ' smallest value is at the end
                        _mostNumerous.RemoveAt(lastIndex)
                        InsertInOrder(item, _mostNumerous)
                    End If
                End If
            Next item
            Return _mostNumerous
        End Function

        ''' <summary>
        ''' Insert ID/multiplicity pair in sorted list
        ''' </summary>
        ''' <param name="item">ID/multiplicity pair to insert in list</param>
        ''' <param name="list">list of ID/multiplicity pairs, sort by multiplicity, largest to smallest</param>
        Private Shared Sub InsertInOrder(ByVal item As IDMultisetItem, ByVal list As IDMultisetItemList)
            If list.Count = 0 Then
                list.Insert(0, item)
            Else
                Dim i As Integer
                For i = 0 To list.Count - 1
                    If item.Value >= list(i).Value Then
                        Exit For
                    End If
                Next i
                list.Insert(i, item) ' inserts ahead of list[i], results in descending order
            End If
        End Sub

        ''' <summary>
        ''' Print list of ID/multiplicity pairs.
        ''' </summary>
        ''' <param name="list">List of ID/multiplicity pairs</param>
        Public Shared Sub Print(ByVal list As IDMultisetItemList)
            For i As Integer = 0 To list.Count - 1
                Console.WriteLine("{0,10:D}{1,5:D}", list(i).Key, list(i).Value)
            Next i
        End Sub
    End Class
End Namespace
