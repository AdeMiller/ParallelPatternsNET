'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================


Imports SubscriberID = System.Int32

Namespace Microsoft.Practices.ParallelGuideSamples.SocialNetwork
    ''' <summary>
    ''' A subscriber's data in the social network
    ''' For our present purposes, just a collection of friends' IDs 
    ''' </summary>
    Public Class Subscriber
        Private ReadOnly _friends As HashSet(Of SubscriberID)

        Public Sub New()
            _friends = New HashSet(Of SubscriberID)()
        End Sub

        ''' <summary>
        ''' Return a reference for read-only operations
        ''' </summary>
        Public ReadOnly Property Friends() As HashSet(Of SubscriberID)
            Get
                Return _friends
            End Get
        End Property

        ''' <summary>
        ''' Return a copy
        ''' </summary>
        Public Function FriendsCopy() As HashSet(Of SubscriberID)
            Return New HashSet(Of SubscriberID)(_friends)
        End Function

        Public Sub Print(ByVal maxFriends As Integer)
            Console.Write("{0,5:D}", Friends.Count)
            Dim subs = Friends.GetEnumerator()
            For i As Integer = 0 To Math.Min(maxFriends, Friends.Count) - 1
                subs.MoveNext()
                Console.Write("{0,8:D}", subs.Current)
            Next i
            Console.WriteLine()
        End Sub
    End Class
End Namespace
