'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.CreditReview
    ''' <summary>
    ''' One customer's account data: array of monthly balances, also predictions and warnings
    ''' </summary>
    Friend Class Account

        Public Property Balance As Double()
        Public Property Overdraft As Double
        Public Property SeqPrediction As Double
        Public Property ParPrediction As Double
        Public Property PlinqPrediction As Double
        Public Property SeqWarning As Boolean
        Public Property ParWarning As Boolean
        Public Property PlinqWarning As Boolean

        ''' <summary>
        ''' Constructor, allocate balance history for months, assign overdraft
        ''' </summary>
        Public Sub New(ByVal months As Integer, ByVal overdraft As Double)
            Balance = New Double(months - 1) {}
            Me.Overdraft = overdraft
        End Sub

        ''' <summary>
        ''' Assign balance history to vary randomly around randomly assigned trend
        ''' </summary>
        Public Sub AssignRandomTrend(ByVal goodBalance As Trend, ByVal badBalance As Trend, ByVal variation As Double, ByVal random As Random)
            ' choose random trend
            Const rateScale As Double = 100.0
            Const balanceScale As Double = 100.0
            Dim rateMean As Double = (goodBalance.Slope + badBalance.Slope) / 2
            Dim initialBalanceMean As Double = (goodBalance.Intercept + badBalance.Intercept) / 2
            Dim rate As Double = rateMean + rateScale * random.NextDouble()
            Dim initialBalance As Double = initialBalanceMean + balanceScale * random.NextDouble()
            Dim trend As Trend = New Trend With {.Slope = rate, .Intercept = initialBalance}

            ' balance history is trend plus noise
            For i As Integer = 0 To Balance.Length - 1
                Balance(i) = trend.Predict(i) + variation * random.NextDouble()
            Next i
        End Sub

        ''' <summary>
        ''' Print balances for months starting at firstMonth
        ''' </summary>
        Public Sub PrintBalance(ByVal firstMonth As Integer, ByVal months As Integer)
            For month As Integer = firstMonth To firstMonth + months - 1
                If month < Balance.Length Then
                    Console.Write("{0,9:F}", Balance(month))
                Else
                    Console.Write("        ") ' line up columns even if data missing
                End If
            Next month
            ' no WriteLine, may want to print more 
        End Sub
    End Class
End Namespace