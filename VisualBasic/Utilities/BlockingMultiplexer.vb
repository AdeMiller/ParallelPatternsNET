'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.Concurrent
Imports System.Threading
Imports System.ComponentModel

Namespace Microsoft.Practices.ParallelGuideSamples.Utilities
    ''' <summary>
    ''' Multiplexer that serializes inputs from multiple producers into a single consumer enumeration in a 
    ''' user-specified order. 
    ''' </summary>
    ''' <typeparam name="T">The type of input element</typeparam>
    ''' <remarks>The use case for this class is a producer/consumer scenario with multiple producers and 
    ''' a single consumer. The producers each have their private blocking collections for enqueuing the elements
    ''' that they produce. The consumer of the producer queues is the multiplexer, which is responsible 
    ''' combining the inputs from all of the producers according to user-provided "lock order." The multiplexer 
    ''' provides an enumeration that a consumer can use to observe the multiplexed values in the chosen order. 
    ''' 
    ''' The multiplexer does not perform sorting. Instead, it relies on the fact the the producer queues are
    ''' locally ordered and looks for the next value by simultaneously monitoring the heads of 
    ''' all of the producer queues.
    ''' 
    ''' The order of elements in the producer queues is given by a user-provided lockOrderFn delegate. This is called
    ''' lock order and is represented by an integer. The initial lock id is specified in the multiplexer's constructor. 
    ''' Producer queues must be consistent. This means that they are locally ordered with respect to lock ids. When
    ''' multiplexed together into a single enumeration, the producer queues must produce a sequence of values whose 
    ''' lock ids are consecutive. (The lock ids in the individual producer queues must be in order but not necessarily 
    ''' consecutive.)
    ''' 
    ''' It is not required that all elements in the producer queues have a lock order. The position of such elements (denoted
    ''' by a lock id that is less than zero) is constrained by preceding and succeeding elements in the producer's queue
    ''' that do include a lock order. This results in a partial order. The unit tests for this class for an example of 
    ''' partial ordering constraints.
    ''' 
    ''' See Campbell et al, "Multiplexing of Partially Ordered Events," in TestCom 2005, Springer Verlag, June 2005,  
    ''' available online at http://research.microsoft.com/apps/pubs/default.aspx?id=77808. 
    ''' </remarks>
    Public Class BlockingMultiplexer(Of T)
#Region "Local Types"

        Private Enum Mode
            Starting
            Running
            Finished
            CleanupStarted
        End Enum

        ' Internal state for each producer
        Private Structure ProducerInfo
            Public Index As Integer ' producer id 0, 1, 2,...
            Public Collection As BlockingCollection(Of T) ' producer's queue
            Public IsCompleted As Boolean ' true if producer's IsCompleted property was observed to be true
            Public HasPendingValue As Boolean ' does lookahead value exist?
            Public PendingValue As T ' if yes, lookahead value
            Public PendingLockId As Integer ' if yes, lookahead lock id
            Public LastLockId As Integer ' last lock id read (for error checking only)
        End Structure

#End Region

#Region "Fields"

        Private ReadOnly boundedCapacity As Integer = -1
        Private ReadOnly producersLock As New Object()
        Private ReadOnly lockOrderFn As Func(Of T, Integer)

        Private producers() As ProducerInfo = {}
        Private _mode As Mode = Mode.Starting
        Private nextLockId As Integer = 0

#End Region

