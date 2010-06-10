//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

using System.Diagnostics;

namespace entropy.parser
{
    /// <summary>
    /// Rule that matches the provided characters
    /// </summary>
    public class MatchCharRule : AbstractRule
    {
        public const string MATCH_ANY_RULE_ID = "MatchCharRule";

        private string m_chars = "";

        /// <summary>
        /// default constructor that sets the matched character to
        /// character in the given string. The string cannot be null
        /// and must have the length > 0
        /// </summary>
        public MatchCharRule( string chars )
        {
            Debug.Assert( chars != null && chars.Length > 0 );

            base.setRuleID(MATCH_ANY_RULE_ID);
            
            m_chars              = chars;
            m_includeInParseTree = false;
        }

        /// <summary>
        /// Returns 1 character in the text at the given index 
        /// is matching any of the characters in this rule.
        /// Returns -1 otherwise
        /// </summary>
        public override int isMatch(string text, int index)
        {
            if (text.Length > index)
            {
                int indexOf = m_chars.IndexOf(text[index]);

                if (indexOf >= 0)
                {
                    if (m_callback != null)
                    {
                        m_callback(this, text, index, 1);
                    }

                    return 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns ParseTreeNode if the character in the text at the given index 
        /// is matching any of the characters in this rule.
        /// null otherwise
        /// </summary>
        public override ParseTreeNode parse( string text, int index )
        {
            if (text.Length > index)
            {
                if (m_chars.IndexOf(text[index]) >= 0)
                {
                    return new ParseTreeNode(this, index, 1);
                }
            }

            return null;
        }
    }
}
