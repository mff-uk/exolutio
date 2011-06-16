using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;
using Exolutio.SupportingClasses;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for DiagramView.xaml
    /// </summary>
    public partial class DiagramView
    {
        public Diagram Diagram { get; private set; }

#if SILVERLIGHT
#else
        public ExolutioCanvas ExolutioCanvas { get { return ExolutioCanvasWithZoomer.ExolutioCanvas; } }
#endif

        public DiagramView()
        {
            InitializeComponent();

#if SILVERLIGHT
#else
            Focusable = false;
            this.ExolutioCanvas.Width = double.NaN;
            this.ExolutioCanvas.Height = double.NaN;
            this.ExolutioCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            this.ExolutioCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
#endif
            ExolutioCanvas.CanvasSelectionCleared += Canvas_CanvasSelectionCleared;
        }

        #region Representants, loading diagrams

        public bool Loading { get; private set; }
        private readonly RepresentantsCollection representantsCollection = new RepresentantsCollection();

        public RepresentantsCollection RepresentantsCollection
        {
            get { return representantsCollection; }
        }

        private readonly List<ComponentViewBase> deferredAddComponents = new List<ComponentViewBase>();

        public List<ComponentViewBase> DeferredAddComponents
        {
            get { return deferredAddComponents; }
        }

        private readonly List<ComponentViewBase> deferredRemoveComponents = new List<ComponentViewBase>();

        public List<ComponentViewBase> DeferredRemoveComponents
        {
            get { return deferredRemoveComponents; }
        }

        public virtual IEnumerable<ComponentViewBase> LoadDiagram(Diagram diagram)
        {
            Loading = true;
            this.Diagram = diagram;

            List<ComponentViewBase> withoutViewHelpers = new List<ComponentViewBase>();
            foreach (Component component in diagram.Components)
            {
                CreateItemView(component, diagram.ViewHelpers.ValueOrNull(component), withoutViewHelpers);
            }

            diagram.Components.CollectionChanged += Components_CollectionChanged;
            Loading = false;
            
            return withoutViewHelpers;
        }


        public virtual void UnLoadDiagram()
        {

        }

        internal void DefferedAddCheck()
        {
            foreach (ComponentViewBase deferredAddComponent in DeferredAddComponents.ToArray())
            {
                deferredAddComponent.DeferredAddCheck();
            }
        }

        internal void DefferedRemoveCheck()
        {
            foreach (ComponentViewBase deferredRemoveComponent in DeferredRemoveComponents.ToArray())
            {
                deferredRemoveComponent.DeferredRemoveCheck();
            }
        }

        private ComponentViewBase CreateItemView(Component component, ViewHelper viewHelper, List<ComponentViewBase> withoutViewHelpers)
        {
            if (!this.RepresentantsCollection.Registrations.ContainsKey(component.GetType()))
            {
                System.Diagnostics.Debug.WriteLine("Cannot represent type {0}.", component);
                return null;
            }

            ComponentViewBase representant = this.RepresentantsCollection.Registrations[component.GetType()].RepresentantFactoryMethod();
            if (viewHelper == null)
            {
                viewHelper = this.RepresentantsCollection.Registrations[component.GetType()].ViewHelperFactoryMethod();
                ((IComponentViewHelper)viewHelper).Component = component;
                Diagram.ViewHelpers[component] = viewHelper;
                if (withoutViewHelpers != null)
                {
                    withoutViewHelpers.Add(representant);
                }
            }

            representant.PutInDiagramDeferred(this, component, viewHelper);
            return representant;
        }

        void Components_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Component component in e.NewItems)
                    {
                        CreateItemView(component, null, null);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Component component in e.OldItems)
                    {
                        RepresentantsCollection[component].RemoveFromDiagramDeferred();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Component component in e.OldItems)
                    {
                        RepresentantsCollection[component].RemoveFromDiagramDeferred();
                    }
                    foreach (Component component in e.NewItems)
                    {
                        CreateItemView(component, null, null);
                    }
                    break;
#if SILVERLIGHT
#else
                case NotifyCollectionChangedAction.Move:
                    // do nothing
                    break;
