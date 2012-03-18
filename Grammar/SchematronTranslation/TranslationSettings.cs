namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class TranslationSettings
    {
        public bool SchemaAware { get; set; }

        public bool Functional { get; set; }

        public bool Retranslation { get; set; }

        public SubexpressionTranslations SubexpressionTranslations { get; private set; }

        public TranslationSettings()
        {
            SubexpressionTranslations = new SubexpressionTranslations();
        }

        public TranslationSettings(bool schemaAware, bool functional) : this()
        {
            SchemaAware = schemaAware;
            Functional = functional;
        }
    }
}