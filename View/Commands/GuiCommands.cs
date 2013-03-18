using System.Collections.Generic;
using Exolutio.Controller.Commands.Atomic.MacroWrappers;
using Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers;
using Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.View.Commands.Edit;
using Exolutio.View.Commands.Grammar;
using Exolutio.View.Commands.OCL;
using Exolutio.View.Commands.Project;
using Exolutio.View.Commands.Versioning;
using Exolutio.View.Commands.PSM;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.View.Commands.PIM;
using Exolutio.View.Commands.View;
using Exolutio.View.Commands.XRX;

namespace Exolutio.View.Commands
{
    public static class GuiCommands
    {
        #region project, application, undo, redo

        public static guiNewProjectCommand NewProjectCommand { get; set; }
        public static guiOpenProjectCommand OpenProjectCommand { get; set; }
        public static guiCloseProjectCommand CloseProjectCommand { get; set; }
        public static guiSaveProjectCommand SaveProjectCommand { get; set; }
        public static guiSaveAsProjectCommand SaveAsProjectCommand { get; set; }
        public static guiCloseAppCommand CloseApplicationCommand { get; set; }
        public static guiControllerCommand RenameProjectCommand { get; set; }
        public static guiUndoCommand UndoCommand { get; set; }
        public static guiRedoCommand RedoCommand { get; set; }
        public static guiDelete Delete { get; set; }
        public static guiShowHelpCommand HelpCommand { get; set; }
        public static guiSampleDocumentCommand CreateSampleDocumentCommand { get; set; }
        public static guiLocateInterpretedComponent LocateInterpretedComponent { get; set; }
        public static guiControllerCommand RenameComponentCommand { get; set; }
        //public static guiControllerCommand NewPIMSchema { get; set; }
        public static guiControllerCommand NewPSMSchema { get; set; }
        public static guiOpenAttributeTypesDialogCommand OpenAttributeTypesDialog { get; set; }
        public static guiXSDtoPSM XSDtoPSM { get; set; }

#if SILVERLIGHT
        public static guiOpenWebProjectCommand OpenWebProjectCommand { get; set; }
#else 
        public static guiFindChanges FindChangesCommand { get; set; }
#endif

        #endregion

        #region translation

        public static guiNormalizeSchemaCommand NormalizeSchemaCommandCommand { get; set; }
        public static guiTestNormalizationCommand TestNormalizationCommand { get; set; }
        public static guiGenerateGrammarCommand GenerateGrammarCommand { get; set; }
        public static guiGenerateXsdCommand GenerateXsdCommand { get; set; }
        public static guiGenerateRngCommand GenerateRngCommand { get; set; }
        public static guiGenerateSchematronSchemaCommand GenerateSchematronSchemaCommand { get; set; }

        #endregion

        #region PIM

        public static guiControllerCommand AddPIMClassCommand { get; set; }
        public static guiControllerCommand AddPIMAttributeCommand { get; set; }
        public static guiControllerCommand AddPIMAssociationCommand { get; set; }
        public static guiControllerCommand AddPIMGeneralizationCommand { get; set; }
        public static guiControllerCommand DerivePSMRootCommand { get; set; }
        public static guiSplitPIMAttribute SplitPIMAttributeCommand { get; set; }
        public static guiSplitPIMAssociation SplitPIMAssociationCommand { get; set; }
        public static guiSplitPIMAttribute SplitPIMAttribute3Command { get; set; }
        public static guiSplitPIMAssociation SplitPIMAssociation3Command { get; set; }
        public static guiSplitPIMAttribute SplitPIMAttribute4Command { get; set; }
        public static guiSplitPIMAssociation SplitPIMAssociation4Command { get; set; }
        public static guiControllerCommand SplitPIMAttributeMoreCommand { get; set; }
        public static guiControllerCommand SplitPIMAssociationMoreCommand { get; set; }
        public static guiShiftPIMAttributeCommand PIMShiftUp { get; set; }
        public static guiShiftPIMAttributeCommand PIMShiftDown { get; set; }
        public static guiAssociate2 Associate2 { get; set; }
        public static guiPIMDelete PIMDelete { get; set; }
        public static guiPIMDeleteClass PIMDeleteClass { get; set; }
        public static guiPIMDeleteAttribute PIMDeleteAttribute { get; set; }
        public static guiPIMDeleteAssociation PIMDeleteAssociation { get; set; }
        public static guiPIMDeleteGeneralization PIMDeleteGeneralization { get; set; }
        #endregion

