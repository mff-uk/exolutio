
Imports System
Imports ActiproSoftware.SyntaxEditor
Imports ActiproSoftware.SyntaxEditor.Addons.Dynamic


''' <summary>
''' Provides an implementation of a <c>VB.NET</c> syntax language that can perform automatic outlining.
''' </summary>
Public Class VBDotNetDynamicSyntaxLanguage
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
	''' Initializes a new instance of the <c>VBDotNetDynamicSyntaxLanguage</c> class. 
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
			Case "XMLCommentStartToken"
				outliningKey = "XMLComment"
				tokenAction = OutliningNodeAction.Start
			Case "XMLCommentEndToken"
				outliningKey = "XMLComment"
				tokenAction = OutliningNodeAction.End
			Case "SubReservedWordToken"
				outliningKey = "SubBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndSubReservedWordToken"
				outliningKey = "SubBlock"
				tokenAction = OutliningNodeAction.End
			Case "FunctionReservedWordToken"
				outliningKey = "FunctionBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndFunctionReservedWordToken"
				outliningKey = "FunctionBlock"
				tokenAction = OutliningNodeAction.End
			Case "PropertyReservedWordToken"
				outliningKey = "PropertyBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndPropertyReservedWordToken"
				outliningKey = "PropertyBlock"
				tokenAction = OutliningNodeAction.End
			Case "ClassReservedWordToken"
				outliningKey = "ClassBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndClassReservedWordToken"
				outliningKey = "ClassBlock"
				tokenAction = OutliningNodeAction.End
			Case "InterfaceReservedWordToken"
				outliningKey = "InterfaceBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndInterfaceReservedWordToken"
				outliningKey = "InterfaceBlock"
				tokenAction = OutliningNodeAction.End
			Case "EnumReservedWordToken"
				outliningKey = "EnumBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndEnumReservedWordToken"
				outliningKey = "EnumBlock"
				tokenAction = OutliningNodeAction.End
			Case "StructureReservedWordToken"
				outliningKey = "StructureBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndStructureReservedWordToken"
				outliningKey = "StructureBlock"
				tokenAction = OutliningNodeAction.End
			Case "ModuleReservedWordToken"
				outliningKey = "ModuleBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndModuleReservedWordToken"
				outliningKey = "ModuleBlock"
				tokenAction = OutliningNodeAction.End
			Case "NamespaceReservedWordToken"
				outliningKey = "NamespaceBlock"
				tokenAction = OutliningNodeAction.Start
			Case "EndNamespaceReservedWordToken"
				outliningKey = "NamespaceBlock"
				tokenAction = OutliningNodeAction.End
			Case "RegionPreProcessorDirectiveStartToken"
				outliningKey = "RegionPreProcessorDirective"
				tokenAction = OutliningNodeAction.Start
			Case "EndRegionPreProcessorDirectiveStartToken"
				outliningKey = "RegionPreProcessorDirective"
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
		document.Outlining.RootNode.CollapseDescendants("RegionPreProcessorDirective")
	End Sub	'OnDocumentAutomaticOutliningComplete

	''' <summary>
	''' Allows for setting the collapsed text for the specified <see cref="OutliningNode"/>.
	''' </summary>
	''' <param name="node">The <see cref="OutliningNode"/> that is requesting collapsed text.</param>
	Public Overrides Sub SetOutliningNodeCollapsedText(ByVal node As OutliningNode)
		Dim tokens As TokenCollection = node.Document.Tokens
		Dim tokenIndex As Integer = tokens.IndexOf(node.StartOffset)
		Dim tokenKey As String = tokens(tokenIndex).Key

		Select Case tokenKey
			Case "XMLCommentStartToken"
				node.CollapsedText = "/**/"
			Case "FunctionReservedWordToken", "PropertyReservedWordToken", "SubReservedWordToken", "ClassReservedWordToken", "InterfaceReservedWordToken", "EnumReservedWordToken", "StructureReservedWordToken", "ModuleReservedWordToken", "NamespaceReservedWordToken"
				Dim collapsedText As String = String.Empty
				tokenIndex = tokenIndex + 1
				While tokenIndex < tokens.Count
					If Not (tokens(tokenIndex).IsWhitespace) Then
						Select Case tokens(tokenIndex).Key
							Case "LineTerminatorToken", "OpenParenthesisPatternGroup"
								Exit Select
							Case Else
								collapsedText = tokens.Document.GetTokenText(tokens(tokenIndex))
								Exit While
						End Select
					End If
					tokenIndex = tokenIndex + 1
				End While

				Select Case tokenKey
					Case "FunctionReservedWordToken"
						node.CollapsedText = String.Format("Function {0}()", collapsedText.Trim())
					Case "PropertyReservedWordToken"
						node.CollapsedText = String.Format("Property {0}()", collapsedText.Trim())
					Case "SubReservedWordToken"
						node.CollapsedText = String.Format("Sub {0}()", collapsedText.Trim())
					Case "ClassReservedWordToken"
						node.CollapsedText = String.Format("Class {0}", collapsedText.Trim())
					Case "InterfaceReservedWordToken"
						node.CollapsedText = String.Format("Interface {0}", collapsedText.Trim())
					Case "EnumReservedWordToken"
						node.CollapsedText = String.Format("Enum {0}", collapsedText.Trim())
					Case "StructureReservedWordToken"
						node.CollapsedText = String.Format("Structure {0}", collapsedText.Trim())
					Case "ModuleReservedWordToken"
						node.CollapsedText = String.Format("Module {0}", collapsedText.Trim())
					Case "NamespaceReservedWordToken"
						node.CollapsedText = String.Format("Namespace {0}", collapsedText.Trim())
				End Select
			Case "RegionPreProcessorDirectiveStartToken"
				Dim collapsedText As String = String.Empty
				tokenIndex = tokenIndex + 1
				While tokenIndex < tokens.Count
					If tokens(tokenIndex).Key = "PreProcessorDirectiveEndToken" Then
						Exit While
					End If
					collapsedText += tokens.Document.GetTokenText(tokens(tokenIndex))
					tokenIndex = tokenIndex + 1
				End While
				collapsedText = collapsedText.Trim()
				If collapsedText.StartsWith("""") Then
					collapsedText = collapsedText.Substring(1)
				End If
				If collapsedText.EndsWith("""") Then
					collapsedText = collapsedText.Substring(0, collapsedText.Length - 1)
				End If
				If collapsedText.Length = 0 Then
					collapsedText = "..."
				End If
				node.CollapsedText = collapsedText.Trim()
		End Select
	End Sub	'SetOutliningNodeCollapsedText
End Class 'VBDotNetDynamicSyntaxLanguage
