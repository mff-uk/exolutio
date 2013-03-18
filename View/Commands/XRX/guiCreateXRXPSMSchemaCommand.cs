using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Exolutio.Dialogs;
using Exolutio.Model.OCL;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.XRX
{
	public class guiCreateXRXPSMSchemaCommand : guiActiveDiagramCommand
	{
		public override void Execute(object parameter = null)
		{
			CreateXRXPSMDialog d = new CreateXRXPSMDialog();
			List<string> items = new List<string>() {"Artist", "Album", "Track"};
			d.SetItems(items);
			d.UseRadioButtons = false;
			d.SelectItem("Artist");
			d.SelectItem("Album");
			d.SelectItem("Track");

			string m1 = "You seem you would like to start with XRX, do you need help with that?";
			string m2 = "Just select those classes that you want for top-level collections. And let me do the rest. "; 
			//BackgroundWorker bw = new BackgroundWorker();
			//bw.DoWork += (sender, args) =>
			//	{
			//		(Current.MainWindow as Control).Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
			//			new Action(delegate() { Current.ClippySay(m1); }));
			//		Thread.Sleep(7000);
			//		(Current.MainWindow as Control).Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
			//					  new Action(delegate() { Current.ClippySay(m2); }));
					
			//	};
			//bw.RunWorkerAsync();
			d.WindowStartupLocation = WindowStartupLocation.Manual;
			d.Top = 150;
			d.Left = 50;

			Current.ActiveDiagram = (Current.ProjectVersion.Diagrams.First(di => di.Caption == "XRX PSM"));
			Current.ActiveDiagramView.ExolutioCanvas.Tag = "XRX PSM";
		}

		public override string Text
		{
			get
			{
				return "Create XRX PSM";
			}
		}

		public override string ScreenTipText
		{
			get { return "Create XRX PSM Schema"; }
		}

		public override bool CanExecute(object parameter = null)
		{
			return Current.ActiveDiagram != null && Current.ActiveDiagram.Schema is PIMSchema;
		}

		public override System.Windows.Media.ImageSource Icon
		{
			get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.index); }
		}
	}

	public class guiGenerateXRXRestInterfaceCommand : guiActiveDiagramCommand
	{
		public override void Execute(object parameter = null)
		{
			string fileContents = File.ReadAllText(@"d:\Development\Exolutio\Projects\music-xrx\rest.xqm");
			Current.MainWindow.FilePresenter.DisplayFile(fileContents, EDisplayedFileType.XQuery, "rest.xqm");
		}

		public override string Text
		{
			get
			{
				return "Generate RestXQ";
			}
		}

		public override string ScreenTipText
		{
			get { return "Generate XRX RestXQ interface"; }
		}

		public override bool CanExecute(object parameter = null)
		{
			return Current.ActiveDiagram != null && Current.ActiveDiagram.Schema is PSMSchema;
		}

		public override System.Windows.Media.ImageSource Icon
		{
			get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.gears); }
		}
	}

	public class guiGenerateXRXXFormsCommand : guiActiveDiagramCommand
	{
		public override void Execute(object parameter = null)
		{
			string fileContents = File.ReadAllText(@"d:\Development\Exolutio\Projects\music-xrx\forms.xqm");
			Current.MainWindow.FilePresenter.DisplayFile(fileContents, EDisplayedFileType.XQuery, "forms.xqm");
		}

		public override string Text
		{
			get
			{
				return "Generate XForms";
			}
		}

		public override string ScreenTipText
		{
			get { return "Generate XRX XForms"; }
		}

		public override bool CanExecute(object parameter = null)
		{
			return Current.ActiveDiagram != null && Current.ActiveDiagram.Schema is PSMSchema;
		}

		public override System.Windows.Media.ImageSource Icon
		{
			get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.form_red); }
		}
	}
}