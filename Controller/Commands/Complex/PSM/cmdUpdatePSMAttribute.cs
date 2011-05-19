using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Complex.PSM
{
    [PublicCommand("Update PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdUpdatePSMAttribute : MacroCommand
    {
        [PublicArgument("Name", ModifiedPropertyName = "Name")]
        public string Name { get; set; }

        [PublicArgument("DefaultValue", ModifiedPropertyName = "DefaultValue", AllowNullInput = true)]
        public string DefaultValue { get; set; }
        
        [PublicArgument("Lower", ModifiedPropertyName = "Lower")]
        public uint Lower { get; set; }
        
        [PublicArgument("Upper", ModifiedPropertyName = "Upper")]
        public UnlimitedInt Upper { get; set; }

        [PublicArgument("Type", typeof(AttributeType), AllowNullInput = true, ModifiedPropertyName = "AttributeType")]
        public Guid AttributeTypeGuid { get; set; }

        [PublicArgument("Represents XML element", ModifiedPropertyName = "Element")]
        public bool Element { get; set; }

        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        [PublicArgument("Updated attribute", typeof(PSMAttribute))]
        public Guid AttributeGuid { get; set; }

        public Guid InterpretedAttribute { get; set; }

        public cmdUpdatePSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeGuid, Guid attributeTypeGuid, string name, uint lower, UnlimitedInt upper, bool element, string defaultValue)
        {
            AttributeGuid = attributeGuid;
            Name = name;
            Lower = lower;
            Upper = upper;
            AttributeTypeGuid = attributeTypeGuid;
            Element = element;
            DefaultValue = defaultValue;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, Name) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeCardinality(Controller, AttributeGuid, Lower, Upper) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, AttributeGuid, AttributeTypeGuid) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeXForm(Controller, AttributeGuid, Element) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeDefaultValue(Controller, AttributeGuid, DefaultValue) { Propagate = false });
            if (InterpretedAttribute != Guid.Empty)
            {
                Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, AttributeGuid, InterpretedAttribute) { Propagate = false });
            }
        }

        public override bool CanExecute()
        {
            return base.CanExecute();
        }
        
    }
}
