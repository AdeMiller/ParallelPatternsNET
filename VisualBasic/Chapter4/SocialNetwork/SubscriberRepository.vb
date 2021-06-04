'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Threading.Tasks
Imports System.Collections.ObjectModel

Imports SubscriberID = System.Int32
Imports IDMultiset = System.Collections.Generic.Dictionary(Of System.Int32, Integer)
Imports IDMultisetItem = System.Collections.Generic.KeyValuePair(Of System.Int32, Integer)
Imports IDMultisetItemList = System.Collections.ObjectModel.Collection(Of System.Collections.Generic.KeyValuePair(Of System.Int32, Integer)) 'to sort, must use list of Multiset items not Multiset itself


Namespace Microsoft.Practices.ParallelGuideSamples.SocialNetwork

    Public Class SubscriberRepository
        ''' <summary>
        ''' Collection of all subscribers in the repository
        ''' </summary>
        Private ReadOnly subscribers As Dictionary(Of SubscriberID, Subscriber)

        ''' <summary>
        ''' Constructor, initialize with no subscribers
        ''' </summary>
        Public Sub New()
            subscribers = New Dictionary(Of SubscriberID, Subscriber)()
        End Sub

        ''' <summary>
        ''' Constructor, allocate subscribers with no friends
        ''' </summary>
        Public Sub New(ByVal subscriberCount As Integer)
            subscribers = New Dictionary(Of SubscriberID, Subscriber)()
            For subscriber As SubscriberID = 0 To subscriberCount - 1
                subscribers(subscriber) = New Subscriber()
            Next subscriber
        End Sub

        ''' <summary>
        ''' Return number of subscribers
        ''' </summary>
        Public ReadOnly Property Count() As Integer
            Get
                Return subscribers.Count
            End Get
        End Property

        ''' <summary>
        ''' Return subscriber with given ID
        ''' </summary>
        ''' <param name="id">ID of subscriber</param>
        ''' <returns>Subcriber data for that ID</returns>
        Public Function GetSubscriber(ByVal id As SubscriberID) As Subscriber
            Return subscribers(id)
        End Function

        ''' <summary>
        ''' Assign every subscriber a random number (up to maxFriends) of randomly chosen new friends
        ''' Ensure friends relation is symmetric 
        ''' </summary> 
        Public Sub AssignRandomFriends(ByVal maxFriends As Integer, ByVal random As Random)
            For Each record As KeyValuePair(Of System.Int32, Subscriber) In subscribers
                Dim id As Integer = record.Key
                Dim _subscriber As Subscriber = record.Value
                If random Is Nothing Then
                    Throw New ArgumentNullException("random")
                End If
                Dim nfriends As Integer = random.Next(maxFriends)
                Dim friends = _subscriber.Friends
                For i As Integer = 0 To nfriends - 1
                    Dim [friend] As SubscriberID = random.Next(subscribers.Count)

                    If [friend] <> id Then ' self is never in friends
                        friends.Add([friend]) ' HashSet ensures no duplicates
                        Dim friendsFriends = subscribers([friend]).Friends
                        friendsFriends.Add(id) ' symmetric relation
                    End If
                Next i
            Next record
        End Sub

        ''' <summary>
        ''' Find potential friends (candidates) for subscriber: other subscribers with mutual friends
        ''' Demonstrate MapReduce with sequential foreach
        ''' </summary>
        ''' <param name="id">ID of subscriber seeking friends</param>
        ''' <param name="maxCandidates">Maximum number of potential friends to return</param>
        ''' <returns>Sorted list of candidates as ID/count pairs, count is number of mutual friends</returns>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")>
        Public Function PotentialFriendsSequential(ByVal id As SubscriberID, ByVal maxCandidates As Integer) As IDMultisetItemList
            ' Map
            Dim foafsList = New List(Of IDMultiset)()
            For Each [friend] As SubscriberID In subscribers(id).Friends
                Dim foafs = subscribers([friend]).FriendsCopy()
                foafs.RemoveWhere(Function(foaf) foaf = id OrElse subscribers(id).Friends.Contains(foaf)) ' remove self, own friends
                foafsList.Add(Multiset.Create(foafs))
            Next [friend]

            ' Reduce
            Dim candidates As New IDMultiset()
            For Each foafs As IDMultiset In foafsList
                candidates = Multiset.Union(foafs, candidates)
            Next foafs

            ' Postprocess
            Return Multiset.MostNumerous(candidates, maxCandidates)
        End Function

        ''' <summary>
        ''' Find potential friends, demonstrate MapReduce with Parallel.ForEach, same parameters as above.
        ''' </summary>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")>
        Public Function PotentialFriendsParallel(ByVal id As SubscriberID, ByVal maxCandidates As Integer) As IDMultisetItemList
            Dim locker As New Object()
            Dim candidates As New IDMultiset()

            'Map over friends
            'init thread-local state localFoafs with empty Multiset
            ' remove self, own friends
            'Reduce, thread-local
            'Reduce, among threads 
            Parallel.ForEach(subscribers(id).Friends,
                             Function() Multiset.Create(New HashSet(Of SubscriberID)()),
                    Function([friend], loopState, localFoafs)
                        Dim foafs = subscribers([friend]).FriendsCopy()
                        foafs.RemoveWhere(Function(foaf) foaf = id OrElse subscribers(id).Friends.Contains(foaf))
                        Return Multiset.Union(localFoafs, Multiset.Create(foafs))
                    End Function,
                    Function(localFoafs)
                        SyncLock locker
                            candidates = Multiset.Union(localFoafs, candidates)
                        End SyncLock
                    End Function)

            Return Multiset.MostNumerous(candidates, maxCandidates) ' postprocess results of Reduce
        End Function

        ''' <summary>
        ''' Find potential friends, demonstrate MapReduce with sequential LINQ, same parameters as above.
        ''' </summary>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")>
        Public Function PotentialFriendsLinq(ByVal id As SubscriberID, ByVal maxCandidates As Integer) As IDMultisetItemList

            'Map
            'remove self, own friends
            'Reduce
            Dim candidates =
                subscribers(id).Friends.SelectMany(
                    Function([friend]) subscribers([friend]).Friends).Where(
                    Function(foaf) foaf <> id AndAlso Not (subscribers(id).Friends.Contains(foaf))).GroupBy(Function(foaf) foaf).Select(Function(foafGroup) New IDMultisetItem(foafGroup.Key, foafGroup.Count())) ' Reduce -  remove self, own friends -  Map

            Return Multiset.MostNumerous(candidates, maxCandidates) ' postprocess results of Reduce
        End Function

        ''' <summary>
        ''' Find potential friends, demonstrate MapReduce with PLINQ, same parameters as above.
        ''' </summary>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")>
        Public Function PotentialFriendsPLinq(ByVal id As SubscriberID, ByVal maxCandidates As Integer) As IDMultisetItemList
            'Map
            'remove self, own friends
            'Reduce
            Dim candidates = subscribers(id).Friends.AsParallel().SelectMany(Function([friend]) subscribers([friend]).Friends).Where(Function(foaf) foaf <> id AndAlso Not (subscribers(id).Friends.Contains(foaf))).GroupBy(Function(foaf) foaf).Select(Function(foafGroup) New IDMultisetItem(foafGroup.Key, foafGroup.Count())) ' Reduce -  remove self, own friends -  Map
            Return Multiset.MostNumerous(candidates, maxCandidates) ' postprocess results of Reduce
        End Function

        ''' <summary>
        ''' Print rows subscribers from repository, up to maxFriends each
        ''' </summary>
        Public Sub Print(ByVal rows As Integer, ByVal maxFriends As Integer)
            Dim subs = subscribers.GetEnumerator()
            Console.WriteLine("Subscriber    N  Friends")
            For i As Integer = 0 To rows - 1
                If subs.MoveNext() Then
                    Dim id As Integer = subs.Current.Key
                    Dim s As Subscriber = subs.Current.Value
                    Console.Write("{0,10:D}", id)
                    s.Print(maxFriends)
                End If
            Next i
        End Sub
    End Class
End Namespace