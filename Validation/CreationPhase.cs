using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Enum urcujci v jake fazi tvorby konecneho automatu jsme. 
     **/
    enum CreationPhase
    {
        ATTRIBUTE, ELEMENT_ATTRIBUTE, ELEMENT, CONTENT_MODEL
    }
}
