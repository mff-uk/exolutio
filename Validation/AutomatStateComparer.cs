using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Komparator slouzici k porovnani dvou AutomatState.
     **/
    class AutomatStateComparer : IEqualityComparer<AutomatState>
    {
        /**
         * Vraci true, pokud se oba stavy stejne jmenuji a maji stejnou hloubku. 
         **/
        public bool Equals(AutomatState a, AutomatState b)
        {
            if (a.name == b.name && a.Depth == b.Depth)
                return true;
            return false;
        }

        public int GetHashCode(AutomatState state)
        {
            return state.GetHashCode();
        }
    }
}
