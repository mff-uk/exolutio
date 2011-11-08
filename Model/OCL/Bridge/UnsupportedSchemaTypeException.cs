using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Bridge {
    class UnsupportedSchemaTypeException:ApplicationException {
        public UnsupportedSchemaTypeException(Schema schemaType)
            : base(string.Format("Scheme type {0} is unsupported.",schemaType.GetType().Name)) {
        }
    }
}
