'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================
Imports System.Diagnostics.CodeAnalysis

Namespace Microsoft.Practices.ParallelGuideSamples.RelatedPatterns
    Public NotInheritable Class UnsafeSingleton
        Private Sub New()
        End Sub

        ' BAD Code, do not use!
        Private Shared _instance As UnsafeSingleton = Nothing

        Public Shared ReadOnly Property Instance() As UnsafeSingleton
            Get
                ' If this is executed on multiple threads more than one
                ' instance of the singleton may be created.
                If _instance Is Nothing Then
                    _instance = New UnsafeSingleton()
                End If
                Return _instance
            End Get
        End Property
    End Class

    Public NotInheritable Class LazyDoubleLockedSingleton

        Private Shared _instance As LazyDoubleLockedSingleton = Nothing
        Private Shared sync As New Object()

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property Instance() As LazyDoubleLockedSingleton
            Get
                If _instance Is Nothing Then
                    SyncLock sync
                        If _instance Is Nothing Then
                            _instance = New LazyDoubleLockedSingleton()
                        End If
                    End SyncLock
                End If
                Return _instance
            End Get
        End Property
    End Class

    Public NotInheritable Class NestedSingleton
        Private Sub New()
        End Sub

        Public Shared ReadOnly Property Instance() As NestedSingleton
            Get
                Return SingletonCreator.PrivateInstance
            End Get
        End Property

        Private NotInheritable Class SingletonCreator
            Private Sub New()
            End Sub
            <SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")> Shared Sub New()
            End Sub

            Friend Shared ReadOnly PrivateInstance As New NestedSingleton()
        End Class
    End Class

    Public NotInheritable Class LazySingleton
        Private Shared ReadOnly _instance As New Lazy(Of LazySingleton)(Function() New LazySingleton())

        Private Sub New()
        End Sub

        Public Shared ReadOnly Property Instance() As LazySingleton
            Get
                Return _instance.Value
            End Get
        End Property
    End Class
End Namespace
