' ===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
' ===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
' ===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.Utilities
Imports AccountRecord = System.Collections.Generic.KeyValuePair(Of Integer, Microsoft.Practices.ParallelGuideSamples.CreditReview.Account)

Namespace Microsoft.Practices.ParallelGuideSamples.CreditReview
    ''' <summary>
    ''' Repository of customer accounts
    ''' </summary>
    Friend Class AccountRepository
        ''' <summary>
        ''' Repository is implemented by a dictionary from customer account numbers to account data,
        ''' an array of monthly balances etc.
        ''' </summary>
        Private ReadOnly accounts As New Dictionary(Of Integer, Account)()

        ''' <summary>
        ''' Constructor, allocate account for customerCount customers, each with months balance history
        ''' </summary>
        Public Sub New(ByVal customerCount As Integer, ByVal months As Integer, ByVal overdraft As Double)
            For customer As Integer = 0 To customerCount - 1
                accounts(customer) = New Account(months, overdraft)
            Next customer
        End Sub

        ''' <summary>
        ''' Assign every account with monthly balances that fit randomly assigned trend
        ''' </summary>
        Public Sub AssignRandomTrends(ByVal goodBalance As Trend, ByVal badBalance As Trend, ByVal variation As Double, ByVal random As Random)
            For Each record As AccountRecord In accounts
                Dim account = record.Value
                account.AssignRandomTrend(goodBalance, badBalance, variation, random)
            Next record
        End Sub

        ''' <summary>
        ''' Property that returns collection of all accounts in repository
        ''' </summary>
        Public ReadOnly Property AllAccounts() As IEnumerable(Of Account)
            Get
                Return accounts.Values
            End Get
        End Property

        ''' <summary>
        ''' Print first rows accounts from firstMonth for months, including predictions and warnings
        ''' </summary>
        Public Sub Print(ByVal rows As Integer, ByVal firstMonth As Integer, ByVal months As Integer)
            ' Print column headings
            Console.WriteLine()
            Console.WriteLine("Customer   Recent balances for month number   Predicted balances and warnings")
            Console.Write("        ")
            For month As Integer = firstMonth To firstMonth + months - 1
                Console.Write("{0,9:D}", month)
            Next month
            Console.WriteLine("      Seq.    Parallel       PLINQ" & vbLf)

            ' Print results for first nRows customers
            For customer As Integer = 0 To rows - 1
                If accounts.ContainsKey(customer) Then
                    Console.Write("{0,7:0.#} ", customer)
                    Dim acc = accounts(customer)
                    acc.PrintBalance(firstMonth, months)
                    Console.WriteLine("  {0,8:F} {1}  {2,8:F} {3}  {4,8:F} {5}", acc.SeqPrediction, If(acc.SeqWarning, "W"c, " "c), acc.ParPrediction, If(acc.ParWarning, "W"c, " "c), acc.PlinqPrediction, If(acc.PlinqWarning, "W"c, " "c))
                End If
            Next customer
        End Sub
    End Class
End Namespace
