using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Urcuje, zda pri prechodu pres hranu konecneho automatu se zvetsi, zmensi nebo nemeni hloubka automatu. 
     **/
    enum EdgeMode
    {
        NONE, ADDING, REMOVING
    }
}
