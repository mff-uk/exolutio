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
        public static guiModelTreeCommand RenameComponentCommand { get; set; }

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
                                           NoScope = true,
                                           OpenDialog = true,
                                           Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil)
                                       };
            RenamePSMSchemaCommand = new guiControllerCommand
                                         {
                                             ControllerCommandFactoryMethod = CommandFactory<acmdRenamePSMSchema>.Factory,
                                             ScopeObjectConvertor =  guiControllerCommandScopeConvertors.DiagramToSchema,
                                             OpenDialog = true,
                                             Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil)
                                         };
            RemovePSMSchemaCommand = new guiControllerCommand
            {
                ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PSM.cmdDeletePSMSchema>.Factory,
                ScopeObjectConvertor = guiControllerCommandScopeConvertors.DiagramToSchema,
                OpenDialog = false,
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete2)
            };

            RenameComponentCommand = new guiModelTreeCommand
            {
                Text = "Rename...",
                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.pencil)
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
                                         NoScope = true,
                                         PIMOnly = true,
                                         Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.@class)
                                     };

            AddPIMAttributeCommand = new guiControllerCommand
                                         {
                                             Text = "Add attribute",
                                             ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMAttribute>.Factory,
                                             AcceptedSelectedComponentType = typeof(PIMClass),
                                             ScopeIsSelectedComponent = true,
                                             PIMOnly = true,
                                             Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes)
                                         };

            AddPIMAssociationCommand = new guiControllerCommand
                                           {
                                               Text = "Add association",
                                               ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMAssociation>.Factory,
                                               NoScope = true,
                                               PIMOnly = true,
                                               OpenDialog = true,
                                               Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.assocclass)
                                           };

            DerivePSMRootCommand = new guiControllerCommand
                                        {
                                            Text = "Derive new PSM root",
                                            ControllerCommandFactoryMethod = CommandFactory<Exolutio.Controller.Commands.Complex.PIM.cmdDerivePSMRoot>.Factory,
                                            PIMOnly = true,
                                            ScopeIsSelectedComponent = true,
                                            AcceptedSelectedComponentType = typeof(PIMClass),
                                            Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.XmlSchema)
                                        };
            #endregion

            #region PSM

            AddPSMSchemaCommand = new guiControllerCommand
                                      {
                                          ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMSchema>.Factory,
                                          NoScope = true
                                      };

            AddPSMClassCommand = new guiControllerCommand
                                     {
                                         Text = "Add class",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMClass>.Factory,
                                         NoScope = true,
                                         PSMOnly = true,
                                         Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.@class)
                                     };

            AddPSMContentModel = new guiControllerCommand
                                     {
                                         Text = "Add content model",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMContentModel>.Factory,
                                         NoScope = true,
                                         PSMOnly = true,
                                         Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.ContentChoice)
                                     };


            AddPSMAttributeCommand = new guiControllerCommand
                                         {
                                             Text = "Add attribute",
                                             ControllerCommandFactoryMethod = CommandFactory<cmdCreateNewPSMAttribute>.Factory,
                                             AcceptedSelectedComponentType = typeof(PSMClass),
                                             ScopeIsSelectedComponent = true,
                                             PSMOnly = true,
                                             Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes)
                                         };

            AddPSMAssociationCommand = new guiControllerCommand
                                           {
                                               Text = "Add association",
                                               ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMAssociation>.Factory,
                                               NoScope = true,
                                               PSMOnly = true,
                                               OpenDialog = true,
                                               Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddChildren),
                                           };

            AddPSMChildInterpreted = new guiControllerCommand
                                            {
                                                Text = "Add interpreted child",
                                                ControllerCommandFactoryMethod = CommandFactory<Exolutio.Controller.Commands.Complex.PSM.cmdCreateNewPSMClassAsIntChild>.Factory,
                                                PSMOnly = true,
                                                OpenDialog = true,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddChildren),
                                                AcceptedSelectedComponentType = typeof(PSMClass),
                                                ScopeIsSelectedComponent = true
                                            };
            AddPSMChildUnInterpreted = new guiControllerCommand
                                            {
                                                Text = "Add uninterpreted child",
                                                ControllerCommandFactoryMethod = CommandFactory<Exolutio.Controller.Commands.Complex.PSM.cmdCreateNewPSMClassAsUnintChild>.Factory,
                                                PSMOnly = true,
                                                OpenDialog = false,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddChildren),
                                                AcceptedSelectedComponentType = typeof(PSMClass),
                                                ScopeIsSelectedComponent = true
                                            };
            
            SplitPSMAttributeCommand = new guiControllerCommand
                                            {
                                                Text = "Split attribute",
                                                ControllerCommandFactoryMethod = CommandFactory<Exolutio.Controller.Commands.Complex.PSM.cmdSplitPSMAttribute>.Factory,
                                                PSMOnly = true,
                                                OpenDialog = false,
                                                Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.AddAttributes),
                                                ScopeIsSelectedComponent = true,
                                                AcceptedSelectedComponentType = typeof(PSMAttribute)
            };

            #endregion

            #region other

            VerifyModelCommand = new guiVerifyModelCommand();
            LocateInterpretedComponent = new guiLocateInterpretedComponent();

            #endregion
        }
    }
}