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
    Public NotInheritable Class StockAnalysis
        Private ReadOnly _name As String
        Private ReadOnly _volatility As Double

        Public ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        Public ReadOnly Property Volatility() As Double
            Get
                Return _volatility
            End Get
        End Property

        Public Sub New(ByVal name As String, ByVal volatility As Double)
            Me._name = name
            Me._volatility = volatility
        End Sub
    End Class

    ' This class is simply a placeholder to show that tasks in the graph can take
    ' different data types as inputs and outputs. They illustrate data moving through
    ' the model.
    Public Class StockAnalysisCollection
        Inherits ReadOnlyCollection(Of StockAnalysis)
        Public Sub New(ByVal data As IList(Of StockAnalysis))
            MyBase.New(data)
        End Sub
    End Class
End Namespace