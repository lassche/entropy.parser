
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
    /// Rule that matches if the end of a file has been reached
    /// </summary>
    public class MatchEOFRule : AbstractRule
    {
        public const string MATCH_EOF_RULE_ID = "MatchEOFRule";

        /// <summary>
        /// default constructor sets the ID to MATCH_EOF_RULE_ID
        /// </summary>
        public MatchEOFRule()
        {
            base.setRuleID(MATCH_EOF_RULE_ID);
        }

        /// <summary>
        /// Returns 0 if the end of file is reached -1 otherwise
        /// </summary>
        public override int isMatch(string text, int index)
        {
            if (text.Length <= index)
            {
                if (m_callback != null)
                {
                    m_callback(this, text, index, 0);
                }
                return 0;
            }

            return -1;
        }
        
        /// <summary>
        /// Returns ParseTreeNode if the end of file has been reached,
        /// null otherwise
        /// </summary>
        
        public override ParseTreeNode parse( string text, int index )
        {
            return (text.Length <= index) ? new ParseTreeNode(this, index, 0) 
                                          : null;
        }
    }
}
