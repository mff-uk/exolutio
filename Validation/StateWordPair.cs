using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     *  Dvojice stav automatu a jmeno hrany pres kterou se prechazi. 
     **/
    struct StateWordPair
    {
        public AutomatState automatState;
        public String word;

        public bool Equals(StateWordPair pair)
        {
            if (this.automatState.Equals(pair.automatState) && this.word.Equals(pair.word))
                return true;
            return false;
        }

        public StateWordPair(AutomatState automatState)
            : this(automatState, null)
        {
        }

        public StateWordPair(AutomatState automatState, String word)
        {
            this.automatState = automatState;
            this.word = word;
        }
    }
}
