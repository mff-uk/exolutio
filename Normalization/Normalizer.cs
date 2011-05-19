using System;
using EvoX.Controller;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;
using EvoX.Controller.Commands.Atomic.PSM.MacroWrappers;
using EvoX.Model;
using EvoX.Model.PSM;
using cmdDeletePSMAssociation = EvoX.Controller.Commands.Complex.PSM.cmdDeletePSMAssociation;
using cmdDeletePSMClass = EvoX.Controller.Commands.Complex.PSM.cmdDeletePSMClass;

namespace EvoX.Model.PSM.Normalization
{
    public class Normalizer
    {
        public Controller.Controller Controller { get; set; }

        private ModelVerifier modelVerifier { get; set; }

        public NestedCommandReport FinalReport { get; set; }

        public void NormalizeSchema(PSMSchema schema)
        {
            modelVerifier = new ModelVerifier();
            //Controller.BeginMacro();
            
            FinalReport = new NestedCommandReport();
                
            while (!modelVerifier.TestSchemaNormalized(schema))
            {
                StackedCommand command = GetNormalizationCommand(schema);
                command.Execute();
                if (command is MacroCommand)
                {
                    NestedCommandReport commandReport = ((MacroCommand)command).GetReport();
                    FinalReport.NestedReports.Add(commandReport);
                }
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
                cmdDeletePSMClass command = new cmdDeletePSMClass(Controller);
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