using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM.MacroWrappers;
using EvoX.Controller.Commands.Atomic.PSM.MacroWrappers;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.ResourceLibrary;
using EvoX.View.Commands.Edit;
using EvoX.View.Commands.Grammar;
using EvoX.View.Commands.Project;
using EvoX.View;
using EvoX.View.Commands.Versioning;

namespace EvoX.View.Commands
{
    public static class EvoXGuiCommands
    {
        #region project, application, undo, redo

        public static guiNewProjectCommand NewProjectCommand { get; set; }
        public static guiOpenProjectCommand OpenProjectCommand { get; set; }
#if SILVERLIGHT
        public static guiOpenWebProjectCommand OpenWebProjectCommand { get; set; }
#endif
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

        #endregion

        #region PSM

        public static guiControllerCommand AddPSMSchemaCommand { get; set; }
        public static guiControllerCommand RenamePSMSchemaCommand { get; set; }
        public static guiControllerCommand RemovePSMSchemaCommand { get; set; }
        public static guiControllerCommand AddPSMClassCommand { get; set; }
        public static guiControllerCommand AddPSMAttributeCommand { get; set; }
        public static guiControllerCommand AddPSMAssociationCommand { get; set; }
        public static guiControllerCommand AddPSMContentModel { get; set; }

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
                                           Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.pencil)
                                       };
            RenamePSMSchemaCommand = new guiControllerCommand
                                         {
                                             ControllerCommandFactoryMethod = CommandFactory<acmdRenamePSMSchema>.Factory,
                                             ScopeObjectConvertor =  guiControllerCommandScopeConvertors.DiagramToSchema,
                                             OpenDialog = true,
                                             Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.pencil)
                                         };
            RemovePSMSchemaCommand = new guiControllerCommand
            {
                ControllerCommandFactoryMethod = CommandFactory<Controller.Commands.Complex.PSM.cmdDeletePSMSchema>.Factory,
                ScopeObjectConvertor = guiControllerCommandScopeConvertors.DiagramToSchema,
                OpenDialog = false,
                Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.delete2)
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

            #endregion

            #region PIM

            AddPIMClassCommand = new guiControllerCommand
                                     {
                                         Text = "Add class",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdCreateNewPIMClass>.Factory,
                                         NoScope = true,
                                         PIMOnly = true,
                                         Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.@class)
                                     };

            AddPIMAttributeCommand = new guiControllerCommand
                                         {
                                             Text = "Add attribute",
                                             ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMAttribute>.Factory,
                                             AcceptedSelectedComponentType = typeof(PIMClass),
                                             ScopeIsSelectedComponent = true,
                                             PIMOnly = true,
                                             Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.AddAttributes)
                                         };

            AddPIMAssociationCommand = new guiControllerCommand
                                           {
                                               Text = "Add association",
                                               ControllerCommandFactoryMethod = CommandFactory<cmdNewPIMAssociation>.Factory,
                                               NoScope = true,
                                               PIMOnly = true,
                                               OpenDialog = true,
                                               Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.assocclass)
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
                                         Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.@class)
                                     };

            AddPSMContentModel = new guiControllerCommand
                                     {
                                         Text = "Add content model",
                                         ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMContentModel>.Factory,
                                         NoScope = true,
                                         PSMOnly = true,
                                         Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.ContentContainer)
                                     };


            AddPSMAttributeCommand = new guiControllerCommand
                                         {
                                             Text = "Add attribute",
                                             ControllerCommandFactoryMethod = CommandFactory<cmdCreateNewPSMAttribute>.Factory,
                                             AcceptedSelectedComponentType = typeof(PSMClass),
                                             ScopeIsSelectedComponent = true,
                                             PSMOnly = true,
                                             Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.AddAttributes)
                                         };

            AddPSMAssociationCommand = new guiControllerCommand
                                           {
                                               Text = "Add association",
                                               ControllerCommandFactoryMethod = CommandFactory<cmdNewPSMAssociation>.Factory,
                                               NoScope = true,
                                               PSMOnly = true,
                                               OpenDialog = true,
                                               Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.AddChildren)
                                           };



            #endregion

            #region other

            VerifyModelCommand = new guiVerifyModelCommand();
            LocateInterpretedComponent = new guiLocateInterpretedComponent();

            #endregion

            Current.SelectionChanged += Current_SelectionChanged;
        }

        private static void Current_SelectionChanged()
        {
            AddPIMAttributeCommand.OnCanExecuteChanged(null);
        }
    }
}