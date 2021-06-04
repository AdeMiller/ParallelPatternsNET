'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.ADash.BusinessObjects

Namespace Microsoft.Practices.ParallelGuideSamples.ADash
    Public Interface IAnalysisEngine
        Inherits IDisposable
        Function DoAnalysisParallel() As AnalysisTasks
        Function DoAnalysisSequential() As MarketRecommendation
        Sub TryCancelAnalysis()
    End Interface
End Namespace
