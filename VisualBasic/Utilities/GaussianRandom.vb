'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================


Namespace Microsoft.Practices.ParallelGuideSamples.Utilities
    ''' <summary>
    ''' Normally distributed random value generator
    ''' </summary>
    Public Class GaussianRandom
        Private ReadOnly random As New Random()
        Private ReadOnly mean As Double
        Private ReadOnly standardDeviation As Double

#Region "Constructors"

        ''' <summary>
        ''' Creates a new instance of a normally distributed random value generator
        ''' using the specified mean and standard deviation.
        ''' </summary>
        ''' <param name="mean">The average value produced by this generator</param>
        ''' <param name="standardDeviation">The amount of variation in the values produced by this generator</param>
        Public Sub New(ByVal mean As Double, ByVal standardDeviation As Double)
            random = New Random()
            Me.mean = mean
            Me.standardDeviation = standardDeviation
        End Sub

        ''' <summary>
        ''' Creates a new instance of a normally distributed random value generator
        ''' using the specified mean, standard deviation and seed.
        ''' </summary>
        ''' <param name="mean">The average value produced by this generator</param>
        ''' <param name="standardDeviation">The amount of variation in the values produced by this generator</param>
        ''' <param name="seed">A number used to calculate a starting value for the pseudo-random number
        ''' sequence. If a negative number is specified, the absolute value of the number
        ''' is used.</param>
        Public Sub New(ByVal mean As Double, ByVal standardDeviation As Double, ByVal seed As Integer)
            random = New Random(seed)
            Me.mean = mean
            Me.standardDeviation = standardDeviation
        End Sub
#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Samples the distribution and returns a random integer
        ''' </summary>
        ''' <returns>A normally distributed random number rounded to the nearest integer</returns>
        Public Function NextInteger() As Integer
            Return CInt(Fix(Math.Floor([Next]() + 0.5)))
        End Function

        ''' <summary>
        ''' Samples the distribution
        ''' </summary>
        ''' <returns>A random sample from a normal distribution</returns>
        Public Function [Next]() As Double
            Dim x As Double = 0.0

            ' get the next value in the interval (0, 1) from the underlying uniform distribution
            Do While x = 0.0 OrElse x = 1.0
                x = random.NextDouble()
            Loop

            ' transform uniform into normal
            Return SampleUtilities.GaussianInverse(x, mean, standardDeviation)
        End Function
#End Region
    End Class
End Namespace
