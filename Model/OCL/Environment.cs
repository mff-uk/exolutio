using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL {

    /// <summary>
    /// 
    /// </summary>
    public abstract class Environment {

        public Exolutio.Model.OCL.Environment Parent {
            get;
            private set;
        }

        /// <summary>
        /// Find a named element in the current environment, not in its parents, based on a single name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract ModelElement LookupLocal(string name);

        public abstract ModelElement LookupNamespaceLocal(string name);

        /// <summary>
        /// Find a named element in the current environment or recursively in its parent environment, based on a single name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract ModelElement Lookup(string name);

        /// <summary>
        /// Find a named element in the current environment or recursively in its parent environment, based on a path name.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract ModelElement LookupPathName(IEnumerable<string> path);

        /// <summary>
        /// Add all elements in the namespace to the environment.
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        public Environment CreateFromNestedNamespace(Namespace ns) {
            NamespaceEnvironment nsEnv = new NamespaceEnvironment(ns);
            nsEnv.Parent = this;
            return nsEnv;
        }

        /// <summary>
        /// Add a new named element to the environment.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="impl"></param>
        /// <returns></returns>
        public Environment AddElement(string name, ModelElement type, bool impl) {
            if (LookupLocal(name) != null) {
                throw new Exception(string.Format("Variable name `{0}` has already existed.", name));
            }

            return new AddElementEnvironment(this,name, type, impl);
        }
    }


    public class NamespaceEnvironment : Environment {
        private Namespace internalNamespace;

        public NamespaceEnvironment(Namespace ns) {
            internalNamespace = ns;
        }

        public override ModelElement LookupLocal(string name) {
            if (internalNamespace.NestedClassifier.ContainsKey(name)) {
                return internalNamespace.NestedClassifier[name];
            }
            if (internalNamespace.NestedNamespace.ContainsKey(name)) {
                return internalNamespace.NestedNamespace[name];
            }
            return null;
        }

        public override ModelElement Lookup(string name) {
            ModelElement lookupLocalResult = LookupLocal(name);
            if (lookupLocalResult != null) {
                return lookupLocalResult;
            }

            if (Parent != null) {
                Parent.Lookup(name);
            }
            return null;

        }

        public override ModelElement LookupNamespaceLocal(string name) {
            if (internalNamespace.NestedNamespace.ContainsKey(name)) {
                return internalNamespace.NestedNamespace[name];
            }
            return null;
        }

        public override ModelElement LookupPathName(IEnumerable<string> path) {
            int pathLength = path.Count();
            if (pathLength == 0) {
                return null;
            }

            string first = path.First();

            if (pathLength == 1) {
                // musi byt local
                return LookupLocal(first);
            }
            else {
                var forwardLookup = ForwardLookupPathName(path, internalNamespace);
                if (forwardLookup != null) {
                    return forwardLookup;
                }


                if (Parent != null) {
                    return Parent.LookupPathName(path);
                }
                else {
                    return null;
                }

            }
        }

        private ModelElement ForwardLookupPathName(IEnumerable<string> path, Namespace ns) {
            int pathLength = path.Count();
            if (pathLength == 0) {
                return null;
            }

            string first = path.First();
            if (pathLength == 1) {
                return ns.NestedClassifier.ContainsKey(first) ? ns.NestedNamespace[first] : null;
            }
            else {

                if (ns.NestedNamespace.ContainsKey(first)) {
                    return ForwardLookupPathName(path.Skip(1), ns.NestedNamespace[first]);
                }
                else {
                    return null;
                }
            }
        }
    }

    public class AddElementEnvironment : Environment {
        private string Name;
        private ModelElement Type;
        private bool IsImplicit;
        private Environment RootEnviroment;

        public AddElementEnvironment(Environment rootEnv, string name, ModelElement type, bool impl) {
            RootEnviroment = rootEnv;
            Name = name;
            Type = type;
            IsImplicit = impl;
        }

        public override ModelElement LookupLocal(string name) {
            if (name == this.Name) {
                return Type;
            }
            else {
                return RootEnviroment.LookupLocal(name);
            }
        }

        public override ModelElement LookupNamespaceLocal(string name) {
            if (name == this.Name && Type is Namespace) {
                return Type;
            }
            else {
                return RootEnviroment.LookupNamespaceLocal(name);
            }
        }

        public override ModelElement Lookup(string name) {
            if (name == this.Name) {
                return Type;
            }
            else {
                return RootEnviroment.Lookup(name);
            }
        }

        public override ModelElement LookupPathName(IEnumerable<string> path) {
            // AddElemnt neni soucasti konkretniho namespace => nebudeme hledat path v name
            return RootEnviroment.LookupPathName(path);
        }
    }
}
