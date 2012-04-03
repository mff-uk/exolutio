using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Exolutio.Model.OCL.Compiler {

    /// <summary>
    /// Unescape character in OCL string literal.
    /// </summary>
    class UnescapeString {
        /// <summary>
        /// Single char escape sequence.
        /// Each row is one replace patern.
        /// First string of row containts value in OCL string literal.
        /// Second string of row containts unescape value.
        /// </summary>
        static readonly string[,] escapeChars = {
                                     {@"\b"/*from*/,"\b"/*to*/},
                                     {@"\t","\t"},
                                     {@"\n","\n"},
                                     {@"\f","\f"},
                                     {@"\r","\r"},
                                     {"\\\"","\""},
                                     {"\\'","'"},
                                     {"\\\\","\\"},
                                     };

        /// <summary>
        /// Regex which matchs escape sequence in string literal.
        /// </summary>
        static readonly Regex escapeRegex;
        /// <summary>
        /// Single escape char mapping
        /// </summary>
        static readonly Dictionary<char, string> charMapping;

        static UnescapeString() {
            // translate to regex format
            int escCharsLen = escapeChars.GetLength(0);
            string[] matchPattern = new string[escCharsLen + 2];
            for (int i = 0; i < escCharsLen; i++) {
                matchPattern[i] = escapeChars[i, 0].Replace(@"\", @"\\");
            }

            string hexDig = "[0-9a-fA-F]"; 
            matchPattern[escCharsLen] = @"\\x"+hexDig+hexDig;
            matchPattern[escCharsLen+1] = @"\\u"+hexDig+hexDig+hexDig+hexDig;

            StringBuilder pattern = new StringBuilder();
            bool isOther = false;
            foreach (string subPatern in matchPattern) {
                if (isOther) {
                    pattern.Append('|');
                }
                pattern.Append(subPatern);
                isOther = true;
            }
            escapeRegex = new Regex(pattern.ToString(), RegexOptions.Compiled);
        
            charMapping = new Dictionary<char, string>();
            for (int i = 0; i < escCharsLen; i++) {
                charMapping.Add(escapeChars[i, 0][1], escapeChars[i, 1]);
            }
        }

        /// <summary>
        /// Unescapes char in OCL string literal.
        /// </summary>
        /// <param name="s">String literal</param>
        /// <returns>Unescaped string</returns>
        public static string Replace(string s) {
            return escapeRegex.Replace(s, UnescapeChar);
        }

        /// <summary>
        /// Translates matched escape sequence
        /// </summary>
        static string UnescapeChar(Match m) {
            if (m.Value.Length < 2) {
                System.Diagnostics.Debug.Fail("Check patterns in UnescapeString class.");
                return "";
            }

            string replaceBy;
            if(charMapping.TryGetValue(m.Value[1],out replaceBy)){
                return replaceBy;
            }

            switch (m.Value[1]) {
                case 'x': // "\ x Hex Hex"
                    if (m.Value.Length != 4) {
                        System.Diagnostics.Debug.Fail("Check pattern for '\\ x Hex Hex' in UnescapeString class.");
                        return "";
                    }
                    return ((char)(GetNumValue(m.Value[2]) * 16 + GetNumValue(m.Value[3]))).ToString();
                case 'u': // "\u Hex Hex Hex Hex"
                    if (m.Value.Length != 6) {
                        System.Diagnostics.Debug.Fail("Check pattern for '\\ u Hex Hex Hex Hex' in UnescapeString class.");
                        return "";
                    }
                    return ((char)(((GetNumValue(m.Value[2]) * 16 + GetNumValue(m.Value[3])) * 16 + GetNumValue(m.Value[4]))*16 + GetNumValue(m.Value[5]))).ToString();
            }
            System.Diagnostics.Debug.Fail("Check replace patterns in UnescapeString class.");
            return "";
        }

        /// <summary>
        /// Convert hex digit in char to int.
        /// </summary>
        /// <param name="c">Hex digit</param>
        /// <returns>Value of hex digit</returns>
        static int GetNumValue(char c) {
            if (c >= '0' && c <= '9') {
                return c - '0';
            }

            if (c >= 'A' && c <= 'F') {
                return c - 'A' + 10;
            }

            if (c >= 'a' && c <= 'f') {
                return c - 'A' + 10;
            }

            System.Diagnostics.Debug.Fail("Check patterns in UnescapeString class.");
            return 0;
        }

    }
}
