using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Trida slouzici k reprezentaci stavu konecneho automatu. 
     **/
    class AutomatState
    {
        public String name;
        public bool enterState;
        public bool exitState;
        public AutomatState startState;
        private String leftSide;
        private int depth;
        private AutomatState automatStateWithoutDepth;
        private SideCompusarity sideCompulsarity = SideCompusarity.BOTH_SIDES_COMPULSARY;

        /**
         * Vraci true pokud se stav state jmenuje stejne jako soucasny stav. 
         **/
        public bool Equals(AutomatState state)
        {
            if (this.name.Equals(state.name))
                return true;
            return false;
        }

        /**
         * Konstruktor pro tridu AutomatState.
         * 
         * atribut name udrcuje nazev stavu
         * atribut startState je odkaz na startovni stav aktualniho automatu
         **/
        public AutomatState(String name, AutomatState startState)
            : this(name, false, startState)
        {
        }

        /**
         * Konstruktor pro tridu AutomatState. 
         * 
         * atribut name udrcuje nazev stavu
         * atribut enterState urcuje, zda je stav startovnim stavem automatu
         * atribut startState je odkaz na startovni stav aktualniho automatu
         **/
        public AutomatState(String name, bool enterState, AutomatState startState)
        {
            this.name = name;
            this.enterState = enterState;
            this.exitState = false;
            this.depth = 0;
            this.automatStateWithoutDepth = this;
            if (startState == null)
                this.startState = this;
            else
                this.startState = startState;
        }

        /**
         * Konstruktor pro tridu AutomatState. Vytvori stav do ktereho se dostaneme po prejeti ze stavu previousState po hrane automatEdge.
         **/
        public AutomatState(AutomatState previousState, AutomatEdge automatEdge)
        {
            this.name = automatEdge.EndState.name;
            this.enterState = automatEdge.EndState.enterState;
            this.exitState = automatEdge.EndState.exitState;
            this.depth = previousState.depth;
            this.startState = automatEdge.EndState.startState;
            this.leftSide = automatEdge.EndState.leftSide;
            this.automatStateWithoutDepth = automatEdge.EndState.automatStateWithoutDepth;
            this.sideCompulsarity = automatEdge.EndState.SideCompulsarity;
            if (automatEdge.EdgeMode == EdgeMode.ADDING)
            {
                depth++;
            }
            else
            {
                if (automatEdge.EdgeMode == EdgeMode.REMOVING)
                {
                    depth--;
                }
            }
        }

        /**
         * Vytvori kopii stavu originalState a nastavi jmeno na stateName. 
         **/
        public AutomatState(String stateName, AutomatState originalState, Object fakeArgument)
        {
            this.name = stateName;
            this.enterState = originalState.enterState;
            this.exitState = originalState.exitState;
            this.depth = originalState.depth;
            this.startState = originalState.startState;
            this.leftSide = originalState.leftSide;
            this.automatStateWithoutDepth = this;
            this.SideCompulsarity = originalState.sideCompulsarity;
        }

        /**
         * Jmeno stavu se vzdy sklada z pismena 's' a cisla. Tato metoda vrati cislo v nazvu.
         **/
        public int getIndexOfState()
        {
            String numberPart = name.Substring(1);
            return int.Parse(numberPart);
        }

        /**
         * Vraci hloubku v jake se stav nachazi. 
         **/
        public int Depth
        {
            get
            {
                return depth;
            }
        }

        /**
         * Vraci automat v nulove hloubce. 
         **/
        public AutomatState AutomatStateWithoutDepth
        {
            get
            {
                return automatStateWithoutDepth;
            }
        }

        /**
         * Vraci nazev leve strany, pro jejiz konecneho automatu nalezi tento stav.
         **/
        public string LeftSide
        {
            set
            {
                leftSide = value;
            }
            get
            {
                return leftSide;
            }
        }

        /**
         * Vraci, zda jsou vsechny asociace nalevo, nebo napravo od tohoto stavu povinne. Tato hodnota je korektne nastavena pouze pro
         * prijmaci stavy.
         **/
        public SideCompusarity SideCompulsarity
        {
            get
            {
                return sideCompulsarity;
            }
            set
            {
                sideCompulsarity = value;
            }
        }

    }
}
