using System;

namespace Exolutio.Controller.Commands
{
    public abstract class CommandArgumentAttribute : Attribute
    {
        public string ArgumentName { get; set; }

        public Type ComponentType { get; set; }

	    public object SuggestedValue { get; set; }

        public string ModifiedPropertyName { get; set; }

        protected bool allowNullInput;
        public virtual bool AllowNullInput
        {
            get { return allowNullInput; }
            set { allowNullInput = value; }
        }

        protected bool createEditorHierarchy;
        public virtual bool CreateEditorHierarchy
        {
            get { return createEditorHierarchy; }
            set { createEditorHierarchy = value; }
        }

        protected bool createControlInEditors = true;
        /// <summary>
        /// Default is true
        /// </summary>
        public virtual bool CreateControlInEditors
        {
            get { return createControlInEditors; }
            set { createControlInEditors = value; }
        }
    }

    /// <summary>
	/// Denotes property of a command as a command argument that must be specified before
	/// <see cref="CommandBase.Execute()"/> is called. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PublicArgumentAttribute: CommandArgumentAttribute
	{
        public PublicArgumentAttribute(string argumentName)
        {
            ArgumentName = argumentName;
            createControlInEditors = true; 
        }

        public PublicArgumentAttribute(string argumentName, Type componentType, bool allowNullInput = false, bool createEditorHierarchy = true, bool createControlInEditors = true)
        {
            ArgumentName = argumentName;
            ComponentType = componentType;
            this.allowNullInput= allowNullInput;
            this.createEditorHierarchy = createEditorHierarchy;
            this.createControlInEditors = createControlInEditors; 
        }
	}

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GeneratedIDArgumentAttribute : CommandArgumentAttribute
    {
        public GeneratedIDArgumentAttribute(string argumentName, Type componentType)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }

            ArgumentName = argumentName;
            ComponentType = componentType;
            this.allowNullInput = true;
            this.createEditorHierarchy = false;
            this.createControlInEditors = false; 
        }

        public override bool CreateEditorHierarchy
        {
            get { return base.CreateEditorHierarchy; }
            set
            {
                if (value)
                {
                    throw new ArgumentException("Only 'false' is permitted", "CreateEditorHierarchy");
                }
                base.CreateEditorHierarchy = value;
            }
        }

        public override bool CreateControlInEditors
        {
            get { return base.CreateControlInEditors; }
            set
            {
                if (value)
                {
                    throw new ArgumentException("Only 'false' is permitted", "CreateControlInEditors");
                }
                base.CreateControlInEditors = value;
            }
        }

        public override bool AllowNullInput
        {
            get
            {
                return base.AllowNullInput;
            }
            set
            {
                if (value)
                {
                    throw new ArgumentException("Only 'true' is permitted", "AllowNullInput");
                }
                base.AllowNullInput = value;
            }
        }
    }
}