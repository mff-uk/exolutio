using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using EvoX.Model;
using EvoX.Model.PSM;

namespace EvoX.Web.ModelHelper
{
    public class PSMSchemaAsDataSource : HierarchicalDataSourceControl
    {
        public PSMSchema PSMSchema { get; private set; }

        public PSMSchemaAsDataSource(PSMSchema psmSchema) : base()
        {
            PSMSchema = psmSchema;
        }

        // Return a strongly typed view for the current data source control.
        private PSMSchemaAsDataSourceView view = null;
        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            if (null == view)
            {
                view = new PSMSchemaAsDataSourceView(PSMSchema);
            }
            return view;
        }

        // The PSMSchemaAsDataSource can be used declaratively. To enable
        // declarative use, override the default implementation of
        // CreateControlCollection to return a ControlCollection that
        // you can add to.
        protected override ControlCollection CreateControlCollection()
        {
            return new ControlCollection(this);
        }
    }

    // The PSMSchemaAsDataSourceView class encapsulates the
    // capabilities of the PSMSchemaAsDataSource data source control.
    public class PSMSchemaAsDataSourceView : HierarchicalDataSourceView
    {
        public PSMSchema PSMSchema { get; private set; }

        public PSMSchemaAsDataSourceView(PSMSchema psmSchema)
        {
            PSMSchema = psmSchema;
        }

        public override IHierarchicalEnumerable Select()
        {
            {
                PSMSchemaHierarchicalEnumerable fshe = new PSMSchemaHierarchicalEnumerable();

                foreach (PSMAssociationMember root in PSMSchema.Roots)
                {
                    fshe.Add(new PSMSchemaHierarchyData(root));
                }
                return fshe;
            }
        }
    }
    
    public class PSMSchemaHierarchicalEnumerable : ArrayList, IHierarchicalEnumerable
    {
        public IHierarchyData GetHierarchyData(object enumeratedItem)
        {
            return enumeratedItem as IHierarchyData;
        }
    }

    public class PSMSchemaHierarchyData : IHierarchyData
    {
        private bool attributesAsNodes = false;

        public PSMSchemaHierarchyData(PSMComponent obj)
        {
            component = obj;
        }

        private readonly PSMComponent component = null;

        private TreeNodeContentProvider nodeContentProvider;
        private  TreeNodeContentProvider NodeContentProvider
        {
            get
            {
                if (nodeContentProvider == null)
                {
                    nodeContentProvider = new TreeNodeContentProvider();
                }
                return nodeContentProvider;
            }
        }

        public override string ToString()
        {
            return NodeContentProvider.Process(component);
        }
        // IHierarchyData implementation.
        public bool HasChildren
        {
            get { return ModelIterator.HasPSMChildren(component, attributesAsNodes); }
        }
        // DirectoryInfo returns the OriginalPath, while FileInfo returns
        // a fully qualified path.
        public string Path
        {
            get
            {
                return ModelHelper.GetHierarchicalPath(component);
            }
        }

        public object Item
        {
            get
            {
                return component;
            }
        }
        public string Type
        {
            get
            {
                return "PSMSchemaData";
            }
        }
        public IHierarchicalEnumerable GetChildren()
        {
            PSMSchemaHierarchicalEnumerable children = new PSMSchemaHierarchicalEnumerable();

            IEnumerable<PSMComponent> psmChildren = ModelIterator.GetPSMChildren(component, attributesAsNodes);
            foreach (PSMComponent child in psmChildren)
            {
                children.Add(new PSMSchemaHierarchyData(child));
            }

            return children;
        }

        public IHierarchyData GetParent()
        {
            PSMComponent parentComponent = ModelIterator.GetPSMParent(component, attributesAsNodes);
            return new PSMSchemaHierarchyData(parentComponent);
        }
    }
}