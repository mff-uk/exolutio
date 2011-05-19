using System.Collections.Generic;
using EvoX.SupportingClasses;

namespace EvoX.Model
{
    public class ComponentList<TComponent> : EvoXList<TComponent>
        where TComponent : Component
    {
    }
}