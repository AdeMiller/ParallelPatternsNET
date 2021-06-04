' ===============================================================================
'  Microsoft patterns & practices
'  Parallel Programming Guide
' ===============================================================================
'  Copyright © Microsoft Corporation.  All rights reserved.
'  This code released under the terms of the 
'  Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
' ===============================================================================

Imports System.Globalization
Imports System.Threading.Tasks
Imports Microsoft.Practices.ParallelGuideSamples.Utilities

Namespace Microsoft.Practices.ParallelGuideSamples.CreditReview
    Friend Class Program
        Private Const NumberOfMonths As Integer = 3

        Private Shared Sub UpdatePredictionsSequential(ByVal accounts As AccountRepository)
            For Each account As Account In accounts.AllAccounts
                Dim trend As Trend = SampleUtilities.Fit(account.Balance)
                Dim prediction As Double = trend.Predict(account.Balance.Length + NumberOfMonths)
                account.SeqPrediction = prediction
                account.SeqWarning = prediction < account.Overdraft
            Next account
        End Sub

        Private Shared Sub UpdatePredictionsParallel(ByVal accounts As AccountRepository)
            Parallel.ForEach(accounts.AllAccounts, Sub(account)
                                                       Dim trend As Trend = SampleUtilities.Fit(account.Balance)
                                                       Dim prediction As Double = trend.Predict(account.Balance.Length + NumberOfMonths)
                                                       account.ParPrediction = prediction
                                                       account.ParWarning = prediction < account.Overdraft
                                                   End Sub)
        End Sub


        Private Shared Sub UpdatePredictionsPlinq(ByVal accounts As AccountRepository)
            accounts.AllAccounts.AsParallel().ForAll(Sub(account)
                                                         Dim trend As Trend = SampleUtilities.Fit(account.Balance)
                                                         Dim prediction As Double = trend.Predict(account.Balance.Length + NumberOfMonths)
                                                         account.PlinqPrediction = prediction
                                                         account.PlinqWarning = prediction < account.Overdraft
                                                     End Sub)
        End Sub


        ''' <summary>
        ''' Usage: CreditReview n, optional n is number of customers, use 100,000+ for meaningful timings
        ''' </summary>
        Shared Sub Main(ByVal args() As String)
            Console.WriteLine("Credit Review Sample" & vbLf)
#If DEBUG Then
            Console.WriteLine("For most accurate timing results, use Release build." & vbLf)
#End If

            Dim random As New Random(1) ' seed 1 makes runs reproducible

            ' Defaults for data generation, may override some on command line
            Dim months As Integer = 36
            Dim customerCount As Integer = 1000000 ' for data runs make big enough for significant timing measurements
            Dim goodBalance As Trend = New Trend With {.Slope = 0.0, .Intercept = 0.0}
            Dim badBalance As Trend = New Trend With {.Slope = -150.0, .Intercept = 0.0}
            Const variation As Double = 100.0
            Const overdraft As Double = -1000.0 ' Default overdraft limit

            ' Printed table of results
            Const rows As Integer = 8
            Const cols As Integer = 4

            ' Optionally override some defaults on command line
            If args.Length > 0 Then
                customerCount = Int32.Parse(args(0), CultureInfo.CurrentCulture)
            End If
            If args.Length > 1 Then
                months = Int32.Parse(args(1), CultureInfo.CurrentCulture)
            End If
            If months < 4 Then
                months = 4 ' PrintBalance requires at least 4 months
            End If

            ' Force JIT compilation before timing tests
            Const fewCustomers As Integer = 10
            Const fewMonths As Integer = 3
            Dim smallAccounts As New AccountRepository(fewCustomers, fewMonths, overdraft)
            smallAccounts.AssignRandomTrends(goodBalance, badBalance, variation, random)
            UpdatePredictionsSequential(smallAccounts)
            UpdatePredictionsParallel(smallAccounts)
            UpdatePredictionsPlinq(smallAccounts)

            ' Create accounts for timing tests
            Dim accounts As New AccountRepository(customerCount, months, overdraft)
            accounts.AssignRandomTrends(goodBalance, badBalance, variation, random)

            ' Print summary of accounts  
            Console.WriteLine()
            Console.WriteLine("{0} customers, {1} months in each account", customerCount, months)

            ' Execute sequential and parallel versions, print timings
            Console.WriteLine()
            SampleUtilities.TimedRun(Function()
                                         UpdatePredictionsSequential(accounts)
                                         Return customerCount
                                     End Function, "Sequential")
            SampleUtilities.TimedRun(Function()
                                         UpdatePredictionsParallel(accounts)
                                         Return customerCount
                                     End Function, "  Parallel")
            SampleUtilities.TimedRun(Function()
                                         UpdatePredictionsPlinq(accounts)
                                         Return customerCount
                                     End Function, "     PLINQ")

            ' Print a few accounts including predictions and warnings
            accounts.Print(rows, months - cols, cols) ' print the last few months

            Console.WriteLine(vbLf & "Run complete... press enter to finish.")
            Console.ReadLine()
        End Sub
    End Class
End Namespace
