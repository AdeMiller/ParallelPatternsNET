'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Namespace Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects
    ' This class is simply a placeholder to show that tasks in the graph can take
    ' different data types as inputs and outputs. They illustrate data moving through
    ' the model.
    Public Class MarketRecommendation
        Private ReadOnly recommendation As String

        Public ReadOnly Property Value() As String
            Get
                Return recommendation
            End Get
        End Property

        Public Sub New(ByVal recommendation As String)
            Me.recommendation = recommendation
        End Sub
    End Class
End Namespace