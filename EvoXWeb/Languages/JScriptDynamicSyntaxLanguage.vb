
Imports System
Imports ActiproSoftware.SyntaxEditor
Imports ActiproSoftware.SyntaxEditor.Addons.Dynamic

''' <summary>
''' Provides an implementation of a <c>JScript</c> syntax language that can perform automatic outlining.
''' </summary>
Public Class JScriptDynamicSyntaxLanguage
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
	''' Initializes a new instance of the <c>JScriptDynamicSyntaxLanguage</c> class. 
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
			Case "OpenCurlyBraceToken"
				outliningKey = "CodeBlock"
				tokenAction = OutliningNodeAction.Start
			Case "CloseCurlyBraceToken"
				outliningKey = "CodeBlock"
				tokenAction = OutliningNodeAction.End
			Case "MultiLineCommentStartToken"
				outliningKey = "MultiLineComment"
				tokenAction = OutliningNodeAction.Start
			Case "MultiLineCommentEndToken"
				outliningKey = "MultiLineComment"
				tokenAction = OutliningNodeAction.End
		End Select
	End Sub	'GetTokenOutliningAction


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
		End Select
	End Sub	'SetOutliningNodeCollapsedText

End Class 'JScriptDynamicSyntaxLanguage
