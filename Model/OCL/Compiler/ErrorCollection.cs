using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Compiler {
    public class ErrorCollection {

        public ErrorCollection() {
            Errors = new List<ErrorItem>();
        }

        /// <summary>
        /// Tady to bude chtit udelat nake zmeny.
        /// </summary>
        public List<ErrorItem> Errors {
            get;
            set;
        }

        public bool HasError {
            get {
                return Errors.Count != 0;
            }
        }

        public void AddError(ErrorItem newError)
        {
            Errors.Add(newError);
        }
    }

}
