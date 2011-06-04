using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using Exolutio.ResourceLibrary;
using Image = System.Windows.Controls.Image;

namespace Exolutio.View.Commands
{
	public abstract class guiCommandBase : ICommand, INotifyPropertyChanged
	{
		#region Constructors

		/// <summary>
		/// Creates a new <see cref="guiCommandBase"/> instance.
		/// </summary>
		protected guiCommandBase()
		{
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
            #if SILVERLIGHT
            #else 
            if (Gesture != null && Current.MainWindow != null)
            {   
                keyBinding = new KeyBinding(this, Gesture);
                Current.MainWindow.InputBindings.Add(keyBinding);
                Current.MainWindow.CommandBindings.Add(new CommandBinding(this));
            }
            #endif
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
		}

		#endregion

		#region ICommand Members

		/// <inheritdoc/>
        public abstract bool CanExecute(object parameter = null);

		/// <inheritdoc/>
		public event EventHandler CanExecuteChanged;

		/// <inheritdoc/>
        public abstract void Execute(object parameter = null);

	    public virtual void Execute()
	    {
	        Execute(null);
	    }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public virtual void OnCanExecuteChanged(EventArgs e)
        {
            EventHandler canExecuteChangedHandler = CanExecuteChanged;
            if (canExecuteChangedHandler != null)
            {
                canExecuteChangedHandler(this, e);
            }
        }

		#endregion

		#region INotifyPropertyChanged Members

		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Protected Methods

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged"/> event on this command instance.
		/// </summary>
		protected void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event on this command instance.
		/// </summary>
		/// <param name="propertyName">
		/// <para>
		/// Type: <see cref="string"/>
		/// </para>
		/// <para>
		/// Name of the property whose value has changed.
		/// </para>
		/// </param>
		protected void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Public Properties

        ///// <summary>
        ///// Gets a reference to the main application window.
        ///// </summary>
        ///// <value>
        ///// <para>
        ///// Type: <see cref="Client.MainWindow"/>
        ///// </para>
        ///// <para>
        ///// The main application window.
        ///// </para>
        ///// </value>
        //public MainWindow MainWindow
        //{
        //    get { return mainWindow; }
        //}

        //public Project CurrentProject
        //{
        //    get { return MainWindow.CurrentProject; }
        //}

        ///// <summary>
        ///// Reference to the active diagram. 
        ///// </summary>
        ///// <value><see cref="ExolutioCanvas"/></value>
        //public ExolutioCanvas ActiveDiagramView
        //{
        //    get
        //    {
        //        return MainWindow.ActiveDiagram;
        //    }
        //}

		/// <summary>
		/// Gets the reason why this command is currently disabled (cannot be executed).
		/// </summary>
		/// <value>
		/// <para>
		/// Type: <see cref="string"/>
		/// </para>
		/// <para>
		/// The textual description of the reason why the command cannot be currently executed.<br/>
		/// </para>
		/// </value>
		public string DisableReason
		{
			get { return disableReason; }
			protected set
			{
				disableReason = value;
				RaisePropertyChanged("DisableReason");
			}
		}

	    /// <summary>
	    /// Gets or sets the key gesture associated with this command.
	    /// </summary>
	    /// <value>
	    /// <para>
	    /// Type: <see cref="KeyGesture"/>
	    /// </para>
	    /// <para>
	    /// The key gesture associated with this command.
	    /// </para>
	    /// </value>
	    public virtual KeyGesture Gesture
	    {
            get { return null; }
            set { throw new InvalidOperationException("Gesture is not supposed to be set in this class. Set accessor exists only to be overriden in derived classes. "); }
	    }
        
		/// <summary>
		/// Short description of the command
		/// </summary>
		/// <value>
		/// <para>
		/// Type: <see cref="string"/>
		/// </para>
		/// </value>
		public virtual string Text { get; set; }

	    /// <summary>
		/// Gets the text of the screen tip associated with this command.
		/// </summary>
		/// <value>
		/// <para>
		/// Type: <see cref="string"/>
		/// </para>
		/// <para>
		/// The text of the screen tip associated with this command.
		/// </para>
		/// </value>
		public abstract string ScreenTipText { get; }

	    private ImageSource icon;

	    /// <summary>
        /// Gets or the icon associated with the command.
	    /// </summary>
	    /// <para>
	    /// Type: <see cref="ImageSource"/>
	    /// </para>
        public virtual ImageSource Icon { get { return icon; } set { icon = value; } }

	    /// <summary>
	    /// Gets the title of the screen tip associated with this command.
	    /// </summary>
	    /// <value>
	    /// <para>
	    /// Type: <see cref="string"/>
	    /// </para>
	    /// <para>
	    /// The title of the screen tip associated with this command.
	    /// If a <see cref="KeyGesture"/> is associated with this command,
	    /// it is automatically added to the screen tip.
	    /// </para>
	    /// </value>
	    public string ScreenTipTitleDisplay
	    {
	        get
	        {
	            if (Gesture != null)
	                return String.Format("{0} ({1})", ScreenTipTitle, Gesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture));

	            return ScreenTipTitle;
	        }
	    }

	    public virtual string ScreenTipTitle { get { return Text; } }

	    #endregion

		#region Private Fields

	    /// <summary>
		/// Holds the value of the <see cref="DisableReason"/> property.
		/// </summary>
		private string disableReason;

        #if SILVERLIGHT
        #else
		/// <summary>
		/// Holds the key binding.
		/// </summary>
		protected KeyBinding keyBinding;
        #endif	
		#endregion
	}
}