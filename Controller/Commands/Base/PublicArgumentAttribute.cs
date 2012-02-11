using System;

namespace Exolutio.Controller.Commands
{
	/// <summary>
	/// Denotes property of a command as a command argument that must be specified before
	/// <see cref="CommandBase.Execute()"/> is called. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PublicArgumentAttribute: Attribute
	{
        public string ArgumentName { get; set; }

        public Type ComponentType { get; set; }

	    public object SuggestedValue { get; set; }

        public string ModifiedPropertyName { get; set; }

	    private bool allowNullInput;
	    public bool AllowNullInput
	    {
	        get { return allowNullInput; }
	        set { allowNullInput = value; }
	    }

	    public bool CreateEditorHierarchy { get; set; }

	    public PublicArgumentAttribute(string argumentName)
	    {
	        ArgumentName = argumentName;
	    }

        public PublicArgumentAttribute(string argumentName, Type componentType, bool allowNullInput = false, bool createEditorHierarchy = true)
	    {
	        ArgumentName = argumentName;
	        ComponentType = componentType;
	        AllowNullInput = allowNullInput;
	        CreateEditorHierarchy = createEditorHierarchy;
	    }
	}
}