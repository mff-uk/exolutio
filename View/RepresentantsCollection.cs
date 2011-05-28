using System;
using System.Collections.Generic;
using Exolutio.Model;
using Exolutio.Model.ViewHelper;
using Exolutio.SupportingClasses;

namespace Exolutio.View
{
    public class RepresentantsCollection : ObservableDictionary<Component, ComponentViewBase>
    {
        public Component this[ComponentViewBase component]
        {
            get { return GetRepresentedComponent(component); }
        }

        public Component GetRepresentedComponent(ComponentViewBase component)
        {
            return component.ModelComponent;
        }

        public ComponentViewBase GetViewOfComponent(Component component)
        {
            if (this.ContainsKey(component))
                return this[component];
            else
                return null;
        }

        public bool IsElementPresent(Component component)
        {
            return this.ContainsKey(component);
        }

        private readonly Dictionary<Type, RegistrationClass> registrations
            = new Dictionary<Type, RegistrationClass>();

        public Dictionary<Type, RegistrationClass> Registrations
        {
            get { return registrations; }
        }

        public struct RegistrationClass
        {
            public delegate ComponentViewBase RepresentantFactoryMethodDelegate();
            
            public RepresentantFactoryMethodDelegate RepresentantFactoryMethod { get; set; }
            public ViewHelperFactoryMethodDelegate ViewHelperFactoryMethod { get; set; }
            public Type ModelType { get; set; }
            public Type ViewHelperType { get; set; }
            public Type ViewType { get; set; }

            public RegistrationClass(RepresentantFactoryMethodDelegate representantFactoryMethod, ViewHelperFactoryMethodDelegate viewHelperFactoryMethod, Type modelType, Type viewHelperType, Type viewType) : this()
            {
                RepresentantFactoryMethod = representantFactoryMethod;
                ViewHelperFactoryMethod = viewHelperFactoryMethod;
                ModelType = modelType;
                ViewHelperType = viewHelperType;
                ViewType = viewType;
            }
        }
    }
}