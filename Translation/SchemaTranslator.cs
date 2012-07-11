using System;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Translation
{
    /// <summary>
    /// Abstract class from which classes that translate PSM schema
    /// into string representation are derived.  
    /// </summary>
    /// <remarks>
    /// Bodies of all function except <see cref="TranslateContentModel"/> and 
    /// <see cref="TranslateAssociation"/> are empty and up to the 
    /// derived classes to override. 
    /// </remarks>
    /// <typeparam name="TContext">The type of the context. This type is up to derived
    /// classes to define, it should encapsulate the information that is 
    /// passed among translation functions. The meaning of the context is up 
    /// to derived classes to define. </typeparam>
    /// <typeparam name="TTypeIdentifier">The type of of identifier used for translated classes.</typeparam>
    /// <typeparam name="TTranslationResult">type of the translation result</typeparam>
    public abstract class SchemaTranslator<TContext, TTypeIdentifier, TTranslationResult>
	{
		/// <summary>
		/// The translated diagram
		/// </summary>
		public PSMSchema Schema { get; protected set; }

		/// <summary>
		/// Log where errors and warnings are written during translation.
		/// </summary>
		public Log Log { get; private set; }

		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="SchemaTranslator{TContext,TTypeIdentifier,TTranslationResult}"/> class.
		/// </summary>
		protected SchemaTranslator()
		{
			Log = new Log();
		}

		/// <summary>
		/// Translates the specified schema into string. The semantics of the return 
		/// value is up to the derived classes to define. 
		/// </summary>
		/// <param name="schema">The translated schema.</param>
		/// <returns>string representation of the <paramref name="schema"/></returns>
        public abstract TTranslationResult Translate(PSMSchema schema, string schemaLocation = null);

		/// <summary>
		/// Translates class. 
		/// </summary>
		/// <param name="psmClass">The PSM class.</param>
		/// <param name="context">The translation context.</param>
		/// <returns></returns>
		protected virtual TTypeIdentifier TranslateClass(PSMClass psmClass, TContext context) { return default(TTypeIdentifier); }

	    #region generalizations not supported yet

        ///// <summary>
        ///// Translates the specializations of <paramref name="generalClass"/>. Calls
        ///// <see cref="TranslateSpecialization"/> for each specialization of <paramref name="generalClass"/>.
        ///// </summary>
        ///// <seealso cref="Class.Specifications"/>
        ///// <param name="generalClass">The general class.</param>
        ///// <param name="generalTypeName">Name of the general type.</param>
        //protected void TranslateSpecializations(PSMClass generalClass, TypeIdentifier generalTypeName)
        //{
        //    foreach (Generalization specialization in generalClass.Specifications)
        //    {
        //        TranslateSpecialization(specialization, generalTypeName);
        //    }
        //}

        ///// <summary>
        ///// Translates the <paramref name="specialization"/>.
        ///// </summary>
        ///// <remarks>
        ///// Could call <see cref="TranslateClass"/> for <paramref name="specialization"/>'s 
        ///// <see cref="Generalization.Specific"/> class to translate the specializing class the 
        ///// same way general classes are translated. If this is not the desired behaviour, 
        ///// all components of the specializing clases should be translated via 
        ///// <see cref="TranslateSubordinateComponent"/> call. 
        ///// </remarks>
        ///// <param name="specialization">The translated specialization.</param>
        ///// <param name="generalTypeName">Identifier of the general type.</param>
        //protected virtual void TranslateSpecialization(Generalization specialization, TypeIdentifier generalTypeName) { }

	    #endregion

	    #region content model

	    /// <summary>
	    /// Translates the content model component.
	    /// </summary>
	    /// <remarks>If not redefined, the default implementation calls one of the following according
	    /// to the type of the component: <see cref="TranslateSequenceContentModel"/>, 
	    /// <see cref="TranslateChoiceContentModel"/> or <see cref="TranslateSetContentModel"/>.
	    /// </remarks>
	    /// <param name="contentModel">The component.</param>
	    /// <param name="context">The translation context.</param>
	    protected void TranslateContentModel(PSMContentModel contentModel, TContext context)
	    {
	        switch (contentModel.Type)
	        {
	            case PSMContentModelType.Sequence:
	                TranslateSequenceContentModel(contentModel, context);
	                break;
	            case PSMContentModelType.Choice:
	                TranslateChoiceContentModel(contentModel, context);
	                break;
	            case PSMContentModelType.Set:
	                TranslateSetContentModel(contentModel, context);
	                break;
	        }
	    }

	    /// <summary>
		/// Translates the 'choice' content model.
		/// </summary>
		/// <param name="choiceModel">The 'choice' content model.</param>
		/// <param name="context">The translation context.</param>
		/// <seealso cref="PSMContentModelType.Choice"/>
		protected virtual void TranslateChoiceContentModel(PSMContentModel choiceModel, TContext context) { }

		/// <summary>
		/// Translates the 'sequence' content model.
		/// </summary>
		/// <param name="sequenceModel">The 'sequence' content model.</param>
		/// <param name="context">The translation context.</param>
        /// <seealso cref="PSMContentModelType.Choice"/>
        protected virtual void TranslateSequenceContentModel(PSMContentModel sequenceModel, TContext context) { }

        /// <summary>
        /// Translates the 'set' content model.
        /// </summary>
        /// <param name="setModel">The 'set' content model.</param>
        /// <param name="context">The translation context.</param>
        /// <seealso cref="PSMContentModelType.Set"/>
        protected virtual void TranslateSetContentModel(PSMContentModel setModel, TContext context) { }

	    #endregion

	    /// <summary>
	    /// Translates the association child. 
	    /// </summary>
        /// <param name="associationChild">The association child.</param>
	    /// <param name="context">The translation context.</param>
	    protected virtual void TranslateAssociationChild(PSMAssociationMember associationChild, TContext context)
	    {
	        if (associationChild is PSMSchemaClass)
	        {
	            TranslateSchemaClass((PSMSchemaClass) associationChild, context);
	        }
            else if (associationChild is PSMClass)
            {
                TranslateClass((PSMClass) associationChild, context);
            }
            else if (associationChild is PSMContentModel)
            {
                TranslateContentModel((PSMContentModel) associationChild, context);
            }
	    }

        /// <summary>
        /// Translates the schema class.
        /// </summary>
        /// <param name="psmSchemaClass">The PSM schema class.</param>
        /// <param name="context">The translation context.</param>
        public virtual void TranslateSchemaClass(PSMSchemaClass psmSchemaClass, TContext context)
        {
            
        }

        /// <summary>
		/// Translates the association. The default implementation calls
		/// <see cref="TranslateAssociationChild"/> on <see cref="PSMAssociation.Child"/>
		/// of <paramref name="association"/>, but this behavior could be redefined in 
		/// derived classes. 
		/// </summary>
		/// <param name="association">The association.</param>
		/// <param name="context">The translation context.</param>
		/// <remarks>Should probably call <see cref="TranslateAssociationChild"/> on  
		/// <see cref="PSMAssociation.Child"/> of <paramref name="association"/> if redefined. 
		/// </remarks>
		protected virtual void TranslateAssociation(PSMAssociation association, TContext context)
		{
			TranslateAssociationChild(association.Child, context);
		}

        /// <summary>
        /// Translates the PSM attribute.
        /// </summary>
        /// <param name="attribute">The PSM attribute.</param>
        /// <param name="context">The translation context.</param>
        protected virtual void TranslateAttribute(PSMAttribute attribute, TContext context)
        {
            
        }
	}
}