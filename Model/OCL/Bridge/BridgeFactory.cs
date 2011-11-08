using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Bridge {
    public class BridgeFactory {
        /// <summary>
        /// Create instance of specific bridge class for the schema class. 
        /// </summary>
        /// <param name="schema">Source schema</param>
        /// <exception cref="UnsupportedSchemaTypeException">Unsopperted type of schema class.</exception>
        /// <returns></returns>
        public IBridgeToOCL Create(Schema schema){
            if (schema is PIM.PIMSchema) {
                return new PIMBridge((PIM.PIMSchema)schema);
            }

            if (schema is PSM.PSMSchema) {
                return new PSMBridge((PSM.PSMSchema)schema);
            }
            throw new UnsupportedSchemaTypeException(schema);
        }
    }
}
