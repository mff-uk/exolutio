using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model;
using EvoX.View.Commands;
using EvoX.View.Commands.ParameterControls;
using EvoX.ViewToolkit;
using EvoX.SupportingClasses;

namespace EvoX.View
{
    public static class MenuHelper
    {
        //private static Dictionary<ScopeAttribute.EScope, ContextMenu> menuDictionary 
        //    = new Dictionary<ScopeAttribute.EScope, ContextMenu>();

        private static Dictionary<Type, guiControllerCommand> guiCommandsForControllerCommands 
            = new Dictionary<Type, guiControllerCommand>();

        private static Dictionary<ScopeAttribute.EScope, List<guiScopeCommand>> localCommandsByScope
            = new Dictionary<ScopeAttribute.EScope, List<guiScopeCommand>>();

        static MenuHelper()
        {
            /*
             * Create one instance of guiControllerCommand for each existing public command. 
             * This instance is later used in every context menu (instances of commands are shared 
             * amont the existing menus)
             */
            foreach (List<CommandDescriptor> scopeCommands in PublicCommandsHelper.publicCommandsByScope.Values)
            {
                foreach (CommandDescriptor commandDescriptor in scopeCommands)
                {
                    guiControllerCommand guiC = new guiControllerCommand();
                    CommandDescriptor descriptor = commandDescriptor;
                    guiC.ControllerCommandFactoryMethod =
                        delegate
                            {
                                return CommandSerializer.CreateCommandObject(descriptor.CommandType);
                            };
                    guiC.ControllerCommandDescription = commandDescriptor.CommandDescription;
                    guiCommandsForControllerCommands[commandDescriptor.CommandType] = guiC;
                }
            }

            foreach (Type t in typeof(guiScopeCommand).Assembly.GetTypes())
            {
                if (t.IsSubclassOf(typeof(guiScopeCommand)))
                {
                    ScopeAttribute a = (ScopeAttribute)t.GetCustomAttributes(typeof(ScopeAttribute), true).FirstOrDefault();
                    if (a != null)
                    {
                        #if SILVERLIGHT
                        foreach (ScopeAttribute.EScope scope in EnumHelper.GetValues(typeof(ScopeAttribute.EScope)))
                        #else 
                        foreach (ScopeAttribute.EScope scope in Enum.GetValues(typeof(ScopeAttribute.EScope)))
                        #endif
                        {
                            if (scope == ScopeAttribute.EScope.None)
                                continue;
                            if (a.Scope.HasFlag(scope))
                            {
                                localCommandsByScope.CreateSubCollectionIfNeeded(scope);
                                localCommandsByScope[scope].Add((guiScopeCommand)t.GetConstructor(Type.EmptyTypes).Invoke(null));
                            }
                        }
                        if (a.Scope == ScopeAttribute.EScope.None)
                        {
                            localCommandsByScope.CreateSubCollectionIfNeeded(a.Scope);
                            localCommandsByScope[a.Scope].Add((guiScopeCommand)t.GetConstructor(Type.EmptyTypes).Invoke(null));
                        }
                    }
                }
            }
        }

        private static ContextMenuItem CreateMenuItem(CommandDescriptor commandDescriptor)
        {
            ContextMenuItem menuItem = new ContextMenuItem(commandDescriptor.CommandDescription);
            //change.Icon = EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.pencil);

            guiControllerCommand guiCommandForControllerCommand = guiCommandsForControllerCommands[commandDescriptor.CommandType];
            menuItem.Command = guiCommandForControllerCommand;
            
            guiCommandForControllerCommand.OpenDialog = commandDescriptor.Parameters.Count > 1;
            guiCommandForControllerCommand.NoScope = commandDescriptor.Scope == ScopeAttribute.EScope.None;
            
            if (guiCommandForControllerCommand.OpenDialog)
            {
                menuItem.Header = commandDescriptor.CommandDescription + "...";
            }

            return menuItem;
        }

