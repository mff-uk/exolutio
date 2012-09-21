using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.PSM.XMLValidation
{
    /**
     * Trida obsahuje pomocne metody pro praci s listy a mnozinami. 
     **/
    class ListUtils
    {
        /**
         * Odebere prvnich n prvku z list. 
         **/
        public static List<string> removeFromStartOfList(List<string> list, int n)
        {
            List<string> result = new List<string>();
            for (int i = n; i < list.Count; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }

        /**
         * Presune v listu originalList prvky od startPosition v poctu range na pocatek listu. 
         **/
        public static List<String> setOnStartPosition(List<String> originalList, int startPosition, int range)
        {
            List<String> result = new List<string>();
            for (int i = startPosition; i < startPosition + range; i++)
            {
                result.Add(originalList[i]);
            }
            for (int i = 0; i < originalList.Count; i++)
            {
                if (i < startPosition || i >= startPosition + range)
                    result.Add(originalList[i]);
            }

            return result;
        }

        /**
         *  Vrati prvnich p prvku z listu. 
         **/
        public static List<String> getFirstFromList(List<String> list, int p)
        {
            List<String> result = new List<String>();
            for (int i = 0; i < p; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }

        /**
         *  Zkopiruje prvky z originalList do noveho listu. 
         **/
        public static List<T> copy<T>(List<T> originalList) {
            List<T> result = new List<T>();
            result.AddRange(originalList);
            return result;
        }

        /**
         *  Zkopiruje prvky z originalSet do nove mnoziny. 
         **/
        public static HashSet<T> copy<T>(HashSet<T> originalSet)
        {
            HashSet<T> result = new HashSet<T>();
            result.UnionWith(originalSet);
            return result;
        } 
    }
}