#Region "Constructors"

        ''' <summary>
        ''' Creates a multiplexer that serializes inputs from multiple producer queues.
        ''' </summary>
        ''' <param name="lockOrderFn">Delegate that returns an integer sequence number for elements of the 
        ''' producer queues. It returns a negative number if order is not important for a given element.</param>
        ''' <param name="initialLockId">The first lock id of the sequence</param>
        Public Sub New(ByVal lockOrderFn As Func(Of T, Integer), ByVal initialLockId As Integer)
            Me.New(lockOrderFn, initialLockId, -1)
        End Sub

        ''' <summary>
        ''' Creates a multiplexer that serializes inputs from multiple producer queues.
        ''' </summary>
        ''' <param name="lockOrderFn">Delegate that returns an integer sequence number for elements of the 
        ''' producer queues. It returns a negative number if order is not important for a given element.</param>
        ''' <param name="initialLockId">The first lock id of the sequence</param>
        ''' <param name="boundedCapacity">The maximum number of elements that a producer queue
        ''' may contain before it blocks the producer.</param>
        Public Sub New(ByVal lockOrderFn As Func(Of T, Integer), ByVal initialLockId As Integer, ByVal boundedCapacity As Integer)
            If lockOrderFn Is Nothing Then
                Throw New ArgumentNullException("lockOrderFn")
            End If
            If initialLockId < 0 Then
                Throw New ArgumentOutOfRangeException("initialLockId")
            End If
            If boundedCapacity < -1 Then
                Throw New ArgumentOutOfRangeException("boundedCapacity")
            End If
            Me.boundedCapacity = boundedCapacity
            nextLockId = initialLockId
            Me.lockOrderFn = lockOrderFn
        End Sub

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Creates a new input source to the multiplexer.
        ''' </summary>
        ''' <returns>A blocking collection that will be used as one of the multiplexer's inputs.
        ''' </returns>
        ''' <remarks>This blocking collection for the use of the producer only. Its only consumer of the 
        ''' is the multiplexer instance that created it.
        ''' 
        ''' The producer should invoke Add to insert elements as needed. After the last element, the producer 
        ''' invokes CompleteAdding.
        ''' 
        ''' If the boundedCapacity was specified in the multiplexer's constructor, this value will be used as the
        ''' boundedCapacity of the blocking collections used by the producers. This will cause the producer to block
        ''' when trying to add elements to the blocking collection above this limit.
        ''' 
        ''' There is a partial order constraint on the values added by the producer to this blocking collection. The 
        ''' lockOrderFn that was provided to the constructor of the multiplexer will be applied to each element in 
        ''' the queue by the multiplexer. If the lockOrderFn returns a non-negative value for the enqueued 
        ''' object, this value must be strictly greater than the lock order of all previous objects that were added 
        ''' to this blocking collection.
        ''' 
        ''' All producer queues must be created before getting the consumer's enumerable object.</remarks>
        Public Function GetProducerQueue() As BlockingCollection(Of T)
            Dim result As BlockingCollection(Of T) = If(boundedCapacity > 0, New BlockingCollection(Of T)(boundedCapacity), New BlockingCollection(Of T)())
            SyncLock producersLock
                If _mode <> Mode.Starting Then
                    Throw New InvalidOperationException("Cannot get new producer queue for running multiplexer")
                End If

                Dim index = producers.Length
                Array.Resize(producers, index + 1)
                producers(index).Index = index
                producers(index).Collection = result
                producers(index).IsCompleted = False
                producers(index).HasPendingValue = False
                producers(index).PendingValue = Nothing
                producers(index).PendingLockId = -1
                producers(index).LastLockId = -1
            End SyncLock
            Return result
        End Function

        ''' <summary>
        ''' Creates an enumerable object for use by the consumer.
        ''' </summary>
        ''' <returns>An enumeration of values. The order of the values will respect the lock order of the
        ''' producer queues. This method may be called only one time for this object.</returns>
        Public Function GetConsumingEnumerable() As IEnumerable(Of T)
            Return GetConsumingEnumerable(CancellationToken.None)
        End Function

        Public Class DoWorkArgs
            Public Property Token As CancellationToken
            Public Property ElementQueue As Queue(Of T)
            'This property is used to indicate if the worker finished with the same behavior as "yield break".
            Public Property IsYieldBreak As Boolean
        End Class

        ''' <summary>
        ''' The handler of DoWork event.
        ''' </summary>
        Private Sub FillConsumingElements(ByVal sender As Object, ByVal e As DoWorkEventArgs)
            Dim arg As DoWorkArgs = CType(e.Argument, DoWorkArgs)
            Dim complete As Boolean = False
            Do While (Not complete) OrElse producers.Any(Function(info) info.HasPendingValue)
                ' Yield case 1: Value with the next lock id is in a lookahead buffer
                If producers.Any(Function(info) info.HasPendingValue AndAlso info.PendingLockId = nextLockId) Then
                    Dim index As Integer = producers.Single(Function(info) info.HasPendingValue AndAlso info.PendingLockId = nextLockId).Index
                    Dim item = producers(index).PendingValue

                    ' clear lookahead buffer
                    producers(index).HasPendingValue = False
                    producers(index).PendingValue = Nothing
                    producers(index).PendingLockId = -1
                    producers(index).LastLockId = nextLockId

                    ' consume value
                    nextLockId += 1
                    arg.ElementQueue.Enqueue(item)
                    ' Look ahead values exist but we didn't find the next lock id and there are no more
                    ' values to read from the producer queues. This means that producer blocking collections 
                    ' violated the contract by failing to give all lock ids between the lowest to the highest observed.
                ElseIf complete Then
                    ' Error occurs only for normal termination, not cancellation
                    If Not arg.Token.IsCancellationRequested Then
                        Throw New InvalidOperationException("Producer blocking collections completed before giving required lock id " & nextLockId.ToString() & ". All values up to " & producers.Where(Function(info) info.HasPendingValue).Select(Function(info) info.PendingLockId).Max() & " are required.")
                    End If
                Else
                    Do While Not complete
                        ' Select producers without lookahead values.
                        Dim waitList = producers.Where(Function(info) (Not info.HasPendingValue) AndAlso (Not info.IsCompleted)).Select(Function(info) info.Collection).ToArray()

                        If waitList.Length = 0 Then
                            If arg.Token.IsCancellationRequested Then
                                arg.IsYieldBreak = True
                                Return
                            Else
                                Throw New InvalidOperationException("Producer blocking collections omitted required value " & nextLockId.ToString())
                            End If
                        End If

                        Dim item As T = Nothing
                        Dim waitListIndex As Integer = -1
                        Try
                            waitListIndex = BlockingCollection(Of T).TakeFromAny(waitList, item)
                        Catch e1 As ArgumentException
                            ' handle occurrence of AddingComplete on another thread.
                            waitListIndex = -2
                        End Try
                        If waitListIndex < 0 Then
                            For i As Integer = 0 To producers.Length - 1
                                If (Not producers(i).IsCompleted) AndAlso producers(i).Collection.IsCompleted Then
                                    producers(i).IsCompleted = True
                                End If
                            Next i
                            complete = producers.All(Function(info) info.IsCompleted)
                            Continue Do
                        End If

                        Dim index = producers.Where(Function(info) info.Collection Is waitList(waitListIndex)).Select(Function(info) info.Index).Single()
                        Dim lockId = lockOrderFn(item)

                        ' Yield case 2: Item with no ordering constraint. Consume it immediately.
                        If lockId < 0 Then
                            arg.ElementQueue.Enqueue(item)
                            ' Yield case 3: Item read is the one we are looking for. Consume it immediately.
                        ElseIf lockId = nextLockId Then
                            producers(index).LastLockId = lockId
                            nextLockId += 1
                            arg.ElementQueue.Enqueue(item)
                            Exit Do
                        ElseIf lockId < nextLockId Then
                            Throw New InvalidOperationException("Blocking queue delivered duplicate sequence number to multiplexer (1). The duplicate value is " & lockId.ToString())
                        ElseIf lockId <= producers(index).LastLockId Then
                            Throw New InvalidOperationException("Blocking queue delivered out-of-order item (2)")
                        ElseIf producers.Where(Function(info) info.HasPendingValue).Any(Function(info) info.PendingLockId.Equals(lockId)) Then
                            Throw New InvalidOperationException("Blocking queue delivered duplicate sequence number to multiplexer (2)")
                        Else