        public static EvoXContextMenu GetContextMenu(ScopeAttribute.EScope scope, Diagram diagram)
        {
            EvoXContextMenu result = new EvoXContextMenu();
            
            result.Opened += ContextMenu_Opened;
            
            foreach (CommandDescriptor commandDescriptor in PublicCommandsHelper.publicCommandsByScope[scope])
            {
                ContextMenuItem m = CreateMenuItem(commandDescriptor);
                result.Items.Add(m);
            }

            if (localCommandsByScope.ContainsKey(scope))
            {
                result.Items.Add(new Separator());
                foreach (guiScopeCommand guiScopeCommand in localCommandsByScope[scope])
                {
                    ContextMenuItem contextMenuItem = new ContextMenuItem(guiScopeCommand.Text);
                    contextMenuItem.Icon = guiScopeCommand.Icon;
                    contextMenuItem.Command = guiScopeCommand;
                    result.Items.Add(contextMenuItem);
                }
            }
            return result; 
        }
 
#if SILVERLIGHT
        public static void CreateSubmenuForCommandsWithoutScope(ContextMenu contextMenu)
        {
            contextMenu.Items.Add(new Separator());
            foreach (CommandDescriptor commandDescriptor in PublicCommandsHelper.publicCommandsByScope[ScopeAttribute.EScope.None])
            {
                ContextMenuItem m = CreateMenuItem(commandDescriptor);
                contextMenu.Items.Add(m);
            }
        }
#else   
        public static void CreateSubmenuForCommandsWithoutScope(ContextMenuItem otherItemsMenu)
        {
            foreach (CommandDescriptor commandDescriptor in PublicCommandsHelper.publicCommandsByScope[ScopeAttribute.EScope.None])
            {
                ContextMenuItem m = CreateMenuItem(commandDescriptor);
                otherItemsMenu.Items.Add(m);
            }
        }


#endif
        static void ContextMenu_Opened(object sender, RoutedEventArgs eventArgs)
        {
            EvoXContextMenu contextMenu = (EvoXContextMenu)sender;
            if (Current.MainWindow.CommandsDisabled)
            {
                contextMenu.Visibility = Visibility.Hidden;
            }
            else
            {
                PrepareCommandsScope(contextMenu.Items, contextMenu);
            }
        }

        private static void PrepareCommandsScope(ItemCollection items, EvoXContextMenu contextMenu)
        {
            foreach (ContextMenuItem contextMenuItem in items.OfType <ContextMenuItem>())
            {
                if (contextMenuItem.Command != null && contextMenuItem.Command is guiControllerCommand)
                {
                    PrepareCommandScope(contextMenu, contextMenuItem);
                }
                if (contextMenuItem.Command != null && contextMenuItem.Command is guiScopeCommand)
                {
                    ((guiScopeCommand) contextMenuItem.Command).ScopeObject = contextMenu.ScopeObject;
                }
                if (contextMenuItem.Items.Count > 0)
                {
                    PrepareCommandsScope(contextMenuItem.Items, contextMenu);
                }
            }
        }

        private static void PrepareCommandScope(EvoXContextMenu contextMenu, ContextMenuItem contextMenuItem)
        {
            guiControllerCommand guiControllerCommand = contextMenuItem.Command as guiControllerCommand;
            if (guiControllerCommand != null)
            {
                guiControllerCommand.Diagram = (Diagram)contextMenu.Diagram;
                guiControllerCommand.CreateControllerCommand();
                if (guiControllerCommand.ControllerCommand is StackedCommand)
                {
                    ((StackedCommand)guiControllerCommand.ControllerCommand).Controller = Current.Controller;
                    guiControllerCommand.OnCanExecuteChanged(null);
                }
                if (!guiControllerCommand.NoScope)
                {
                    guiControllerCommand.ScopeObject = contextMenu.ScopeObject;
                    SetScopeForCommand(guiControllerCommand.ControllerCommand, contextMenu.ScopeObject); 
                }
                guiControllerCommand.ProjectVersion = Current.ProjectVersion;
            }            
        }

        public static void SetScopeForCommand(CommandBase controllerCommand, object scopeObject)
        {
            PropertyInfo scopeProperty = PublicCommandsHelper.publicCommandsByType[controllerCommand.GetType()].ScopeProperty;
            scopeProperty.SetValue(controllerCommand, ((EvoXObject)scopeObject).ID, null);
            controllerCommand.OnCanExecuteChanged(null, null);
        }

        public static void CreateDialogControlsForCommand(Type commandType, EvoXObject scopeObject, ProjectVersion projectVersion, StackPanel stackPanel, out List<Control> controls)
        {
            CommandDescriptor commandDescriptior = PublicCommandsHelper.GetCommandDescriptor(commandType);
            commandDescriptior.ScopeObject = scopeObject;
            controls = OperationParametersControlCreator.CreateControls
                (commandDescriptior, projectVersion);

            int tabOrder = 0;
            bool foundFirst = false; 
            foreach (Control control in controls)
            {
                if (control is ScopePropertyEditor)
                {
                    ((ScopePropertyEditor)control).Value = scopeObject;
                }
                if (!foundFirst && !(control is System.Windows.Controls.Label))
                {
                    control.Focus();
                    foundFirst = true;
                }
                control.TabIndex = tabOrder++;
                control.HorizontalAlignment = HorizontalAlignment.Left;
                control.Margin = new Thickness(0, 0, 0, 5);
                if (control is System.Windows.Controls.Label)
                    control.Margin = new Thickness(0, 5, 0, 5);
                control.MinWidth = 180;
                stackPanel.Children.Add(control);
            }
        }
    }
}