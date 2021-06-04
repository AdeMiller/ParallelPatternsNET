'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================


Namespace Microsoft.Practices.ParallelGuideSamples.ImagePipeline
    Friend NotInheritable Class Program
        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Private Sub New()
        End Sub
        <STAThread()>
        Shared Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Dim form As New MainForm()
            Application.Run(form)
        End Sub
    End Class
End Namespace