        #region PSM

        public static guiValidateCommand ValidateXMLFile { get; set; }
        public static guiValidateAllCommand ValidateAllXMLFiles { get; set; }
        public static guiControllerCommand AddPSMSchemaCommand { get; set; }
        public static guiMappingV MappingV { get; set; }
        public static guiControllerCommand RenamePSMSchemaCommand { get; set; }
        public static guiControllerCommand RemovePSMSchemaCommand { get; set; }
        public static guiControllerCommand AddPSMClassCommand { get; set; }
        public static guiControllerCommand AddPSMAttributeCommand { get; set; }
        public static guiControllerCommand AddPSMAssociationCommand { get; set; }
        public static guiControllerCommand AddPSMGeneralizationCommand { get; set; }
        public static guiControllerCommand AddPSMContentModel { get; set; }
        public static guiSplitPSMAttribute SplitPSMAttributeCommand { get; set; }
        public static guiSplitPSMAssociation SplitPSMAssociationCommand { get; set; }
        public static guiSplitPSMAttribute SplitPSMAttribute3Command { get; set; }
        public static guiSplitPSMAssociation SplitPSMAssociation3Command { get; set; }
        public static guiSplitPSMAttribute SplitPSMAttribute4Command { get; set; }
        public static guiSplitPSMAssociation SplitPSMAssociation4Command { get; set; }
        public static guiControllerCommand SplitPSMAttributeMoreCommand { get; set; }
        public static guiControllerCommand SplitPSMAssociationMoreCommand { get; set; }
        public static guiControllerCommand AddPSMChildInterpreted { get; set; }
        public static guiControllerCommand AddPSMChildUnInterpreted { get; set; }
        public static guiCreateContentModelCommand CreateSequenceContentModelCommand { get; set; }
        public static guiCreateContentModelCommand CreateChoiceContentModelCommand { get; set; }
        public static guiCreateContentModelCommand CreateSetContentModelCommand { get; set; }
        public static guiShiftCommand ShiftLeft { get; set; }
        public static guiShiftCommand ShiftRight { get; set; }
        public static guiShiftPSMAttributeCommand PSMShiftUp { get; set; }
        public static guiShiftPSMAttributeCommand PSMShiftDown { get; set; }
        public static guiLeaveOutUnintAM LeaveOutUnintAM { get; set; }
        public static guiInsertPSMClass InsertPSMClass { get; set; }

        //public static guiCutAssociation CutAssociation { get; set; }
        //public static guiDeletePSMAttribute DeletePSMAttribute { get; set; }
        public static guiPSMDelete PSMDelete { get; set; }
        public static guiDeleteSubtree DeleteSubtree { get; set; }
        public static guiDeletePSMSchema DeletePSMSchema { get; set; }
        #endregion

        #region versioning

        public static guiBranchCurrentVersion BranchCurrentVersionCommand { get; set; }
        public static guiSaveAsSingleVersion SaveAsSingleVersionCommand { get; set; }
        public static guiRemoveCurrentVersion RemoveCurrentVersionCommand { get; set; }
        public static guiVerifyModelCommand VerifyModelCommand { get; set; }
        public static guiCreateVersionLinkBetweenSchemas CreateVersionLinkBetweenSchemas { get; set; }
        public static guiRemoveVersionLinkBetweenSchemas RemoveVersionLinkBetweenSchemas { get; set; }

