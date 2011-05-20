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
    [PublicCommand("Update PIM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdUpdatePIMAttribute : MacroCommand
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
        
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        [PublicArgument("Updated attribute", typeof(PIMAttribute))]
        public Guid AttributeGuid { get; set; }
        
        public cmdUpdatePIMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePIMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeGuid, Guid attributeTypeGuid, string name, uint lower, UnlimitedInt upper, string defaultValue)
        {
            AttributeGuid = attributeGuid;
            Name = name;
            Lower = lower;
            Upper = upper;
            AttributeTypeGuid = attributeTypeGuid;
            DefaultValue = defaultValue;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, Name) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, AttributeGuid, Lower, Upper) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeType(Controller, AttributeGuid, AttributeTypeGuid) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeDefaultValue(Controller, AttributeGuid, DefaultValue) { Propagate = false });
        }

        public override bool CanExecute()
        {
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_UPDATE_PIM_ATTR);
        }
        
    }
}
