﻿using System.Reflection;
using EvoX.Model.PIM;
using System.Linq;

namespace EvoX.Controller.Commands.Reflection
{
    public class PIMAttributeNeighboringClassParameterConsistency : ParameterConsistency
    {
        public const string Key = "PIMAttributeNeighboringClassParameterConsistency";

        public override bool VerifyConsistency(object superordinateObject, object candidate)
        {
            PIMAttribute pimAttribute = (PIMAttribute)superordinateObject;
            PIMClass pimClass = (PIMClass)candidate;

            return pimAttribute.PIMClass != pimClass &&
                pimAttribute.PIMClass.PIMAssociationEnds.Any(e => e.PIMAssociation.PIMClasses.Contains(pimClass));
        }
    }
}