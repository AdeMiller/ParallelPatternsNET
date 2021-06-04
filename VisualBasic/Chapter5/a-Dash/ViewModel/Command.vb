'===============================================================================
' Microsoft patterns & practices
' Parallel Programming Guide
'===============================================================================
' Copyright © Microsoft Corporation.  All rights reserved.
' This code released under the terms of the 
' Microsoft patterns & practices license (http://parallelpatterns.codeplex.com/license).
'===============================================================================


Namespace Microsoft.Practices.ParallelGuideSamples.ADash.ViewModel
    ''' <summary>
    ''' A command object recognized by WPF. Implements the System.Windows.Input.ICommand interface.
    ''' </summary>
    Public Class Command
        Implements ICommand
#Region "Fields"

        Private ReadOnly _execute As Action(Of Object)
        Private ReadOnly _canExecute As Predicate(Of Object)
        Private canExecuteChangedHandler As EventHandler

#End Region ' Fields

#Region "Constructors"

        ''' <summary>
        ''' Creates a new command that can always execute.
        ''' </summary>
        ''' <param name="execute">The execution logic.</param>
        Public Sub New(ByVal execute As Action(Of Object))
            Me.New(execute, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new command.
        ''' </summary>
        ''' <param name="execute">The execution logic.</param>
        ''' <param name="canExecute">The execution status logic.</param>
        Public Sub New(ByVal executeAction As Action(Of Object), ByVal canExecuteCommand As Predicate(Of Object))
            If executeAction Is Nothing Then
                Throw New ArgumentNullException("executeAction")
            End If

            _execute = executeAction
            _canExecute = canExecuteCommand
        End Sub

#End Region ' Constructors

#Region "ICommand Members"

        ''' <summary>
        ''' Tests whether the current data context allows this command to be run
        ''' </summary>
        ''' <param name="parameter">Parameter passed to the object's CanExecute delegate</param>
        ''' <returns>True if the command is currently enabled; false if not enabled.</returns>
        <DebuggerStepThrough()> _
        Public Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute
            Return If(_canExecute Is Nothing, True, _canExecute(parameter))
        End Function

        ''' <summary>
        ''' Event that is raised when the "CanExecute" status of this command changes
        ''' </summary>
        Public Custom Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
            AddHandler(ByVal value As EventHandler)
                canExecuteChangedHandler = CType(System.Delegate.Combine(canExecuteChangedHandler, value), EventHandler)
                AddHandler CommandManager.RequerySuggested, value
            End AddHandler

            RemoveHandler(ByVal value As EventHandler)
                canExecuteChangedHandler = CType(System.Delegate.Remove(canExecuteChangedHandler, value), EventHandler)
                RemoveHandler CommandManager.RequerySuggested, value
            End RemoveHandler
            RaiseEvent(ByVal sender As Object, ByVal e As EventArgs)
            End RaiseEvent
        End Event

        ''' <summary>
        ''' Performs the work of the "execute" delegate.
        ''' </summary>
        ''' <param name="parameter"></param>
        Public Sub Execute(ByVal parameter As Object) Implements ICommand.Execute
            _execute(parameter)
        End Sub

#End Region ' ICommand Members

        ''' <summary>
        ''' Causes the CanExecuteChanged handler to run.
        ''' </summary>
        ''' <remarks>Should only be invoked by the view model</remarks>
        Public Sub NotifyExecuteChanged()
            Dim handler As EventHandler = canExecuteChangedHandler
            If handler IsNot Nothing Then
                handler(Me, EventArgs.Empty)
            End If
        End Sub
    End Class
End Namespace