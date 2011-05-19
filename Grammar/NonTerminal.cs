namespace EvoX.Model.PSM.Grammar
{
    public class NonTerminal
    {
        public PSMComponent Component { get; private set; }

        public string UniqueName { get; set; }

        public NonTerminal(PSMComponent component)
        {
            Component = component;
        }

        public override string ToString()
        {
            return UniqueName ?? Component.Name.ToUpper();
        }
    }
}