﻿'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Diagnostics.CodeAnalysis
Namespace Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects
    ' This class is simply a placeholder to show that tasks in the graph can take
    ' different data types as inputs and outputs. They illustrate data moving through
    ' the model.
    Public NotInheritable Class MarketModeler
        ' data parameter here is placeholder intended for illustrative purposes
        Private Sub New()
        End Sub
        <SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="data")> _
        Public Shared Function Run(ByVal data As StockAnalysisCollection) As MarketModel
            Return New MarketModel()
        End Function
    End Class
End Namespace