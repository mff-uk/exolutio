using System;

namespace Exolutio.Model
{
    public interface IHasCardinality
    {
        uint Lower { get; set; }

        UnlimitedInt Upper { get; set; }

        string CardinalityString { get; }
    }
    
    public static class IHasCardinalityExt
    {
        public static string GetCardinalityString(this IHasCardinality cardinalityElement)
        {
            if (cardinalityElement.Lower != cardinalityElement.Upper)
                return String.Format("{0}..{1}", cardinalityElement.Lower, cardinalityElement.Upper);
            else
                return cardinalityElement.Lower.ToString();
        }

        public static string GetCardinalityString(uint lower, UnlimitedInt upper)
        {
            if (lower != upper)
                return String.Format("{0}..{1}", lower, upper);
            else
                return lower.ToString();
        }

        public static bool HasNondefaultCardinality(this IHasCardinality cardinalityElement)
        {
            return cardinalityElement.Lower != 1 || cardinalityElement.Upper != 1;
        }

        public static UnlimitedInt ParseUnlimitedNatural(string value)
		{
			if (value == "*")
				return UnlimitedInt.Infinity;
			uint result;

			if (!uint.TryParse(value, out result))
			{
				throw new FormatException(string.Format("Wrong cardinality format: '{0}'.", value));
			}

			return result;
		}

		public static uint ParseUint(string value)
		{
			uint result;
			if (!uint.TryParse(value, out result))
			{
				throw new FormatException(string.Format("Wrong cardinality format: '{0}'.", value));
			}

			return result;
		}

		public static bool ParseMultiplicityString(string newCardinality, out uint lower, out UnlimitedInt upper)
		{
			try
			{
				if (newCardinality.Contains(".."))
				{
					int pos = newCardinality.IndexOf("..");
					lower = ParseUint(newCardinality.Substring(0, pos));
					upper = ParseUnlimitedNatural(newCardinality.Substring(pos + 2));
				}
				else
				{
					lower = uint.Parse(newCardinality);
					upper = (UnlimitedInt)lower;
				}
				return true; 
			}
            catch(FormatException)
            {
                lower = 0;
                upper = 0;
                return false;
            }
		}

        public static bool IsMultiplicityStringValid(string newCardinality)
        {
            uint? lower;
            UnlimitedInt upper;

            if (newCardinality.Contains(".."))
            {
                int pos = newCardinality.IndexOf("..");
                lower = ParseUint(newCardinality.Substring(0, pos));
                upper = ParseUnlimitedNatural(newCardinality.Substring(pos + 2));
            }
            else
            {
                lower = uint.Parse(newCardinality);
                upper = (UnlimitedInt)lower;
            }
            return IsMultiplicityValid(lower, upper); 
        }

        public static bool IsMultiplicityValid(uint? lower, UnlimitedInt upper)
        {
            return !lower.HasValue || lower <= upper;
        }
	}
}