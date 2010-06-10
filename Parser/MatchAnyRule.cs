//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

namespace entropy.parser
{
    /// <summary>
    /// Rule that matches any character except the end-of-file character.
    /// </summary>
    public class MatchAnyRule : AbstractRule
    {
        public const string MATCH_ANY_RULE_ID = "MatchAnyRule";

        /// <summary>
        /// Default constructor that will set the includeInParseTree
        /// to false and the default id to MATCH_ANY_RULE_ID
        /// </summary>
        public MatchAnyRule()
        {
            base.setRuleID(MATCH_ANY_RULE_ID);
            m_includeInParseTree = false;
        }

        /// <summary>
        /// Returns 1 if the index < text.Length and
        /// -1 otherwise
        /// </summary>
        public override int isMatch(string text, int index)
        {
            if (text.Length > index )
            {
                if (m_callback != null)
                {
                    m_callback(this, text, index, 1);
                }
                
                return 1;
            }

            return -1;
        }

        /// <summary>
        /// Returns ParseTreeNode if the index < text.Length and
        /// null otherwise
        /// </summary>
        public override ParseTreeNode parse( string text, int index )
        {
            return (text.Length > index) ? new ParseTreeNode( this, index, 1 ) : null;
        }
    }
}
