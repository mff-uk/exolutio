using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Controls;
using EvoX.Model;
using EvoX.Model.ViewHelper;
using EvoX.ViewToolkit;
using EvoX.SupportingClasses;

namespace EvoX.View
{
    public abstract class ComponentViewBaseVH<TViewHelper> : ComponentViewBase 
        where TViewHelper : ViewHelper
    {
        public abstract TViewHelper ViewHelper { get; protected set; }

        public override void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper)
        {
            ViewHelper = (TViewHelper) viewHelper;
            base.PutInDiagram(diagramView, viewHelper);
        }

        void ViewHelper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsBindingSuspended)
            {
                UpdateView();
            }
        }

        
        protected override void BindModelView()
        {
            base.BindModelView();
            ViewHelper.PropertyChanged += ViewHelper_PropertyChanged;
            UpdateView();
        }

        protected override void UnBindModelView()
        {
            ViewHelper.PropertyChanged -= ViewHelper_PropertyChanged;
        }
    }

    public abstract class ComponentViewBase
    {
        public abstract Component ModelComponent { get; protected set; }

        public DiagramView DiagramView { get; private set; }

        protected readonly List<Control> CreatedControls = new List<Control>();

        #region model binding

        private int suspendCounter = 0; 
        public bool IsBindingSuspended { get; private set; }

        public void SuspendModelBinding()
        {
            suspendCounter++;
            IsBindingSuspended = true; 
        }

        public void ResumeModelBinding()
        {
            if (suspendCounter > 0)
            {
                suspendCounter--;
                if (suspendCounter == 0)
                {
                    IsBindingSuspended = false; 
                    UpdateView();
                }
            }
        }

        void ModelComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsBindingSuspended)
            {
                UpdateView();
            }
        }

        /// <summary>
        /// This method is used to unbind events bound 
        /// in <see cref="BindModelView"/>.
        /// </summary>
        /// <remarks>
        /// The best time to call <see cref="UnBindModelView"/>is during <see cref="RemoveFromDiagram"/>.
        /// In normal scenarios, this method need not to be overloaded. 
        /// </remarks>
        /// <seealso cref="UnBindModelView"/>
        protected virtual void UnBindModelView()
        {
            ModelComponent.PropertyChanged -= ModelComponent_PropertyChanged;
        }

        /// <summary>
        /// This method is used to bind changes in the model to 
        /// control updates (i.e. propagation of a name change to
        /// label text update) and should be called exactly once.
        /// </summary>
        /// <remarks>
        /// The best time to call <see cref="BindModelView"/>is at the end of <see cref="PutInDiagram"/>.
        /// In normal scenarios, this method need not to be overloaded. 
        /// </remarks>
        /// <seealso cref="UnBindModelView"/>
        protected virtual void BindModelView()
        {
            ModelComponent.PropertyChanged += ModelComponent_PropertyChanged;
        }

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        public virtual void UpdateView() { }

        #endregion 

        #region deffered add & remove

        private DiagramView pendingDiagramView;
        private ViewHelper pendingViewHelper;

        public void PutInDiagramDeferred(DiagramView diagramView, Component component, ViewHelper viewHelper)
        {
            this.ModelComponent = component;
            if (CanPutInDiagram(diagramView))
            {
                PutInDiagram(diagramView, viewHelper);
                diagramView.RepresentantsCollection.CollectionChanged -= DeferredAddCheck;
            }
            else
            {
                pendingDiagramView = diagramView;
                pendingViewHelper = viewHelper;
                diagramView.RepresentantsCollection.CollectionChanged += DeferredAddCheck;
            }
        }

        public void RemoveFromDiagramDeferred()
        {
            if (CanRemoveFromDiagram())
            {
                DiagramView.RepresentantsCollection.CollectionChanged -= DeferredRemoveCheck;
                RemoveFromDiagram();
            }
            else
            {
                DiagramView.RepresentantsCollection.CollectionChanged += DeferredRemoveCheck;
            }
        }

        private void DeferredRemoveCheck(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CanRemoveFromDiagram())
            {
                DiagramView.RepresentantsCollection.CollectionChanged -= DeferredRemoveCheck;
                RemoveFromDiagram();
            }
        }

        private void DeferredAddCheck(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CanPutInDiagram(pendingDiagramView))
            {
                PutInDiagram(pendingDiagramView, pendingViewHelper);
                pendingDiagramView.RepresentantsCollection.CollectionChanged -= DeferredAddCheck;
            }
        }

        #endregion

        public virtual bool CanPutInDiagram(DiagramView diagramView)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This instance of <see cref="ComponentViewBase"/> is not yet in <see cref="DiagramView"/>.<see cref="View.DiagramView.RepresentantsCollection"/>,
        /// in which it is added in the method's body. 
        /// </remarks>
        /// <param name="diagramView"></param>
        /// <param name="viewHelper"></param>
        public virtual void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper)
        {
            if (ModelComponent == null)
            {
                throw new EvoXViewException(string.Format("'{0}'ShowInDiagram can not be called when ModelCompoennt is null", this));
            }
            if (!CanPutInDiagram(diagramView))
            {
                throw new EvoXViewException(string.Format("Can not put '{0}' to diagram.", this));
            }

            DiagramView = diagramView;
            DiagramView.RepresentantsCollection[ModelComponent] = this;
        }

        public virtual bool CanRemoveFromDiagram()
        {
            return true;
        }

        public virtual void RemoveFromDiagram()
        {
            if (!CanRemoveFromDiagram())
            {
                throw new EvoXViewException(string.Format("Can not remove '{0}' from diagram.", this));
            }
            DiagramView.SelectedViews.Remove(this);
            DiagramView.RepresentantsCollection.Remove(ModelComponent);
            DiagramView = null;
        }

        #region select & highlight

        private bool selected;
        public virtual bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                foreach (Control createdControl in CreatedControls)
                {
                    if (createdControl is ISelectable && ((ISelectable)createdControl).Selected != value)
                    {
                        ((ISelectable)createdControl).Selected = value;
                    }
                }
                if (value)
                {
                    DiagramView.SelectedViews.AddIfNotContained(this);
                }
                else
                {
                    DiagramView.SelectedViews.Remove(this);
                }
                DiagramView.InvokeSelectionChanged();
            }
        }

        private bool highlighted;

        /// <summary>
        /// True if the view is highlighted. 
        /// </summary>
        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                highlighted = value;
                foreach (Control createdControl in CreatedControls)
                {
                    if (createdControl is ISelectable)
                    {
                        ((ISelectable)createdControl).Highlighted = value;
                    }
                }
            }
        }

        #endregion

        public virtual ContextMenu ContextMenu
        {
            get;
            set;
        }
    }
}