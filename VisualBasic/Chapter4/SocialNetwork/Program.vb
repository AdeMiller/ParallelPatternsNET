'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.Utilities
Imports System.Globalization
Imports System.Collections.ObjectModel

Imports SubscriberID = System.Int32
Imports IDMultisetItemList = System.Collections.ObjectModel.Collection(Of System.Collections.Generic.KeyValuePair(Of System.Int32, Integer)) 'to sort, must use list of Multiset items not Multiset itself

Namespace Microsoft.Practices.ParallelGuideSamples.SocialNetwork

    Friend Class Program
        ''' <summary>
        ''' Usage: SocialNework subscriberCount maxFriends
        ''' Both arguments are optional, defaults are 1000 and 10.
        ''' </summary>
        Shared Sub Main(ByVal args() As String)
            Console.WriteLine("Social Network Sample" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If

            Dim _random As New Random(1) ' seed for random but reproducible runs

            ' Defaults for data generation, may override some on command line
            Dim subscriberCount As Integer = 10000
            Dim maxFriends As Integer = 2000

            ' Defaults for printed table of results
            Const maxRows As Integer = 16
            Const maxCols As Integer = 8
            Const maxCandidates As Integer = maxRows

            ' Optionally override some defaults on command line
            If args.Length > 0 Then
                subscriberCount = Int32.Parse(args(0), CultureInfo.CurrentCulture)
            End If
            If args.Length > 1 Then
                maxFriends = Int32.Parse(args(1), CultureInfo.CurrentCulture)
            End If

            Console.WriteLine("Creating data...")
            ' Allocate subscribers, assign friends for timing tests
            Dim subscribers As New SubscriberRepository(subscriberCount)
            subscribers.AssignRandomFriends(maxFriends, _random)

            ' Print a few subscribers and a summary
            Console.WriteLine()
            Console.WriteLine("Some subscribers and some of their friends")
            subscribers.Print(maxRows, maxCols)
            Console.WriteLine()
            Console.WriteLine("{0} subscribers in all, with up to {1} friends or even more  ", subscriberCount, maxFriends)

            ' Choose a subscriber seeking friends
            Const id As SubscriberID = 0
            Dim _subscriber = subscribers.GetSubscriber(id)
            Console.WriteLine()
            Console.WriteLine("Find potential friends for this subscriber, with these friends:")
            Console.Write("{0,10:D}", id)
            _subscriber.Print(_subscriber.Friends.Count)
            Console.WriteLine()

            ' Sequential for
            Dim candidates = New IDMultisetItemList() ' to sort, must use list of Multiset items not Multiset itself
            SampleUtilities.TimedRun(
                Function()
                    candidates = subscribers.PotentialFriendsSequential(id, maxCandidates)
                    Return candidates.Count
                End Function, "  Sequential for")
            Console.WriteLine()

            Dim rows As Integer = Math.Min(maxRows, candidates.Count)
            Console.WriteLine("{0} potential friends for this subscriber, and the number of mutual friends", rows)
            Multiset.Print(candidates)
            Console.WriteLine()

            ' Parallel.ForEach 
            SampleUtilities.TimedRun(
                Function()
                    candidates = subscribers.PotentialFriendsParallel(id, maxCandidates)
                    Return candidates.Count
                End Function, "Parallel.ForEach")
            Console.WriteLine()

            rows = Math.Min(maxRows, candidates.Count)
            Console.WriteLine("{0} potential friends for this subscriber, and the number of mutual friends", rows)
            Multiset.Print(candidates)
            Console.WriteLine()

            ' Sequential LINQ
            SampleUtilities.TimedRun(
                Function()
                    candidates = subscribers.PotentialFriendsLinq(id, maxCandidates)
                    Return candidates.Count
                End Function, " Sequential LINQ")
            Console.WriteLine()

            rows = Math.Min(maxRows, candidates.Count)
            Console.WriteLine("{0} potential friends for this subscriber, and the number of mutual friends", rows)
            Multiset.Print(candidates)
            Console.WriteLine()

            ' PLINQ
            SampleUtilities.TimedRun(
                Function()
                    candidates = subscribers.PotentialFriendsPLinq(id, maxCandidates)
                    Return candidates.Count
                End Function, "           PLINQ")
            Console.WriteLine()

            rows = Math.Min(maxRows, candidates.Count)
            Console.WriteLine("{0} potential friends for this subscriber, and the number of mutual friends", rows)
            Multiset.Print(candidates)

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub
    End Class
End Namespace
