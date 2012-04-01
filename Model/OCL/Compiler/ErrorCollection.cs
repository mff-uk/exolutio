using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.Compiler {
    public class ErrorCollection {

        public ErrorCollection() {
            Errors = new List<ErrorItem>();
        }

        public ErrorCollection(ErrorCollection errorRoot) {
            Errors = errorRoot.Errors;
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

        public void CopyToLog(Log<OclExpression> log)
        {
            foreach (ErrorItem errorItem in Errors)
            {
                if (errorItem is CodeErrorItem)
                {
                    CodeErrorItem codeError = (CodeErrorItem) errorItem;
                    LogMessage<OclExpression> logMessage = log.AddError(codeError.Text);
                    if (codeError.StartToken != null)
                    {
                        logMessage.Line = codeError.StartToken.Line;
                        logMessage.Column = codeError.StartToken.CharPositionInLine;
                    }
                }
                else
                {
                    log.AddError(errorItem.Text);
                }
            }
        }
    }

}
