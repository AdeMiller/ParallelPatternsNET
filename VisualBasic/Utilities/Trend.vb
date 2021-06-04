'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports System.Diagnostics.CodeAnalysis

Namespace Microsoft.Practices.ParallelGuideSamples.Utilities
    ''' <summary>
    ''' Linear trend from slope and intercept. Predict y given any x value using the formula
    ''' y = slope * x + intercept.
    ''' </summary>
    <SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")> _
    Public Structure Trend
        ''' <summary>
        ''' The change in y per unit of x.
        ''' </summary>
        Public Property Slope() As Double

        ''' <summary>
        ''' The value of y when x is zero.
        ''' </summary>
        Public Property Intercept() As Double

        ''' <summary>
        ''' Predicts a y value given any x value using the formula y = slope * x + intercept.
        ''' </summary>
		''' <param name="x">The x value</param>
        ''' <returns>The predicted y value</returns>
		Public Function Predict(ByVal x As Double) As Double
			Return Slope * x + Intercept
		End Function
    End Structure
End Namespace
