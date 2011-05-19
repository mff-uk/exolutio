using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Complex.PIM
{
    [PublicCommand("Create new PIM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdCreateNewPIMAttribute : MacroCommand
    {
        [PublicArgument("Name", SuggestedValue = "newAttribute")]
        public string Name { get; set; }
        
        [PublicArgument("Lower", SuggestedValue = 1)]
        public uint Lower { get; set; }
        
        [PublicArgument("Upper", SuggestedValue = 1)]
        public UnlimitedInt Upper { get; set; }

        [PublicArgument("Type", typeof(AttributeType), AllowNullInput = true)]
        public Guid AttributeTypeGuid { get; set; }

        [PublicArgument("DefaultValue", ModifiedPropertyName = "DefaultValue", AllowNullInput = true)]
        public string DefaultValue { get; set; }

        [PublicArgument("PIMClass", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid PIMClassGuid { get; set; }

        public Guid AttributeGuid { get; set; }
        
        public cmdCreateNewPIMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdCreateNewPIMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid pimClassGuid, Guid attributeTypeGuid, string name, uint lower, UnlimitedInt upper, string defaultValue)
        {
            Name = name;
            Lower = lower;
            Upper = upper;
            AttributeTypeGuid = attributeTypeGuid;
            PIMClassGuid = pimClassGuid;
            DefaultValue = defaultValue;
        }

        protected override void GenerateSubCommands()
        {
            if (AttributeGuid == Guid.Empty) AttributeGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPIMAttribute(Controller, PIMClassGuid, Project.TranslateComponent<PIMClass>(PIMClassGuid).Schema) { AttributeGuid = AttributeGuid });
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, Name) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, AttributeGuid, Lower, Upper) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeType(Controller, AttributeGuid, AttributeTypeGuid) { Propagate = false });
            if (!string.IsNullOrEmpty(DefaultValue))
            {
                Commands.Add(new acmdUpdatePIMAttributeDefaultValue(Controller, AttributeGuid, DefaultValue) { Propagate = false });
            }
        }

        public override bool CanExecute()
        {
            if (Name == null || PIMClassGuid == Guid.Empty) return false;
            return base.CanExecute();
        }
        
    }
}
