using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers;
using Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.View.Commands.Edit;
using Exolutio.View.Commands.Grammar;
using Exolutio.View.Commands.Project;
using Exolutio.View;
using Exolutio.View.Commands.Versioning;
using Exolutio.View.Commands.PSM;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Model;
using Exolutio.View.Commands.PIM;

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
        public static guiShowHelpCommand HelpCommand { get; set; }
        public static guiSampleDocumentCommand CreateSampleDocumentCommand { get; set; }
        public static guiLocateInterpretedComponent LocateInterpretedComponent { get; set; }
        public static guiControllerCommand RenameComponentCommand { get; set; }

#if SILVERLIGHT
        public static guiOpenWebProjectCommand OpenWebProjectCommand { get; set; }
#else 
        public static guiFindChangesCommand FindChangesCommand { get; set; }
#endif

        #endregion

        #region translation

        public static guiNormalizeSchemaCommand NormalizeSchemaCommandCommand { get; set; }
        public static guiTestNormalizationCommand TestNormalizationCommand { get; set; }
        public static guiGenerateGrammarCommand GenerateGrammarCommand { get; set; }

        #endregion

        #region PIM

        public static guiControllerCommand AddPIMClassCommand { get; set; }
        public static guiControllerCommand AddPIMAttributeCommand { get; set; }
        public static guiControllerCommand AddPIMAssociationCommand { get; set; }
        public static guiControllerCommand DerivePSMRootCommand { get; set; }
        public static guiControllerCommand SplitPIMAttributeCommand { get; set; }
        public static guiControllerCommand SplitPIMAssociationCommand { get; set; }
        public static guiShiftPIMAttributeCommand PIMShiftUp { get; set; }
        public static guiShiftPIMAttributeCommand PIMShiftDown { get; set; }

        #endregion

        #region PSM

        public static guiControllerCommand AddPSMSchemaCommand { get; set; }
        public static guiControllerCommand RenamePSMSchemaCommand { get; set; }
        public static guiControllerCommand RemovePSMSchemaCommand { get; set; }
        public static guiControllerCommand AddPSMClassCommand { get; set; }
        public static guiControllerCommand AddPSMAttributeCommand { get; set; }
        public static guiControllerCommand AddPSMAssociationCommand { get; set; }
        public static guiControllerCommand AddPSMContentModel { get; set; }
        public static guiControllerCommand SplitPSMAttributeCommand { get; set; }
        public static guiControllerCommand AddPSMChildInterpreted { get; set; }
        public static guiControllerCommand AddPSMChildUnInterpreted { get; set; }
        public static guiCreateContentModelCommand CreateSequenceContentModelCommand { get; set; }
        public static guiCreateContentModelCommand CreateChoiceContentModelCommand { get; set; }
        public static guiCreateContentModelCommand CreateSetContentModelCommand { get; set; }
        public static guiShiftAssociationCommand ShiftLeft { get; set; }
        public static guiShiftAssociationCommand ShiftRight { get; set; }
        public static guiShiftPSMAttributeCommand PSMShiftUp { get; set; }
        public static guiShiftPSMAttributeCommand PSMShiftDown { get; set; }
        public static guiControllerCommand LeaveOutUnintAM { get; set; }
        #endregion

        #region versioning

        public static guiBranchCurrentVersionCommand BranchCurrentVersionCommand { get; set; }
        public static guiSaveAsSingleVersionCommand SaveAsSingleVersionCommand { get; set; }
        public static guiRemoveCurrentVersionCommand RemoveCurrentVersionCommand { get; set; }
        public static guiVerifyModelCommand VerifyModelCommand { get; set; }

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
#if SILVERLIGHT
            OpenWebProjectCommand = new guiOpenWebProjectCommand();
