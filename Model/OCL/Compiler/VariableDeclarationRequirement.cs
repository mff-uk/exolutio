using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Compiler {
    enum VariableDeclarationRequirement {
        /// <summary>
        /// Type in not empty and value is empty
        /// </summary>
        TupleType, 
        /// <summary>
        /// Type is optimal and init value is not empty
        /// </summary>
        TupleLiteral, 
        /// <summary>
        /// Todo
        /// </summary>
        Iterate,
        /// <summary>
        /// Type is not empty and value is empty
        /// </summary>
        Iterator, 
        /// <summary>
        /// Type is optimal and value is not empty
        /// </summary>
        Let, 
        OperationContext,
        Def
    }
}
