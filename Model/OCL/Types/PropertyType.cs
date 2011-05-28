using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public enum PropertyType
    {
        /// <summary>
        /// Attribut - cordinalyty 1..1
        /// </summary>
        One,
        /// <summary>
        /// Association which has cardinality 0..1 or 1..1
        /// Attribut and Association
        /// </summary>
        ZeroToOne, 
        /// <summary>
        /// Other(0..*,1..*,3..4,...)
        /// Only Association
        /// </summary>
        Many
    }
}
