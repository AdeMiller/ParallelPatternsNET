'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================

Imports Microsoft.Practices.ParallelGuideSamples.ADash.ViewModel

Namespace Microsoft.Practices.ParallelGuideSamples.ADash
    Partial Public Class MainWindow
        Inherits Window
        ' Provide a strongly typed ViewModel property for the View.
        ' If your application uses a dependency injection container this
        ' makes it simpler to configure the container to inject the 
        ' ViewModel.
        Public Property ViewModel() As IMainWindowViewModel
            Get
                Return TryCast(DataContext, IMainWindowViewModel)
            End Get
            Set(ByVal value As IMainWindowViewModel)
                DataContext = value
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
        End Sub
    End Class
End Namespace
