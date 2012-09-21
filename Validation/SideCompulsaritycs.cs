using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Enum urcujci zda je nejaky z prechodu z leve, ci prave strany od daneho stavu povinny.
     **/
    enum SideCompusarity
    {
        VOLATILE_LEFT_SIDE, VOLATILE_RIGHT_SIDE, BOTH_SIDES_COMPULSARY, BOTH_SIDES_VOLATILE
    }
}
