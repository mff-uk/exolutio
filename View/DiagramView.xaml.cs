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
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;
using EvoX.SupportingClasses;

namespace EvoX.View
{
    /// <summary>
    /// Interaction logic for DiagramView.xaml
    /// </summary>
    public partial class DiagramView 
    {
        public Diagram Diagram { get; private set; }

        public EvoXCanvas EvoXCanvas { get { return EvoXCanvasWithZoomer.EvoXCanvas; } }

        public DiagramView()
        {
            InitializeComponent();

            #if SILVERLIGHT
            #else
            Focusable = false;
            this.EvoXCanvas.Width = double.NaN;
            this.EvoXCanvas.Height = double.NaN;
            this.EvoXCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            this.EvoXCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            #endif
        }

        #region Representants, loading diagrams

        private readonly RepresentantsCollection representantsCollection = new RepresentantsCollection(); 

        public RepresentantsCollection RepresentantsCollection
        {
            get { return representantsCollection; }
        }

        public virtual IEnumerable<ComponentViewBase> LoadDiagram(Diagram diagram)
        {
            this.Diagram = diagram;

            List<ComponentViewBase> withoutViewHelpers = new List<ComponentViewBase>();
            foreach (Component component in diagram.Components)
            {
                CreateItemView(component, diagram.ViewHelpers.ValueOrNull(component), withoutViewHelpers);
            }

            diagram.Components.CollectionChanged += Components_CollectionChanged;
            return withoutViewHelpers;
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
                ((IComponentViewHelper) viewHelper).Component = component;
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

        public event Action SelectionChanged;

        public void InvokeSelectionChanged()
        {
            Action handler = SelectionChanged;
            if (handler != null) handler();
        }

        public void ClearSelection(bool invokeSelectionChanged = true)
        {
            foreach (ComponentViewBase view in SelectedViews.ToArray())
            {
                view.Selected = false; 
            }
            SelectedViews.Clear();
            if (invokeSelectionChanged)
            {
                InvokeSelectionChanged();
            }
        }

        public bool IsSelectedComponentOfType(Type type)
        {
            IEnumerable<Component> isSelectedComponentOfType = GetSelectedComponents();
            return isSelectedComponentOfType.Count() == 1 && isSelectedComponentOfType.First().GetType() == type;
        }

        public void SetSelection(IEnumerable<Component> components)
        {
            ClearSelection(false);
            foreach (Component component in components)
            {
                ComponentViewBase componentViewBase = RepresentantsCollection[component];
                SelectedViews.Add(componentViewBase);
                componentViewBase.Selected = true; 
            }
            InvokeSelectionChanged();
        }

        public void SetSelection(params IEnumerable<Component>[] components)
        {
            SetSelection((IEnumerable<Component>)components);
        }

        public void SetSelection(Component component)
        {
            ClearSelection(false);
            ComponentViewBase view = RepresentantsCollection[component];
            if (view != null)
            {
                view.Selected = true;
            }
            InvokeSelectionChanged();
        }

        public IEnumerable<Component> GetSelectedComponents()
        {
            return SelectedViews.Select(view => view.ModelComponent);
        }

        #endregion 
    }
}