        #if SILVERLIGHT
        #else
        public static guiCreateVersionLink CreateVersionLinkCommand { get; set; }
        public static guiRemoveVersionLink RemoveVersionLinkCommand { get; set; }
        public static guiAdaptationCommand AdaptationCommand { get; set; }
        public static guiVisualizeLinks VisualizeLinksCommand { get; set; }
        #endif

        #endregion

        #region align

        public static guiAlignCommand AlignLeftCommand { get; set; }
        public static guiAlignCommand AlignRightCommand { get; set; }
        public static guiAlignCommand AlignTopCommand { get; set; }
        public static guiAlignCommand AlignBottomCommand { get; set; }

        public static guiAlignCommand CenterVerticallyCommand { get; set; }
        public static guiAlignCommand CenterHorizontallyCommand { get; set; }
        public static guiAlignCommand DistributeVerticallyCommand { get; set; }
        public static guiAlignCommand DistributeHorizontallyCommand { get; set; }

        #endregion

        #region traversing

        public static guiSelectNextCommand SelectNextCommand { get; set; }
        public static guiSelectNextCommand SelectPrevCommand { get; set; }
        
        #endregion

        #region OCL

        public static guiCreateOCLScriptCommand CreateOCLValidationScriptCommand { get; set; }
        public static guiCreateOCLScriptCommand CreateOCLEvolutionScriptCommand { get; set; }
        public static guiOCLSyntaxCheckCommand OCLSyntaxCheckCommand { get; set; }
        public static guiRemoveOCLScriptCommand RemoveOCLScriptCommand { get; set; }
        public static guiRenameOCLScriptCommand RenameOCLScriptCommand { get; set; }
        public static guiSuggestConstraintsCommand SuggestConstraintsCommand { get; set; }

        #endregion

		#region XRX

		public static guiCreateXRXPSMSchemaCommand CreateXRXPSMSchemaCommand { get; set; }
		public static guiGenerateXRXRestInterfaceCommand GenerateXRXRestInterfaceCommand { get; set; }
		public static guiGenerateXRXXFormsCommand GenerateXRXXFormsCommand { get; set; }

		#endregion 

		public static void Init(IMainWindow mainWindow)
        {
            #region project
            NewProjectCommand = new guiNewProjectCommand();
            OpenProjectCommand = new guiOpenProjectCommand();
            CloseApplicationCommand = new guiCloseAppCommand();
            CloseProjectCommand = new guiCloseProjectCommand();
            SaveProjectCommand = new guiSaveProjectCommand();
            SaveAsProjectCommand = new guiSaveAsProjectCommand();
            XSDtoPSM = new guiXSDtoPSM();
#if SILVERLIGHT
            OpenWebProjectCommand = new guiOpenWebProjectCommand();
#endif
            UndoCommand = new guiUndoCommand();
            RedoCommand = new guiRedoCommand();
            Delete = new guiDelete();
            HelpCommand = new guiShowHelpCommand();
            //RenameProjectCommand = new guiControllerCommand
            //                           {
            //                               ControllerCommandFactoryMethod = CommandFactory<cmdRenameProject>.Factory,
            //                               ControllerCommandType = typeof(cmdRenameProject),
            //                               NoScope = true,
            //                               OpenDialog = true,
            //                               Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil)
            //                           };
            RenamePSMSchemaCommand = new guiControllerCommand
                                         {
                                             ControllerCommandFactoryMethod = CommandFactory<cmdRenamePSMSchema>.Factory,
                                             ControllerCommandType = typeof(cmdRenamePSMSchema),
                                             ScopeObjectConvertor =  guiControllerCommandScopeConvertors.DiagramToSchema,
                                             OpenDialog = true,
                                             Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil)
                                         };
            RemovePSMSchemaCommand = new guiControllerCommand
            {
                ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PSM.cmdDeletePSMSchema>.Factory,
                ControllerCommandType = typeof(Controller.Commands.Complex.PSM.cmdDeletePSMSchema),
                ScopeObjectConvertor = guiControllerCommandScopeConvertors.DiagramToSchema,
                OpenDialog = false,
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete2)
            };