#If DEBUG Then
                            If producers(index).HasPendingValue Then
                                Throw New InvalidOperationException("Internal error-- double read from blocking collection")
                            End If
#End If
                            producers(index).HasPendingValue = True
                            producers(index).PendingValue = item
                            producers(index).PendingLockId = lockId
                        End If
                    Loop
                End If
            Loop
        End Sub

        ''' <summary>
        ''' Creates an enumerable object for use by the consumer.
        ''' </summary>
        ''' <param name="token">The cancellation token</param>
        ''' <returns>An enumeration of values. The order of the values will respect the lock order of the
        ''' producer queues. This method may be called only one time for this object.</returns>
        Public Function GetConsumingEnumerable(ByVal token As CancellationToken) As IEnumerable(Of T)
            SyncLock producersLock
                If producers.Length = 0 Then
                    Throw New InvalidOperationException("Multiplexer requires at least one producer before getting consuming enumerable")
                End If
                If _mode <> Mode.Starting Then
                    Throw New InvalidOperationException("Cannot get enumerator of multiplexer that has already been started")
                End If
                _mode = Mode.Running
            End SyncLock

            Dim worker As New BackgroundWorker()
            AddHandler worker.DoWork, AddressOf Me.FillConsumingElements
            Dim enumerable As New AutoAddedEnumberable(Of T)(worker)
            Dim arg As New DoWorkArgs()
            arg.Token = token
            arg.ElementQueue = enumerable.ElementsQueue
            arg.IsYieldBreak = False
            worker.RunWorkerAsync(arg)
            If arg.IsYieldBreak = True Then
                Return enumerable
            End If

            SyncLock producersLock
                If _mode = Mode.Running Then
                    _mode = Mode.Finished
                End If
            End SyncLock
            Return enumerable
        End Function

        ''' <summary>
        ''' Returns an enumeration of all values that have been read by the multiplexer but not yet consumed.
        ''' </summary>
        ''' <returns>The enumerable object</returns>
        Public Function GetCleanupEnumerable() As IEnumerable(Of T)
            Dim returnValue As New List(Of T)
            SyncLock producersLock
                If _mode = Mode.Finished OrElse _mode = Mode.Running Then
                    _mode = Mode.CleanupStarted
                Else
                    Return returnValue
                End If
            End SyncLock
            For Each p In producers
                If p.HasPendingValue Then
                    returnValue.Add(p.PendingValue)
                End If
            Next p
            Return returnValue
        End Function

        ''' <summary>
        ''' Returns the number of items in all of the producer queues and in the multiplexer's buffers
        ''' </summary>
        Public ReadOnly Property Count() As Integer
            Get
                Dim result As Integer = 0
                For Each p In producers
                    result += p.Collection.Count
                    result += If(p.HasPendingValue, 1, 0)
                Next p
                Return result
            End Get
        End Property

#End Region
    End Class

    ''' <summary>
    ''' You can use this class to create IEnumerable whose element is added by your own BackgroundWorker. this class's ElementsQueue
    ''' doesn't guarantee all elements you added is remained.
    ''' </summary>
    Public Class AutoAddedEnumberable(Of T)
        Implements IEnumerable(Of T), IEnumerator(Of T)
        Private _worker As BackgroundWorker
        ''' <summary>
        ''' This flag is used to indicate whether user's work run completely.
        ''' </summary>
        Private _workerFinished As Boolean = False
        ''' <summary>
        ''' The time is used to wait user's worker add elements.
        ''' </summary>
        Private _waitTime As Integer = 50

        ''' <summary>
        ''' This queue is used to buffer element.
        ''' </summary>
        Private _elementsQueue As New Queue(Of T)()
        Public Property ElementsQueue() As Queue(Of T)
            Get
                Return _elementsQueue
            End Get
            Set(ByVal value As Queue(Of T))
                _elementsQueue = value
            End Set
        End Property

        Public Sub New(ByVal addElementWorker As BackgroundWorker)
            _worker = addElementWorker
            AddHandler _worker.RunWorkerCompleted, AddressOf Worker_RunWorkerCompleted
        End Sub
        Public Sub New(ByVal addElementWorker As BackgroundWorker, ByVal timeOut As Integer)
            Me.New(addElementWorker)
            _waitTime = timeOut
        End Sub
        Private Sub Worker_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
            _workerFinished = True
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return Me
        End Function

        Private Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return Me
        End Function

        Public ReadOnly Property Current() As T Implements IEnumerator(Of T).Current
            Get
                Return _elementsQueue.Dequeue()
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            _worker = Nothing
            _elementsQueue = Nothing
        End Sub

        Private ReadOnly Property IEnumerator_Current() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            If ElementsQueue.Count = 0 Then
                If Not _workerFinished Then
                    Do While _elementsQueue.Count = 0 AndAlso (Not _workerFinished)
                        Thread.Sleep(_waitTime)
                    Loop
                    If _elementsQueue.Count = 0 Then
                        Return False
                    Else
                        Return True
                    End If
                End If
                Return False
            Else
                Return True
            End If
        End Function

        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            _elementsQueue.Clear()
        End Sub
    End Class
End Namespace
