
Imports System
Imports ActiproSoftware.SyntaxEditor
Imports ActiproSoftware.SyntaxEditor.Addons.Dynamic


''' <summary>
''' Provides an implementation of a <c>HTML</c> syntax language that can perform automatic outlining.
''' </summary>
Public Class HtmlDynamicSyntaxLanguage
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
	''' Initializes a new instance of the <c>HtmlDynamicSyntaxLanguage</c> class. 
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
		Dim languageTokenKey As String = token.Language.Key + "_" + token.Key

		' See if the token starts or ends an outlining node
		Select Case languageTokenKey
			Case "CSS_PropertyStartToken"
				outliningKey = "CSS_PropertyBlock"
				tokenAction = OutliningNodeAction.Start
			Case "CSS_PropertyEndToken"
				outliningKey = "CSS_PropertyBlock"
				tokenAction = OutliningNodeAction.End
			Case "CSS_CommentStartToken"
				outliningKey = "CSS_Comment"
				tokenAction = OutliningNodeAction.Start
			Case "CSS_CommentEndToken"
				outliningKey = "CSS_Comment"
				tokenAction = OutliningNodeAction.End
			Case "JScript_OpenCurlyBraceToken"
				outliningKey = "JScript_CodeBlock"
				tokenAction = OutliningNodeAction.Start
			Case "JScript_CloseCurlyBraceToken"
				outliningKey = "JScript_CodeBlock"
				tokenAction = OutliningNodeAction.End
			Case "JScript_MultiLineCommentStartToken"
				outliningKey = "JScript_MultiLineComment"
				tokenAction = OutliningNodeAction.Start
			Case "JScript_MultiLineCommentEndToken"
				outliningKey = "JScript_MultiLineComment"
				tokenAction = OutliningNodeAction.End
			Case Else
				' If a language change is occurring (tokenAction is only initialized to Start or End on language transitions)...
				If tokenAction <> OutliningNodeAction.NoChange Then
					If token.HasFlag(LexicalParseFlags.LanguageStart) Then
						If tokenStream.Position > 0 Then
							Dim previousToken As IToken = tokenStream.ReadReverse()
							If previousToken.LexicalState.Key = "ASPDirectiveResponseWriteState" Then
								' Don't do outlining on <%=  %> blocks
								outliningKey = Nothing
								tokenAction = OutliningNodeAction.NoChange
							End If
						End If
					ElseIf token.HasFlag(LexicalParseFlags.LanguageEnd) Then
						tokenStream.Read()
						If Not tokenStream.IsDocumentEnd Then
							Dim nextToken As IToken = tokenStream.Peek()
							If nextToken.LexicalState.Key = "ASPDirectiveResponseWriteState" Then
								' Don't do outlining on <%=  %> blocks
								outliningKey = Nothing
								tokenAction = OutliningNodeAction.NoChange
							End If
						End If
					End If
				End If
		End Select
	End Sub	'GetTokenOutliningAction

	''' <summary>
	''' Allows for setting the collapsed text for the specified <see cref="OutliningNode"/>.
	''' </summary>
	''' <param name="node">The <see cref="OutliningNode"/> that is requesting collapsed text.</param>
	Public Overrides Sub SetOutliningNodeCollapsedText(ByVal node As OutliningNode)
		Select Case node.ParseData.Key
			Case "CSS_Comment"
				node.CollapsedText = "/**/"
			Case "CSS_PropertyBlock"
				node.CollapsedText = "{...}"
			Case "JScript_MultiLineComment"
				node.CollapsedText = "/**/"
		End Select
	End Sub	'SetOutliningNodeCollapsedText
End Class 'HtmlDynamicSyntaxLanguage
