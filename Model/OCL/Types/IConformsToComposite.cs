﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    public interface IConformsToComposite
    {
        bool ConformsToComposite(Classifier other);
    }
}
