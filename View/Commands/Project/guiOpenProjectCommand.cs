using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.Serialization;
using Microsoft.Win32;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Project
{
    public class guiOpenProjectCommand : guiProjectCommand
    {
        #region Overrides of ExolutioGuiCommandBase

        public override KeyGesture Gesture
        {
            get { return KeyGestures.ControlO; }
        }

        public override string Text
        {
            get { return CommandsResources.guiOpenProjectCommand_Text_Open_project; }
        }

        public override string ScreenTipText
        {
            get { return CommandsResources.guiOpenProjectCommand_ScreenTipText_Opens_new_Exolutio_project; }
        }

        public override ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.folder); }
        }

        public string InitialDirectory { get; set; }

        #endregion

        public void Execute(string fileName = null, bool noDialogs = false, bool noOpenFileDialog = false)
        {
#if SILVERLIGHT
            OpenFileDialog dlg = new OpenFileDialog
                                     {
                                         Filter = CommandsResources.guiOpenProjectCommand_Execute_Exolutio_files____Exolutio____Exolutio_XML_files____xml____xml_All_files____________,
                                         
                                     };
#else
            OpenFileDialog dlg = new OpenFileDialog
										{
											DefaultExt = CommandsResources.guiOpenProjectCommand_Execute_Exolutio_Default_Extension,
											Filter = CommandsResources.guiOpenProjectCommand_Execute_Exolutio_files____Exolutio____Exolutio_XML_files____xml____xml_All_files____________,
                                            CheckFileExists = true,
                                            CheckPathExists = true
										};

            if (!string.IsNullOrEmpty(InitialDirectory))
            {
                dlg.InitialDirectory = InitialDirectory;
            }
#endif

            bool? result = null;

            FileInfo selectedFile = null;
            if (!noDialogs && !noOpenFileDialog)
			{
				result = dlg.ShowDialog();
                if (result == true)
                {
                    #if SILVERLIGHT
			        selectedFile = dlg.File;
                    #else
                    selectedFile = new FileInfo(dlg.FileName);
                    #endif
                }
			}
			else
            {
#if SILVERLIGHT
                selectedFile = new FileInfo(fileName);
#else
                selectedFile = new FileInfo(fileName);
#endif
            }

            

            if (noDialogs || noOpenFileDialog || result == true)
			{
                //String msg = string.Empty;

                //XmlDeserializatorBase deserializator;
                //if (XmlDeserializatorVersions.UsesVersions(dlg.FileName))
                //{
                //    deserializator = new XmlDeserializatorVersions();
                //}
                //else
                //{
                //    deserializator = new XmlDeserializator();
                //}

                //// First, validates if the file is a valid Exolutio XML file
                //// TODO: it would be better to have two separate schemas rather than one choice schema 
                //if (!deserializator.ValidateXML(dlg.FileName, ref msg))
                //{
                //    Dialogs.ErrMsgBox.Show("File cannot be opened", "Not a valid eXolutio XML file");
                //    return;
                //}

                //// oldVersion check
                //string v1;
                //string v2;
                //if (!XmlDeserializatorBase.VersionsEqual(dlg.FileName, out v1, out v2))
                //{
                //    fProjectConverter projectConverter = new fProjectConverter();
			        
                //    if (projectConverter.CanConvert(v1, v2))
                //    {
                //        MessageBoxResult yn = ExolutioYesNoBox.Show("Project is obsolete. ", "Project is obsolete and must be converted to a new oldVersion before opening. \r\nDo you want to convert it now? ");
                //        if (yn == MessageBoxResult.Yes)
                //        {
                //            projectConverter.SetFile(dlg.FileName);
                //            projectConverter.DialogMode = true;
                //            projectConverter.ShowDialog();
                //        }
                //    }
                //    else
                //    {
                //        Dialogs.ErrMsgBox.Show(string.Format("Can not open file {0}. Project oldVersion is {1}, needed oldVersion is {2}.", dlg.FileName, v1, v2), "");
                //    }
                //    if (!XmlDeserializatorBase.VersionsEqual(dlg.FileName, out v1, out v2))
                //    {
                //        Dialogs.ErrMsgBox.Show(string.Format("Can not open file {0}. Project oldVersion is {1}, needed oldVersion is {2}. \r\nUse project converter to convert the file to new oldVersion.",  dlg.FileName, v1, v2), "");
                //        return;
                //    }
                //}

                // Closes existing project
                if (Current.Project != null)
                {
                    GuiCommands.CloseProjectCommand.Execute();
                }

                ProjectSerializationManager projectSerializationManager = new ProjectSerializationManager();
			    
                try
                {
                    Current.BusyState = true;
#if SILVERLIGHT
                    Model.Project project = projectSerializationManager.LoadProjectFromClientFile(selectedFile);
#else
                    Model.Project project = projectSerializationManager.LoadProject(selectedFile);
#endif
                    Current.Project = project;
			    }
			    catch (Exception e)
			    {
                    throw new ExolutioModelException(string.Format("Failed to load project from the file \r\n'{0}'", selectedFile.FullName), e) { ExceptionTitle = "Cannot open project" };
			    }
                finally
                {
                    Current.BusyState = false; 
                }

			    ////try
			    //{
			    //    BusyState.SetBusy();

			    //    if (deserializator is XmlDeserializator)
			    //    {
			    //        Model.Project p = deserializator.RestoreProject(dlg.FileName);
			    //        p.FilePath = dlg.FileName;
			    //        p.CreateModelController();
			    //        // oldHACK (SEVERE) - this should be somewhere else ...
			    //        p.GetModelController().getUndoStack().ItemsChanged += MainWindow.UndoStack_ItemsChanged;
			    //        MainWindow.CurrentProject = p;
                //        MainWindow.Title = "eXolutio editor - " + p.FilePath;
			    //        MainWindow.HasUnsavedChanges = false;


			    //        //It is this way so that when the CurrentProject is set, ModelController is present
			    //        MainWindow.projectsWindow.BindToProject(CurrentProject);
			    //        MainWindow.navigatorWindow.BindToProject(CurrentProject);
			    //        MainWindow.propertiesWindow.BindDiagram(ref MainWindow.dockManager);
			    //        MainWindow.InitializeMainMenu();
			    //        MainWindow.HasUnsavedChanges = false;
			    //        MainWindow.OpenProjectDiagrams();
			    //    }
			    //    else
			    //    {
			    //        VersionManager versionManager = ((XmlDeserializatorVersions)deserializator).RestoreVersionedProject(dlg.FileName);
			    //        versionManager.FilePath = dlg.FileName;
			    //        foreach (Model.Project project in versionManager.VersionedProjects.Values)
			    //        {
			    //            project.FilePath = dlg.FileName;
			    //            project.CreateModelController();
			    //        }
			    //        MainWindow.projectsWindow.BindToVersionManager(versionManager);
			    //        Model.Project latestProjectVersion = versionManager.LatestVersion;
			    //        MainWindow.projectsWindow.SwitchToVersion(latestProjectVersion.Version);
			    //        MainWindow.navigatorWindow.BindToProject(latestProjectVersion);
			    //        MainWindow.InitializeMainMenu();
			    //        MainWindow.OpenProjectDiagrams();
			    //        MainWindow.HasUnsavedChanges = false;
                //        MainWindow.Title = "eXolutio editor - " + versionManager.FilePath;

			    //        #if DEBUG
			    //        foreach (KeyValuePair<Version, Model.Project> kvp in versionManager.VersionedProjects)
			    //        {
			    //            Tests.ModelIntegrity.ModelConsistency.CheckEverything(kvp.Value);    
			    //        }
			    //        Tests.ModelIntegrity.VersionsConsistency.CheckVersionsConsistency(versionManager);
			    //        #endif
			    //    }
			    //}
			    //catch
			    //{
			    //    new cmdNewProject(MainWindow, null).Execute();
			    //    throw;
			    //}
			    //finally
			    //{
			    //    BusyState.SetNormalState();
			    //}

                if (Current.Project.ProjectFile != null)
                {
                    Current.InvokeRecentFile(Current.Project.ProjectFile);
                }
			}

            Current.MainWindow.CloseRibbonBackstage();
		}

        public void Execute(string[] droppedFilePaths, bool noOpenFileDialog)
        {
            if (droppedFilePaths.Length > 0)
            {
                if (droppedFilePaths[0].ToUpper().EndsWith(CommandsResources.guiOpenProjectCommand_Execute_Exolutio_Default_Extension_No_Dot))
                {
                    Execute(fileName: droppedFilePaths[0], noOpenFileDialog: true);
                }
                else if (droppedFilePaths[0].ToUpper().EndsWith(".XSD"))
                {
                    throw new NotImplementedException("Member MainWindow.MainWindow_FileDropped not implemented for XSD.");
                }

            }
        }

        /// <summary>
		/// Opens a project from a file.
		/// </summary>
		/// <param name="parameter">ignored</param>
		public override void Execute(object parameter = null)
        {
            Execute(null, false, false);
        }

        public override bool CanExecute(object parameter = null)
        {
            return true;
        }
    }
}