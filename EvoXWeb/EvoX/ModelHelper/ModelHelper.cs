using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.SessionState;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.SupportingClasses.Reflection;

namespace EvoX.Web.ModelHelper
{
    public class ModelHelper
    {
        public static string GetHierarchicalPath(PSMComponent component)
        {
            PSMAttribute psmAttribute = component as PSMAttribute;
            if (psmAttribute != null)
            {
                return GetHierarchicalPath(psmAttribute.PSMClass) + "." + GetIdentifier(psmAttribute);
            }

            PSMAssociationMember psmAssociationMember = component as PSMAssociationMember;
            if (psmAssociationMember != null)
            {
                if (psmAssociationMember.ParentAssociation != null)
                {
                    return GetHierarchicalPath(psmAssociationMember.ParentAssociation.Parent) + "." + GetIdentifier(psmAssociationMember);
                }
                else
                {
                    return GetIdentifier(psmAssociationMember);
                }
            }

            else return string.Empty;
        }

        private static string GetIdentifier(PSMComponent psmComponent)
        {
            return psmComponent.ID.ToString();
        }

        public static Project GetSessionProject(HttpSessionState sessionState)
        {
            object o = sessionState["SessionProject"];
            return o != null ? (Project)o : null;
        }

        public static void SetSessionProject(HttpSessionState sessionState, Project project)
        {
            sessionState["SessionProject"] = project;
            SetSessionController(sessionState, null);
            GetOrCreateSessionController(sessionState);
        }

        public static Controller.Controller GetOrCreateSessionController(HttpSessionState sessionState)
        {
            object o = sessionState["SessionController"];
            Project sessionProject = GetSessionProject(sessionState);
            Controller.Controller controller = ((Controller.Controller) o);
            if (o == null || controller.Project == null)
            {
                controller = new Controller.Controller(sessionProject);
                SetSessionController(sessionState, controller);
                return controller;
            }
            else
            {
                return controller;
            }
        }

        public static void SetSessionController(HttpSessionState sessionState, Controller.Controller controller)
        {
            sessionState["SessionController"] = controller;
        }
        
        public static Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> GetAvailableOperations()
        {
            return PublicCommandsHelper.GetAvailableOperations();
        }

        
    }
}