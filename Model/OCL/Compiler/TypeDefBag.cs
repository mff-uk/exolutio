using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.Compiler {
    class TypeDefBag {
        public TypeDefBag(string name, Classifier type, VariableDeclaration declaration) {
            this.Name = name;
            this.Type = type;
            this.Declaration = declaration;
        }

        public string Name {
            get;
            protected set;
        }

        public Classifier Type {
            get;
            protected set;
        }

        public VariableDeclaration Declaration {
            get;
            protected set;
        }
    }
}
