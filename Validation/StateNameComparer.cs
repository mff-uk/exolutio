using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Komparator, ktery porovnava jmena stavu. 
     **/
    class StateNameComparer : IComparer<AutomatState>
    {
        /**
         *  Vraci 1, mali druhy stav v nazvu vetsi cislo, -1 ma-li prvni stav v nazvu vetsi cislo a 0 pokud maji oba stejne cislo.
         **/
        public int Compare(AutomatState a, AutomatState b)
        {
            int state1Index = a.getIndexOfState();
            int state2Index = b.getIndexOfState();
            if (state1Index < state2Index)
                return 1;
            if (state1Index > state2Index)
                return -1;
            return 0;
        }
    }
}