#endif
            UndoCommand = new guiUndoCommand();
            RedoCommand = new guiRedoCommand();
            HelpCommand = new guiShowHelpCommand();
            RenameProjectCommand = new guiControllerCommand
                                       {
                                           ControllerCommandFactoryMethod = CommandFactory<acmdRenameProject>.Factory,
                                           ControllerCommandType = typeof(acmdRenameProject),
                                           NoScope = true,
                                           OpenDialog = true,
                                           Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil)
                                       };
            RenamePSMSchemaCommand = new guiControllerCommand
                                         {
                                             ControllerCommandFactoryMethod = CommandFactory<acmdRenamePSMSchema>.Factory,
                                             ControllerCommandType = typeof(acmdRenamePSMSchema),
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
                ControllerCommandFactoryMethod = CommandFactory<acmdRenameComponent>.Factory,
                ControllerCommandType = typeof(acmdRenameComponent),
                Gesture = KeyGestures.F2,
                OpenDialog = true,
                AcceptedSelectedComponentType = typeof(Component),
                ScopeIsSelectedComponent = true
            };

            #endregion

            #region grammar

            NormalizeSchemaCommandCommand = new guiNormalizeSchemaCommand();
            TestNormalizationCommand = new guiTestNormalizationCommand();
            GenerateGrammarCommand = new guiGenerateGrammarCommand();
            CreateSampleDocumentCommand = new guiSampleDocumentCommand();

            #endregion

            #region versioning

            BranchCurrentVersionCommand = new guiBranchCurrentVersionCommand();
            SaveAsSingleVersionCommand = new guiSaveAsSingleVersionCommand();
            RemoveCurrentVersionCommand = new guiRemoveCurrentVersionCommand();
            #if SILVERLIGHT
            #else
            FindChangesCommand = new guiFindChangesCommand();
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
            SplitPIMAttributeCommand = new guiControllerCommand
                                        {
                                            Text = "Split attribute",
                                            ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PIM.cmdSplitPIMAttribute>.Factory,
                                            ControllerCommandType = typeof(Controller.Commands.Complex.PIM.cmdSplitPIMAttribute),
                                            PIMOnly = true,
                                            ScopeIsSelectedComponent = true,
                                            AcceptedSelectedComponentType = typeof(PIMAttribute)
                                        };
            SplitPIMAssociationCommand = new guiControllerCommand
                                        {
                                            Text = "Split association",
                                            ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PIM.cmdSplitPIMAssociation>.Factory,
                                            ControllerCommandType = typeof(Controller.Commands.Complex.PIM.cmdSplitPIMAssociation),
                                            PIMOnly = true,
                                            ScopeIsSelectedComponent = true,
                                            AcceptedSelectedComponentType = typeof(PIMAssociation)
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
            
            SplitPSMAttributeCommand = new guiControllerCommand
                                            {
                                                Text = "Split attribute",
                                                ControllerCommandFactoryMethod = CommandFactory<cmdSplitPSMAttribute>.Factory,
                                                ControllerCommandType = typeof(cmdSplitPSMAttribute),
                                                PSMOnly = true,
                                                OpenDialog = false,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes),
                                                ScopeIsSelectedComponent = true,
                                                AcceptedSelectedComponentType = typeof(PSMAttribute)
            };

            CreateSequenceContentModelCommand = new guiCreateContentModelCommand() { Type = PSMContentModelType.Sequence };
            CreateChoiceContentModelCommand = new guiCreateContentModelCommand() { Type = PSMContentModelType.Choice };
            CreateSetContentModelCommand = new guiCreateContentModelCommand() { Type = PSMContentModelType.Set };
            ShiftLeft = new guiShiftAssociationCommand() { Left = true };
            ShiftRight = new guiShiftAssociationCommand() { Left = false };
            PSMShiftUp = new guiShiftPSMAttributeCommand() { Up = true };
            PSMShiftDown = new guiShiftPSMAttributeCommand() { Up = false };
            LeaveOutUnintAM = new guiControllerCommand()
                                            {
                                                Text = "Leave out",
                                                ControllerCommandFactoryMethod = CommandFactory<cmdLeaveOutUninterpretedAssociationMember>.Factory,
                                                ControllerCommandType = typeof(cmdLeaveOutUninterpretedAssociationMember),
                                                PSMOnly = true,
                                                OpenDialog = false,
                                                ScopeIsSelectedComponent = true,
                                                AcceptedSelectedComponentType = typeof(PSMAssociationMember),
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.RemoveContainer)
                                            };
            #endregion

            #region other

            VerifyModelCommand = new guiVerifyModelCommand();
            LocateInterpretedComponent = new guiLocateInterpretedComponent();

            #endregion
        }
    }
}