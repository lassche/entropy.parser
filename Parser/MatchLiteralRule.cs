//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

using System;
using System.Diagnostics;

namespace entropy.parser
{
    /// <summary>
    /// Rule that matches if the input matches a string literal
    /// </summary>
    public class MatchLiteralRule : AbstractRule
    {
        public const string MATCH_LITERAL_RULE_ID = "MatchLiteralRule";

        private string             m_literal;

        /// <summary>
        /// default constructor sets the ID to MATCH_LITERAL_RULE_ID,
        /// provided literal cannot be null 
        /// </summary>
        public MatchLiteralRule( string literal )
        {
            Debug.Assert( literal != null );

            m_literal   = literal;
            m_callback  = null;
            base.setRuleID(MATCH_LITERAL_RULE_ID);
        }

        /// <summary>
        /// Sets the literal that is used to match this rule.
        /// Literal cannot be null and has to have length > 0
        /// </summary>
        public void setLiteral( string literal )
        {
            Debug.Assert( literal != null && literal.Length > 0);

            m_literal = literal;
        }

        /// <summary>
        /// Returns the length of the literal if the text at the
        /// given index matches the rules' literal
        /// </summary>
        public override int isMatch(string text, int index)
        {
            if (text.Length > index)
            {
                int result = 0;

                for (result = 0; result < m_literal.Length; ++result)
                {
                    if (  (text.Length < index + result)
                       || (m_literal[result] != text[index + result]))
                    {
                        result = -1;
                        break;
                    }
                }
                
                if (m_callback != null && result >= 0)
                {
                    m_callback(this, text, index, result);
                }

                return result;
            }

            return -1;
        }

        /// <summary>
        /// Returns ParseTreeNode if the text at the
        /// given index matches the rules' literal, null otherwise
        /// </summary>
        public override ParseTreeNode parse( string text, int index )
        {
            if (text.Length > index)
            {
                int result = 0;

                for (result = 0; result < m_literal.Length; ++result)
                {
                    if (  (text.Length < index + result)
                       || (m_literal[result] != text[index + result]))
                    {
                        result = -1;
                        break;
                    }
                }
                
                if (result != -1)
                {
                    return new ParseTreeNode(this, index, result);
                }
            }

            return null;
        }
   }
}
