using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Komparator slouzici k porovnani dvou hran. 
     **/
    class AutomatEdgeComparer : IEqualityComparer<AutomatEdge>
    {
        /**
         * Vraci true, pokud hrany vedou do stejneho stavu a pokud maji stejny StackMode. 
         **/
        public bool Equals(AutomatEdge a, AutomatEdge b)
        {
            if (a.EndState.Equals(b.EndState) && a.EdgeMode == b.EdgeMode)
                return true;
            return false;
        }

        public int GetHashCode(AutomatEdge edge)
        {
            return edge.GetHashCode();
        }
    }
}
