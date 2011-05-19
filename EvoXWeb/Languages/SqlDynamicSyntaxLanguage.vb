
Imports System
Imports ActiproSoftware.SyntaxEditor
Imports ActiproSoftware.SyntaxEditor.Addons.Dynamic

''' <summary>
''' Provides an implementation of a <c>SQL</c> syntax language that can perform automatic outlining.
''' </summary>
Public Class SqlDynamicSyntaxLanguage
	Inherits DynamicOutliningSyntaxLanguage

	'///////////////////////////////////////////////////////////////////////////////////////////////////
	' OBJECT
	'///////////////////////////////////////////////////////////////////////////////////////////////////

	''' <summary>
	''' This constructor is for designer use only and should never be called by your code.
	''' </summary>
	Public Sub New()
	End Sub	'New

	''' <summary>
	''' Initializes a new instance of the <c>SqlDynamicSyntaxLanguage</c> class. 
	''' </summary>
	''' <param name="key">The key of the language.</param>
	''' <param name="secure">Whether the language is secure.</param>
	Public Sub New(ByVal key As String, ByVal secure As Boolean)
		MyBase.New(key, secure)
	End Sub	'New

	'///////////////////////////////////////////////////////////////////////////////////////////////////
	' PUBLIC PROCEDURES
	'///////////////////////////////////////////////////////////////////////////////////////////////////

	''' <summary>
	''' Returns token parsing information for automatic outlining that determines if the current <see cref="IToken"/>
	''' in the <see cref="TokenStream"/> starts or ends an outlining node.
	''' </summary>
	''' <param name="tokenStream">A <see cref="TokenStream"/> that is positioned at the <see cref="IToken"/> requiring outlining data.</param>
	''' <param name="outliningKey">Returns the outlining node key to assign.  A <see langword="null"/> should be returned if the token doesn't start or end a node.</param>
	''' <param name="tokenAction">Returns the <see cref="OutliningNodeAction"/> to take for the token.</param>
	Public Overrides Sub GetTokenOutliningAction(ByVal tokenStream As TokenStream, ByRef outliningKey As String, ByRef tokenAction As OutliningNodeAction)
		' Get the token
		Dim token As IToken = tokenStream.Peek()

		' See if the token starts or ends an outlining node
		Select Case token.Key
			Case "MultiLineCommentStartToken"
				outliningKey = "MultiLineComment"
				tokenAction = OutliningNodeAction.Start
			Case "MultiLineCommentEndToken"
				outliningKey = "MultiLineComment"
				tokenAction = OutliningNodeAction.End
			Case "RegionStartToken"
				outliningKey = "Region"
				tokenAction = OutliningNodeAction.Start
			Case "EndRegionStartToken"
				outliningKey = "Region"
				tokenAction = OutliningNodeAction.End
		End Select
	End Sub	'GetTokenOutliningAction

	''' <summary>
	''' Occurs after automatic outlining is performed on a <see cref="Document"/> that uses this language.
	''' </summary>
	''' <param name="document">The <see cref="Document"/> that is being modified.</param>
	''' <param name="e">A <c>DocumentModificationEventArgs</c> that contains the event data.</param>
	''' <remarks>
	''' A <see cref="DocumentModification"/> may or may not be passed in the event arguments, depending on if the outlining
	''' is performed in the main thread.
	''' </remarks>
	Protected Overrides Sub OnDocumentAutomaticOutliningComplete(ByVal document As Document, ByVal e As DocumentModificationEventArgs)
		' Collapse all outlining region nodes
		document.Outlining.RootNode.CollapseDescendants("Region")
	End Sub	'OnDocumentAutomaticOutliningComplete

	''' <summary>
	''' Allows for setting the collapsed text for the specified <see cref="OutliningNode"/>.
	''' </summary>
	''' <param name="node">The <see cref="OutliningNode"/> that is requesting collapsed text.</param>
	Public Overrides Sub SetOutliningNodeCollapsedText(ByVal node As OutliningNode)
		Dim tokens As TokenCollection = node.Document.Tokens
		Dim tokenIndex As Integer = tokens.IndexOf(node.StartOffset)

		Select Case tokens(tokenIndex).Key
			Case "MultiLineCommentStartToken"
				node.CollapsedText = "/**/"
			Case "RegionStartToken"
				Dim collapsedText As String = String.Empty
				tokenIndex = tokenIndex + 1
				While tokenIndex < tokens.Count
					If tokens(tokenIndex).Key = "CommentStringEndToken" Then
						Exit While
					End If
					collapsedText += tokens.Document.GetTokenText(tokens(tokenIndex))
					tokenIndex = tokenIndex + 1
				End While
				node.CollapsedText = collapsedText.Trim()
		End Select
	End Sub	'SetOutliningNodeCollapsedText

End Class 'SqlDynamicSyntaxLanguage