#endif
                case NotifyCollectionChangedAction.Reset:
                    ClearDiagramView();
                    LoadDiagram(Diagram);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearDiagramView()
        {
            foreach (Component component in Diagram.Components)
            {
                RepresentantsCollection[component].RemoveFromDiagramDeferred();
            }
        }

        #endregion

        #region Selection

        private readonly IList<ComponentViewBase> selectedViews = new List<ComponentViewBase>();

        public IList<ComponentViewBase> SelectedViews
        {
            get { return selectedViews; }
        }

        private readonly IList<IComponentTextBox> selectedTextBoxes = new List<IComponentTextBox>();

        public IList<IComponentTextBox> SelectedTextBoxes
        {
            get { return selectedTextBoxes; }
        }

        public event Action SelectionChanged;

        public void InvokeSelectionChanged()
        {
            Action handler = SelectionChanged;
            if (handler != null) handler();
        }

        void Canvas_CanvasSelectionCleared(System.ComponentModel.Component obj)
        {
            ClearSelection();
        }

        public void ClearSelection(bool invokeSelectionChanged = true)
        {
            if (SelectedViews.Count > 0)
            {
                foreach (ComponentViewBase view in SelectedViews.ToArray())
                {
                    view.Selected = false;
                }
                SelectedViews.Clear();
            }
            
            if (SelectedTextBoxes.Count > 0)
            {
                foreach (IComponentTextBox textBox in SelectedTextBoxes.ToArray())
                {
                    textBox.Selected = false;
                }
                SelectedTextBoxes.Clear();
            }

            if (invokeSelectionChanged)
            {
                InvokeSelectionChanged();
            }
        }

        public bool IsSelectedComponentOfType(Type type)
        {
            IEnumerable<Component> isSelectedComponentOfType = GetSelectedComponents();
            return isSelectedComponentOfType.Count() == 1 && type.IsAssignableFrom(isSelectedComponentOfType.First().GetType());
        }

        public void SetSelection(IEnumerable<Component> components)
        {
            ClearSelection(false);
            foreach (Component component in components)
            {
                ComponentViewBase componentViewBase = RepresentantsCollection[component];
                SelectedViews.Add(componentViewBase);
                componentViewBase.SelectAndSelectCreatedControls();
            }
            InvokeSelectionChanged();
        }

        public void SetSelection(params IEnumerable<Component>[] components)
        {
            SetSelection((IEnumerable<Component>)components);
        }

        public void SetSelection(Component component, bool focusComponent = false)
        {
            ClearSelection(false);
            ComponentViewBase view;
            RepresentantsCollection.TryGetValue(component, out view);
            bool textboxfound = false;
            if (view == null)
            {
                EditableTextBox t = null;
                if (component is PIMAttribute)
                {
                    view = RepresentantsCollection[((PIMAttribute)component).PIMClass];
                    t = ((PIMClassView)view).FirstOrDefault(tb => tb.PIMAttribute == component);
                }
                if (component is PSMAttribute)
                {
                    view = RepresentantsCollection[((PSMAttribute) component).PSMClass];
                    t = ((PSMClassView) view).FirstOrDefault(tb => tb.PSMAttribute == component);
                }
                if (t != null)
                {
                    t.Selected = true;
                    textboxfound = true; 
                }
            }
            if (view != null)
            {
                if (!textboxfound)
                {
                    view.SelectAndSelectCreatedControls();
                }
                if (focusComponent)
                {
                    //view.Focus();
                    double x = 0;
                    double y = 0;
                    if (view is INodeComponentViewBase)
                    {
                        x = Canvas.GetLeft(((INodeComponentViewBase)view).MainNode);
                        y = Canvas.GetTop(((INodeComponentViewBase)view).MainNode);
                    }
                    if (view is IConnectorViewBase)
                    {
                        x = ((IConnectorViewBase)view).Connector.GetBounds().Left;
                        y = ((IConnectorViewBase)view).Connector.GetBounds().Top;
                    }

#if SILVERLIGHT
#else
                    ScrollViewer scrollViewer = ExolutioCanvasWithZoomer.scrollViewer;
                    if (!double.IsNaN(x))
                        scrollViewer.ScrollToHorizontalOffset(x);
                    if (!double.IsNaN(y))
                        scrollViewer.ScrollToVerticalOffset(y);
#endif
                }
            }
            InvokeSelectionChanged();
        }

        public IEnumerable<Component> GetSelectedComponents()
        {
            return SelectedViews.Select(view => view.ModelComponent).Union(SelectedTextBoxes.Select(t => t.Component));
        }

        public Component GetSingleSelectedComponentOrNull()
        {
            if (GetSelectedComponents().Count() == 1)
            {
                return GetSelectedComponents().First();
            }
            return null;
        }

        #endregion

        private readonly VersionedElementInfo infoWindow = new VersionedElementInfo();

        public void InvokeVersionedElementMouseEnter(object sender, Component component)
        {
            if (Current.Project.UsesVersioning &&
                infoWindow.Component != component && 
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                infoWindow.Component = component;
                Point pointToScreen = this.PointToScreen(Mouse.GetPosition(this));
                infoWindow.Left = pointToScreen.X + 50;
                infoWindow.Top = pointToScreen.Y + 30;
                infoWindow.Show();
            }

        }

        public void InvokeVersionedElementMouseLeave(object sender, Component component)
        {
            if (infoWindow.Component == component)
            {
                infoWindow.Hide();
                infoWindow.Component = null;
            }
        }
    }
}
