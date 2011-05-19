namespace EvoX.Model.PSM.Grammar
{
    public class Terminal
    {
        public string Term { get; private set; }
        
        public Terminal(string term)
        {
            Term = term;
        }

        public override string ToString()
        {
            return Term;
        }
    }
}