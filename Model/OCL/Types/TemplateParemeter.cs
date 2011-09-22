using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types {
    class TemplateParemeter:Parameter {

        public TemplateParemeter(string name, Func<Operation,Classifier> templateFunction)
            : base(name, null) {
        }

        Func<Operation, Classifier> TemplateFunction {
            get;
            set;
        }


       
        public override Classifier Type {
            get {
                if (_Type == null) {
                    _Type = TemplateFunction(owner);
                }
                return _Type;
            }
        }
    }
}
