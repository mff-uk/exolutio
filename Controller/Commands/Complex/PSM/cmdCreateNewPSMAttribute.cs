using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Create new PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdCreateNewPSMAttribute : ComposedCommand
    {
        [PublicArgument("Name", SuggestedValue = "newAttribute")]
        public string Name { get; set; }
        
        [PublicArgument("Lower", SuggestedValue = 1)]
        public uint Lower { get; set; }
        
        [PublicArgument("Upper", SuggestedValue = 1)]
        public UnlimitedInt Upper { get; set; }

        [PublicArgument("Type", typeof(AttributeType), AllowNullInput = true)]
        public Guid AttributeTypeGuid { get; set; }

        [PublicArgument("PSMClass", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid PSMClassGuid { get; set; }

        [PublicArgument("Represents XML element")]
        public bool Element { get; set; }

        [PublicArgument("InterpretedAttribute", ComponentType = typeof(PIMAttribute), CreateControlInEditors = false, AllowNullInput = true)]
        public Guid InterpretedAttribute { get; set; }

        [GeneratedIDArgument("AssociationGuid", typeof(PIMAssociation))]
        public Guid AttributeGuid { get; set; }
        
        public cmdCreateNewPSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdCreateNewPSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmClassGuid, Guid attributeTypeGuid, string name, uint lower, UnlimitedInt upper, bool element)
        {
            Name = name;
            Lower = lower;
            Upper = upper;
            AttributeTypeGuid = attributeTypeGuid;
            PSMClassGuid = psmClassGuid;
            Element = element;
            
        }

        internal override void GenerateSubCommands()
        {
            if (AttributeGuid == Guid.Empty) AttributeGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPSMAttribute(Controller, PSMClassGuid, Project.TranslateComponent<PSMClass>(PSMClassGuid).Schema) { AttributeGuid = AttributeGuid });
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, Name) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeCardinality(Controller, AttributeGuid, Lower, Upper) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, AttributeGuid, AttributeTypeGuid) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeXForm(Controller, AttributeGuid, Element) { Propagate = false });
            if (InterpretedAttribute != Guid.Empty)
            {
                Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, AttributeGuid, InterpretedAttribute) { Propagate = false });
            }
        }

        public override bool CanExecute()
        {
            if (Name == null || PSMClassGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_NEW_PSM_ATTR);
        }
        
    }
}
