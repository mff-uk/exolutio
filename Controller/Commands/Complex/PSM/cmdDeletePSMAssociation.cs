using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Delete PSM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdDeletePSMAssociation : MacroCommand
    {
        [PublicArgument("Deleted PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }
        
        public cmdDeletePSMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid associationGuid)
        {
            AssociationGuid = associationGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(AssociationGuid);

            Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, association, 1, 1) { Propagate = false });
            Commands.Add(new acmdRenameComponent(Controller, association, "") { Propagate = false });
            Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, association, Guid.Empty));
            if (association.Child != null && !(association.Child is PSMClass && (association.Child as PSMClass).Interpretation != null))
            {
                /*RESOLVE POTENTIAL PROBLEMS WITH INTERPRETED ATTRIBUTES IN UNINTERPRETED SUBCLASSES*/
                IEnumerable<PSMClass> unInterpretedSubClasses = association.Child.UnInterpretedSubClasses(true);
                //PSM attributes within the uninterpreted PSM Class subtree cannot have interpretations
                foreach (PSMAttribute a in unInterpretedSubClasses
                      .SelectMany<PSMClass, PSMAttribute>(cl => cl.PSMAttributes)
                      .Where<PSMAttribute>(at => at.Interpretation != null)
                        )
                {
                    Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, a, Guid.Empty) { Propagate = false });
                }

                /*RESOLVE POTENTIAL PROBLEMS WITH INTERPRETED ASSOCIATIONS IN UNINTERPRETED SUBCLASSES SUBTREE*/
                foreach (PSMAssociation a in unInterpretedSubClasses
                  .Select<PSMClass, PSMAssociation>(cl => cl.ParentAssociation)
                  .Where<PSMAssociation>(assoc => assoc.Interpretation != null)
                )
                {
                    Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, a, Guid.Empty) { Propagate = false });
                }
            }
            Commands.Add(new acmdDeletePSMAssociation(Controller, association));
        }

        public override bool CanExecute()
        {
            if (AssociationGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_ASSOCIATION);
        }

    }
}
