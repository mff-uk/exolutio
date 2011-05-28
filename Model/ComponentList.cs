using System.Collections.Generic;
using Exolutio.SupportingClasses;

namespace Exolutio.Model
{
    public class ComponentList<TComponent> : ExolutioList<TComponent>
        where TComponent : Component
    {
    }
}