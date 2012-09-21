using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Trida, ktera slouzi k reprezentaci prechodu mezi dvema stavy konecneho automatu. 
     **/
    class AutomatEdge
    {
        private AutomatState startState;
        private AutomatState endState;
        private EdgeMode edgeMode;

        /**
         *  Kontruktor pro tridu AutomatEdge.
         *  
         *  atribut endState urcuje do ktereho stavu prechod vede
         **/
        public AutomatEdge(AutomatState endState)
            : this(endState, EdgeMode.NONE)
        {
        }

        /**
         *  Kontruktor pro tridu AutomatEdge.
         *  
         *  atribut endState urcuje do ktereho stavu prechod vede
         *  atribut mode urcuje, zda pokud po hrane prejdeme, tak dochazi k zanorovani, ci vynorovani
         **/
        public AutomatEdge(AutomatState endState, EdgeMode mode) {
            this.endState = endState;
            this.edgeMode = mode;
        }

        /**
         * Vraci stav do ktereho vede prechod. 
         **/
        public AutomatState EndState {
            get {
                return endState;
            }
        }

        /**
         * Vraci mod, ktery urcuje, zda pri prejiti hrany dochazi k zanorovani, vynorovani, ci se hloubka nemeni. 
         **/
        public EdgeMode EdgeMode {
            get {
                return edgeMode;
            }
        }
    }
}
