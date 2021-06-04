'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Collections.ObjectModel

Namespace Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects
    ' This class is simply a placeholder to show that tasks in the graph can take
    ' different data types as inputs and outputs. They illustrate data moving through
    ' the model.
    Public NotInheritable Class StockData
        Private ReadOnly _name As String
        Private ReadOnly _priceHistory As ReadOnlyCollection(Of Double)

        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        Public ReadOnly Property PriceHistory() As ReadOnlyCollection(Of Double)
            Get
                Return _priceHistory
            End Get
        End Property

        Public Sub New(ByVal name As String, ByVal priceHistory() As Double)
            Me._name = name
            Me._priceHistory = New ReadOnlyCollection(Of Double)(priceHistory)
        End Sub

        ' Implement value equality

        Public Shared Operator =(ByVal a As StockData, ByVal b As StockData) As Boolean
            If Object.ReferenceEquals(a, b) Then
                Return True
            End If
            If (CObj(a) Is Nothing) OrElse (CObj(b) Is Nothing) Then
                Return False
            End If
            If a.Name <> b.Name Then
                Return False
            End If
            If a.PriceHistory.Count <> b.PriceHistory.Count Then
                Return False
            End If
            For i As Integer = 0 To a.PriceHistory.Count - 1
                If a.PriceHistory(i) <> b.PriceHistory(i) Then
                    Return False
                End If
            Next i

            Return True
        End Operator

        Public Shared Operator <>(ByVal a As StockData, ByVal b As StockData) As Boolean
            Return Not (a = b)
        End Operator

        Public Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
            If obj Is Nothing Then
                Return False
            End If
            Return Equals(TryCast(obj, StockData))
        End Function

        Public Overloads Function Equals(ByVal d As StockData) As Boolean
            If d Is Nothing Then
                Return False
            End If
            Return (Me = d)
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim result As Integer = _name.GetHashCode() Xor _priceHistory.Count
            For i As Integer = 0 To _priceHistory.Count - 1
                result = result Xor _priceHistory(i).GetHashCode()
            Next i
            Return result
        End Function
    End Class

    ''' <summary>
    ''' A data set with time series price information for various financial assets
    ''' </summary>
    Public NotInheritable Class StockDataCollection
        Inherits ReadOnlyCollection(Of StockData)
        Public Sub New(ByVal data As IList(Of StockData))
            MyBase.New(data)
        End Sub
    End Class
End Namespace