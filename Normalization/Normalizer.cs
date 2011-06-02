using System;
using Exolutio.Controller;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers;
using Exolutio.Model;
using Exolutio.Model.PSM;
using cmdDeletePSMAssociation = Exolutio.Controller.Commands.Complex.PSM.cmdDeletePSMAssociation;
using cmdDeleteRootPSMClass = Exolutio.Controller.Commands.Complex.PSM.cmdDeleteRootPSMClass;

namespace Exolutio.Model.PSM.Normalization
{
    public class Normalizer
    {
        public Controller.Controller Controller { get; set; }

        private ModelVerifier modelVerifier { get; set; }

        public CommandReportBase FinalReport { get; set; }

        public void NormalizeSchema(PSMSchema schema)
        {
            modelVerifier = new ModelVerifier();
            //Controller.BeginMacro();
            
            FinalReport = new NestedCommandReport();

            bool somethingFound = false;
            while (!modelVerifier.TestSchemaNormalized(schema))
            {
                somethingFound = true; 
                StackedCommand command = GetNormalizationCommand(schema);
                command.Execute();
                if (command is MacroCommand)
                {
                    NestedCommandReport commandReport = ((MacroCommand)command).GetReport();
                    ((NestedCommandReport)FinalReport).NestedReports.Add(commandReport);
                }
            }

            if (!somethingFound)
            {
                FinalReport = new CommandReport("Schema is already normalized. ");
            }
            //Controller.CommitMacro();
        }

        private StackedCommand GetNormalizationCommand(PSMSchema schema)
        {
            // (d) Content model association normalization 
            if (!modelVerifier.TestSchemaClassChildrenAreClasses(schema))
            {
                PSMContentModel cm = (PSMContentModel) modelVerifier.LastViolatingComponent;
                cmdDeletePSMAssociation command = new cmdDeletePSMAssociation(Controller);
                command.Set(cm.ParentAssociation.ID);
                return command;
            } // (c) Empty name association 
            else if (!modelVerifier.TestSchemaClassChildrenNames(schema))
            {
                PSMAssociation a = (PSMAssociation)modelVerifier.LastViolatingComponent;
                cmdDeletePSMAssociation command = new cmdDeletePSMAssociation(Controller);
                command.Set(a.ID);
                return command;
            } // (e) Root content model normalization 
            else if (!modelVerifier.TestRootsAreNotContentModels(schema))
            {
                PSMContentModel cm = (PSMContentModel)modelVerifier.LastViolatingComponent;
                MacroCommand m = new MacroCommand(Controller);
                foreach (PSMAssociation childPsmAssociation in cm.ChildPSMAssociations)
                {
                    cmdDeletePSMAssociation delA = new cmdDeletePSMAssociation(Controller);
                    delA.Set(childPsmAssociation.ID);
                    m.Commands.Add(delA);
                }
                cmdDeletePSMContentModel delCM = new cmdDeletePSMContentModel(Controller);
                delCM.Set(cm.ID);
                m.Commands.Add(delCM);
                return m;
            } // (f) Root class normalization 
            else if (!modelVerifier.TestRootsAreReferenced(schema))
            {
                PSMClass c = (PSMClass) modelVerifier.LastViolatingComponent;
                cmdDeleteRootPSMClass command = new cmdDeleteRootPSMClass(Controller);
                command.Set(c.ID);
                return command;
            } // (a) Cardinality normalization 
            else if (!modelVerifier.TestSchemaClassChildrenCardinality(schema))
            {
                PSMAssociation a = (PSMAssociation) modelVerifier.LastViolatingComponent;
                cmdUpdatePSMAssociationCardinality command = new cmdUpdatePSMAssociationCardinality(Controller);
                command.Set(a.ID, 1, 1);
                return command;
            } // (b) Name normalization 
            else if (!modelVerifier.TestContentModelsAssociationNames(schema))
            {
                PSMAssociation a = (PSMAssociation)modelVerifier.LastViolatingComponent;
                acmdRenameComponent command = new acmdRenameComponent(Controller, a.ID, String.Empty);
                return command;
            }
            else
            {
                throw new InvalidOperationException("Schema is already normalized. ");
            }
        }
    }
}