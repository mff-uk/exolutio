using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Trida slouzici k predavani vysledku validace a konstrukci hlasky o vysledku validace. 
     **/
    public class ValidationResult
    {
        private bool successful;
        private String message;

        public ValidationResult( bool successful,Stack<String> readedNodesStackTrace  ) {
            this.successful = successful;
            StringBuilder messageBuilder = new StringBuilder();
            foreach (String nodeName in readedNodesStackTrace.Reverse())
            {
                messageBuilder.Append(nodeName);
                messageBuilder.Append(".");
            }
            if (messageBuilder.Length > 0)
            {
                message = "Error in ";                
                message += messageBuilder.ToString();
            }
            else {
                if (successful)
                    message = "File coresponds to the model.";
                else
                    message = "Error in root element.";
            }            
        }

        public ValidationResult(bool successful, String message) {
            this.successful = successful;
            this.message = message;
        }

        public bool Successful {
            get {
                return successful;
            }
        }

        public String Message {
            get {
                return message;
            }
        }
    }
}
