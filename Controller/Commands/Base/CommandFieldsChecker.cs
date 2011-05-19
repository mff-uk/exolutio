using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using EvoX.SupportingClasses.Reflection;

namespace EvoX.Controller.Commands
{
	/// <summary>
	/// Class provides methods to check whether all command's
	/// Mandatory arguments (properties with <see cref="PublicArgumentAttribute"/> attribute) are properly initialized
	/// before executing the command and all command results (properties with <see cref="CommandResultAttribute"/> attribute)
	/// are set after executing the command.
	/// </summary>
	public class CommandFieldsChecker
	{
		private readonly Dictionary<Type, Dictionary<String, GetHandler>> mandatoryArguments = new Dictionary<Type, Dictionary<string, GetHandler>>();

		private readonly Dictionary<Type, Dictionary<String, GetHandler>> commandResults = new Dictionary<Type, Dictionary<string, GetHandler>>();

		/// <summary>
		/// <see cref="Debug.Fail(string)">Fails</see> if some of the 
		/// mandatory arguments (properties with <see cref="PublicArgumentAttribute"/> attribute) have <c>null</c> value
		/// </summary>
		/// <param name="command">checked command</param>
		public void CheckMandatoryArguments(CommandBase command)
		{
			//if (String.IsNullOrEmpty(command.Description))
			//{
			//	Debug.WriteLine("WARNING: Command executed without description: " + command);
			//}
			CheckAttributesNotNull(command.GetType(), command, mandatoryArguments, typeof(PublicArgumentAttribute), CommandErrors.CMDERR_MANDATORY_ARGUMENT_NOT_INITIALIZED_2);
		}

		/// <summary>
		/// <see cref="Debug.Fail(string)">Fails</see> if some of the 
		/// command results (properties with <see cref="CommandResultAttribute"/> attribute) have <c>null</c> value
		/// </summary>
		/// <param name="command">checked command</param>
		public void CheckCommandResults(CommandBase command)
		{
            CheckAttributesNotNull(command.GetType(), command, commandResults, typeof(CommandResultAttribute), CommandErrors.CMDERR_RESULT_ARGUMENT_NULL);
		}

		private static void CheckAttributesNotNull(Type type, CommandBase command, IDictionary<Type, Dictionary<string, GetHandler>> getterDictionary, Type attributeType, string errorMsg)
		{
            #if SILVERLIGHT
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.GetCustomAttributes(attributeType, true).Length > 0)
                {
                    object value = property.GetValue(command, null);
                    if (value == null)
                    {
                        Debug.Assert(false, String.Format(errorMsg, command, property.Name));
                    }
                }
            }
            #else 
			if (!getterDictionary.ContainsKey(type))
			{
				getterDictionary[type] = new Dictionary<String, GetHandler>();
				foreach (PropertyInfo property in type.GetProperties())
				{
				    object[] customAttributes = property.GetCustomAttributes(attributeType, true);
				    if (customAttributes.Length > 0 && !((PublicArgumentAttribute)customAttributes[0]).AllowNullInput)
					{
						getterDictionary[type][property.Name] = DynamicMethodCompiler.CreateGetHandler(type, property);
					}
				}
			}

			foreach (KeyValuePair<string, GetHandler> getHandler in getterDictionary[type])
			{
				object value = getHandler.Value(command);
				if (value == null)
				{
					Debug.Assert(false, String.Format(errorMsg, command, getHandler.Key));
				}				
			}
            #endif  
		}
	}
}