using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    public class cmdReorderComponents<TComponentType> : MacroCommand where TComponentType : ExolutioObject
    {
        public UndirectCollection<TComponentType> OwnerCollection { get; set; }

        public List<Guid> ComponentGuids { get; set; }

        public cmdReorderComponents() { }

        public cmdReorderComponents(Controller c)
            : base(c) { }

        public void Set(UndirectCollection<TComponentType> ownerCollection, List<Guid> componentGuids)
        {
            ComponentGuids = componentGuids;
            OwnerCollection = ownerCollection;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdReorderComponents<TComponentType>(Controller, OwnerCollection, ComponentGuids));
        }

    }
}
