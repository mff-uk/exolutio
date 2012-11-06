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
        private AutomatState endState;
        private EdgeMode edgeMode;
        private AttributeType attributeType = null;

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
       *  atribut att Type urcuje, jakyho typu je atribut, pro nejz je konstruovana tato hrana     
       **/
        public AutomatEdge(AutomatState endState, AttributeType attType)
            : this(endState, EdgeMode.NONE)
        {
            this.attributeType = attType;
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
         *  Kontruktor pro tridu AutomatEdge.
         *  
         *  atribut endState urcuje do ktereho stavu prechod vede
         *  atribut mode urcuje, zda pokud po hrane prejdeme, tak dochazi k zanorovani, ci vynorovani
         *  atribut att Type urcuje, jakyho typu je atribut, pro nejz je konstruovana tato hrana
         **/
        public AutomatEdge(AutomatState endState, EdgeMode mode, AttributeType attType)
        {
            this.endState = endState;
            this.edgeMode = mode;
            this.AttributeType = attType;
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

        /**
         *  Pokud to je hrana pro atribut, tak vraci jakeho typu je atribut, jinak vraci null. 
         **/
        public AttributeType AttributeType {
            get {
                return attributeType;
            }
            set {
                attributeType = value;
            }
        }
    }
}