            RenameComponentCommand = new guiControllerCommand
            {
                Text = "Rename...",
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.note_edit),
                ControllerCommandFactoryMethod = CommandFactory<cmdRenameComponent>.Factory,
                ControllerCommandType = typeof(cmdRenameComponent),
                Gesture = KeyGestures.F2,
                OpenDialog = true,
                AcceptedSelectedComponentType = typeof(Component),
                ScopeIsSelectedComponent = true
            };

            NewPSMSchema = new guiControllerCommand
            {
                Text = "New PSM schema",
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.GenericDocument),
                NoScope = true,
                ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMSchema>.Factory,
                ControllerCommandType = typeof(cmdNewPSMSchema)
            };

            OpenAttributeTypesDialog = new guiOpenAttributeTypesDialogCommand();

            #endregion

            #region grammar

            NormalizeSchemaCommandCommand = new guiNormalizeSchemaCommand();
            TestNormalizationCommand = new guiTestNormalizationCommand();
            GenerateGrammarCommand = new guiGenerateGrammarCommand();
            GenerateXsdCommand = new guiGenerateXsdCommand();
            GenerateSchematronSchemaCommand = new guiGenerateSchematronSchemaCommand();
            CreateSampleDocumentCommand = new guiSampleDocumentCommand();
            SuggestConstraintsCommand = new guiSuggestConstraintsCommand();
            GenerateRngCommand = new guiGenerateRngCommand();

            #endregion

            #region versioning

            BranchCurrentVersionCommand = new guiBranchCurrentVersion();
            SaveAsSingleVersionCommand = new guiSaveAsSingleVersion();
            RemoveCurrentVersionCommand = new guiRemoveCurrentVersion();
            #if SILVERLIGHT
            #else
            FindChangesCommand = new guiFindChanges();
            CreateVersionLinkCommand = new guiCreateVersionLink();
            RemoveVersionLinkCommand = new guiRemoveVersionLink();
            AdaptationCommand = new guiAdaptationCommand();
            VisualizeLinksCommand = new guiVisualizeLinks();
            RemoveVersionLinkBetweenSchemas = new guiRemoveVersionLinkBetweenSchemas();
            CreateVersionLinkBetweenSchemas = new guiCreateVersionLinkBetweenSchemas();
            #endif

            #endregion

            #region PIM

            AddPIMClassCommand = new guiControllerCommand
                                     {
                                         Text = "Add class",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdCreateNewPIMClass>.Factory,
                                         ControllerCommandType = typeof(cmdCreateNewPIMClass),
                                         NoScope = true,
                                         PIMOnly = true,
                                         Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.@class)
                                     };

            AddPIMAttributeCommand = new guiControllerCommand
                                         {
                                             Text = "Add attribute",
                                             ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMAttribute>.Factory,
                                             ControllerCommandType = typeof(cmdNewPIMAttribute),
                                             AcceptedSelectedComponentType = typeof(PIMClass),
                                             ScopeIsSelectedComponent = true,
                                             PIMOnly = true,
                                             Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes)
                                         };

            AddPIMAssociationCommand = new guiControllerCommand
                                           {
                                               Text = "Add association",
                                               ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMAssociation>.Factory,
                                               ControllerCommandType = typeof(cmdNewPIMAssociation),
                                               NoScope = true,
                                               PIMOnly = true,
                                               OpenDialog = true,
                                               Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.assocclass)
                                           };
            AddPIMGeneralizationCommand = new guiControllerCommand
            {
                Text = "Add generalization",
                ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMGeneralization>.Factory,
                ControllerCommandType = typeof(cmdNewPIMGeneralization),
                NoScope = true,
                PIMOnly = true,
                OpenDialog = true,
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.gen)
            };
            SplitPIMAttributeCommand = new guiSplitPIMAttribute();
            SplitPIMAssociationCommand = new guiSplitPIMAssociation();
            SplitPIMAttribute3Command = new guiSplitPIMAttribute() { Count = 3 };
            SplitPIMAssociation3Command = new guiSplitPIMAssociation() { Count = 3 };
            SplitPIMAttribute4Command = new guiSplitPIMAttribute() { Count = 4 };
            SplitPIMAssociation4Command = new guiSplitPIMAssociation() { Count = 4 };
            SplitPIMAttributeMoreCommand = new guiControllerCommand
            {
                Text = "More...",
                ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PIM.cmdSplitPIMAttribute>.Factory,
                ControllerCommandType = typeof(Controller.Commands.Complex.PIM.cmdSplitPIMAttribute),
                PIMOnly = true,
                OpenDialog = true,
                ScopeIsSelectedComponent = true,
                AcceptedSelectedComponentType = typeof(PIMAttribute),
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes)
            };
            SplitPIMAssociationMoreCommand = new guiControllerCommand
            {
                Text = "More...",
                ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PIM.cmdSplitPIMAssociation>.Factory,
                ControllerCommandType = typeof(Controller.Commands.Complex.PIM.cmdSplitPIMAssociation),
                PIMOnly = true,
                OpenDialog = true,
                ScopeIsSelectedComponent = true,
                AcceptedSelectedComponentType = typeof(PIMAssociation),
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.split_pim_assoc)
            };
            DerivePSMRootCommand = new guiControllerCommand
                                        {
                                            Text = "Derive new PSM root",
                                            ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PIM.cmdDerivePSMRoot>.Factory,
                                            ControllerCommandType = typeof(Controller.Commands.Complex.PIM.cmdDerivePSMRoot),
                                            PIMOnly = true,
                                            ScopeIsSelectedComponent = true,
                                            AcceptedSelectedComponentType = typeof(PIMClass),
                                            Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.XmlSchema)
                                        };
            PIMShiftUp = new guiShiftPIMAttributeCommand() { Up = true };
            PIMShiftDown = new guiShiftPIMAttributeCommand() { Up = false };
            Associate2 = new guiAssociate2();
            PIMDelete = new guiPIMDelete();
            PIMDeleteAssociation = new guiPIMDeleteAssociation();
            PIMDeleteAttribute = new guiPIMDeleteAttribute();
            PIMDeleteClass = new guiPIMDeleteClass();
            PIMDeleteGeneralization = new guiPIMDeleteGeneralization();
            #endregion

            #region PSM

            AddPSMSchemaCommand = new guiControllerCommand
                                      {
                                          ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMSchema>.Factory,
                                          ControllerCommandType = typeof(cmdNewPSMSchema),
                                          NoScope = true
                                      };

            AddPSMClassCommand = new guiControllerCommand
                                     {
                                         Text = "Add class",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMClass>.Factory,
                                         ControllerCommandType = typeof(cmdNewPSMClass),
                                         NoScope = true,
                                         PSMOnly = true,
                                         Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.@class)
                                     };

            AddPSMContentModel = new guiControllerCommand
                                     {
                                         Text = "Add content model",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMContentModel>.Factory,
                                         ControllerCommandType = typeof(cmdNewPSMContentModel),
                                         NoScope = true,
                                         PSMOnly = true,
                                         Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.ContentChoice)
                                     };


            AddPSMAttributeCommand = new guiControllerCommand
                                         {
                                             Text = "Add attribute",
                                             ControllerCommandFactoryMethod = CommandFactory<Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers.cmdCreateNewPSMAttribute>.Factory,
                                             ControllerCommandType = typeof(Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers.cmdCreateNewPSMAttribute),
                                             AcceptedSelectedComponentType = typeof(PSMClass),
                                             ScopeIsSelectedComponent = true,
                                             PSMOnly = true,
                                             Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes)
                                         };

            AddPSMAssociationCommand = new guiControllerCommand
                                           {
                                               Text = "Add association",
                                               ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMAssociation>.Factory,
                                               ControllerCommandType = typeof(cmdNewPSMAssociation),
                                               NoScope = true,
                                               PSMOnly = true,
                                               OpenDialog = true,
                                               Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddChildren),
                                           };

            AddPSMGeneralizationCommand = new guiControllerCommand
                                            {
                                                Text = "Add generalization",
                                                ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMGeneralization>.Factory,
                                                ControllerCommandType = typeof(cmdNewPSMGeneralization),
                                                NoScope = true,
                                                PSMOnly = true,
                                                OpenDialog = true,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.gen),
                                            };

            AddPSMChildInterpreted = new guiControllerCommand
                                            {
                                                Text = "Add interpreted child",
                                                ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PSM.cmdCreateNewPSMClassAsIntChild>.Factory,
                                                ControllerCommandType = typeof(Controller.Commands.Complex.PSM.cmdCreateNewPSMClassAsIntChild),
                                                PSMOnly = true,
                                                OpenDialog = true,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddChildren),
                                                AcceptedSelectedComponentType = typeof(PSMClass),
                                                ScopeIsSelectedComponent = true
                                            };
            AddPSMChildUnInterpreted = new guiControllerCommand
                                            {
                                                Text = "Add uninterpreted child",
                                                ControllerCommandFactoryMethod = CommandFactory<cmdCreateNewPSMClassAsUnintChild>.Factory,
                                                ControllerCommandType = typeof(Exolutio.Controller.Commands.Complex.PSM.cmdCreateNewPSMClassAsUnintChild),
                                                PSMOnly = true,
                                                OpenDialog = false,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddChildren),
                                                AcceptedSelectedComponentType = typeof(PSMClass),
                                                ScopeIsSelectedComponent = true
                                            };

            ValidateXMLFile = new guiValidateCommand();
            ValidateAllXMLFiles = new guiValidateAllCommand();

            SplitPSMAttributeCommand = new guiSplitPSMAttribute();
            SplitPSMAssociationCommand = new guiSplitPSMAssociation();
            SplitPSMAttribute3Command = new guiSplitPSMAttribute() { Count = 3 };
            SplitPSMAssociation3Command = new guiSplitPSMAssociation() { Count = 3 };
            SplitPSMAttribute4Command = new guiSplitPSMAttribute() { Count = 4 };
            SplitPSMAssociation4Command = new guiSplitPSMAssociation() { Count = 4 };
            SplitPSMAttributeMoreCommand = new guiControllerCommand
            {
                Text = "More...",
                ControllerCommandFactoryMethod = CommandFactory<cmdSplitPSMAttribute>.Factory,
                ControllerCommandType = typeof(cmdSplitPSMAttribute),
                PSMOnly = true,
                OpenDialog = true,
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes),
                ScopeIsSelectedComponent = true,
                AcceptedSelectedComponentType = typeof(PSMAttribute)
            };
            SplitPSMAssociationMoreCommand = new guiControllerCommand
            {
                Text = "More...",
                ControllerCommandFactoryMethod = CommandFactory<cmdSplitPSMAttribute>.Factory,
                ControllerCommandType = typeof(cmdSplitPSMAttribute),
                PSMOnly = true,
                OpenDialog = true,
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.split_psm_assoc),
                ScopeIsSelectedComponent = true,
                AcceptedSelectedComponentType = typeof(PSMAssociation)
            };
            CreateSequenceContentModelCommand = new guiCreateContentModelCommand() { Type = PSMContentModelType.Sequence };
            CreateChoiceContentModelCommand = new guiCreateContentModelCommand() { Type = PSMContentModelType.Choice };
            CreateSetContentModelCommand = new guiCreateContentModelCommand() { Type = PSMContentModelType.Set };
            ShiftLeft = new guiShiftCommand() { Left = true };
            ShiftRight = new guiShiftCommand() { Left = false };
            PSMShiftUp = new guiShiftPSMAttributeCommand() { Up = true };
            PSMShiftDown = new guiShiftPSMAttributeCommand() { Up = false };
            LeaveOutUnintAM = new guiLeaveOutUnintAM();
            InsertPSMClass = new guiInsertPSMClass();
            //CutAssociation = new guiCutAssociation();
            //DeletePSMAttribute = new guiDeletePSMAttribute();
            PSMDelete = new guiPSMDelete();
            DeleteSubtree = new guiDeleteSubtree();
            DeletePSMSchema = new guiDeletePSMSchema();

            MappingV = new guiMappingV();

            #endregion

            #region other

            VerifyModelCommand = new guiVerifyModelCommand();
            LocateInterpretedComponent = new guiLocateInterpretedComponent();

            #endregion

            #region align

            List<guiAlignCommand> alignCommands = new List<guiAlignCommand>();

            AlignLeftCommand = new guiAlignCommand { Alignment = EAlignment.Left, AllAlignCommands = alignCommands };
            AlignRightCommand = new guiAlignCommand { Alignment = EAlignment.Right, AllAlignCommands = alignCommands };
            AlignTopCommand = new guiAlignCommand { Alignment = EAlignment.Top, AllAlignCommands = alignCommands };
            AlignBottomCommand = new guiAlignCommand { Alignment = EAlignment.Bottom, AllAlignCommands = alignCommands };

            CenterVerticallyCommand = new guiAlignCommand { Alignment = EAlignment.CenterV, AllAlignCommands = alignCommands };
            CenterHorizontallyCommand = new guiAlignCommand { Alignment = EAlignment.CenterH, AllAlignCommands = alignCommands };
            DistributeVerticallyCommand = new guiAlignCommand { Alignment = EAlignment.DistributeV, AllAlignCommands = alignCommands };
            DistributeHorizontallyCommand = new guiAlignCommand { Alignment = EAlignment.DistributeH, AllAlignCommands = alignCommands };

            alignCommands.Add(AlignLeftCommand);
            alignCommands.Add(AlignRightCommand);
            alignCommands.Add(AlignTopCommand);
            alignCommands.Add(AlignBottomCommand);
            alignCommands.Add(CenterVerticallyCommand);
            alignCommands.Add(CenterHorizontallyCommand);
            alignCommands.Add(DistributeVerticallyCommand);
            alignCommands.Add(DistributeHorizontallyCommand);

            #endregion

            #region traversing

            SelectNextCommand = new guiSelectNextCommand();
            SelectPrevCommand = new guiSelectNextCommand();

            #endregion 
        
            #region OCL

            CreateOCLValidationScriptCommand = new guiCreateOCLScriptCommand() { CreatedScriptType = OCLScript.EOclScriptType.Validation };
            CreateOCLEvolutionScriptCommand = new guiCreateOCLScriptCommand() { CreatedScriptType = OCLScript.EOclScriptType.Evolution };
            OCLSyntaxCheckCommand = new guiOCLSyntaxCheckCommand();
            RemoveOCLScriptCommand = new guiRemoveOCLScriptCommand();
            RenameOCLScriptCommand = new guiRenameOCLScriptCommand();
            SuggestConstraintsCommand = new guiSuggestConstraintsCommand();

            #endregion 

			#region XRX

			CreateXRXPSMSchemaCommand = new guiCreateXRXPSMSchemaCommand();
			GenerateXRXRestInterfaceCommand = new guiGenerateXRXRestInterfaceCommand();
			GenerateXRXXFormsCommand = new guiGenerateXRXXFormsCommand();

			#endregion 
		}
    }
}