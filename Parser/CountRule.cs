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
    /// Implements a rule that has to match a given rule a certain number of times.
    /// If the min or max count is set to -1, the minimum number of matches
    /// is irrelevant.
    /// </summary>
    public class CountRule : AbstractRule
    {
        public const string COUNT_RULE_ID = "CountRule";

        private IRule m_rule;
        private int   m_min;
        private int   m_max;

        /// <summary>
        /// Default constructor, sets the min and max counts to -1
        /// and the ruleID to COUNT_RULE_ID
        /// </summary>
        public CountRule ()
        {
            base.setRuleID(COUNT_RULE_ID);

            m_rule = null;
            m_min  = -1;
            m_max  = -1;
        }

        /// <summary>
        /// Constuctor taking a string and turning it in a 
        /// Match Literal rule. Rule string cannot be null.
        /// The ruleID will be set to COUNT_RULE_ID.
        /// </summary>
        public CountRule ( string rule, int min, int max)
        {
            Debug.Assert(rule != null);

            base.setRuleID(COUNT_RULE_ID);
            initialize( new MatchLiteralRule( rule ), min, max );
        }

        /// <summary>
        /// Constuctor taking a rule and min and max count. Rule cannot be null.
        /// The ruleID will be set to COUNT_RULE_ID.
        /// </summary>
        public CountRule ( IRule rule, int min, int max)
        {
            Debug.Assert(rule != null);

            base.setRuleID(COUNT_RULE_ID);
            initialize( rule, min, max );
        }

        /// <summary>
        /// Initializes the count rule. Rule cannot be null.
        /// </summary>
        public void initialize(IRule rule, int min, int max)
        {
            Debug.Assert(rule != null);

            m_rule = rule;
            m_min  = min;
            m_max  = max;

        }

        /// <summary>
        /// Tries to parse the given text starting at the given index.
        /// IF succesfull a parsenode will be returned otherwise
        /// </summary>
        public override ParseTreeNode parse( string text, int index )
        {
            if (m_debug)
            {
                m_debugText = text.Substring(index, text.Length - index);
            }

            ParseTreeNode result = new ParseTreeNode(this, index, 0);
            ParseTreeNode temp   = null;
            
            int count  = 0;
            
            do
            {
                temp = m_rule.parse( text, index + result.getLength() );
                
                if ( temp != null )
                {
                    if (m_rule.includeParseTreeNode())
                    {
                        result.addChild( temp );
                    }

                    result.addLength ( temp.getLength() );
                    count++;
                }
            }
            while (temp != null);
            
            if ((m_min < 0  ||count >= m_min) 
                && (m_max < 0 || count <= m_max))
            {
                return result;
            }
            
            return null;
        }

        /// <summary>
        /// Scans the text from the given index, if the conditions
        /// of the rule are met the length of the match is returned,
        /// otherwise -1 is returned
        /// </summary>
        public override int isMatch(string text, int index)
        {
            int count  = 0;
            int temp   = 0;
            int result = 0;

            do
            {
                temp = m_rule.isMatch(text, index + result);

                if (temp >= 0)
                {
                    result += temp;
                    count++;
                }
            }
            while (temp >= 0);
            
            if ((m_min < 0  ||count >= m_min) 
                && (m_max < 0 || count <= m_max))
            {
                if (m_callback != null)
                {
                    m_callback(this, text, index, result);
                }

                return result;
            }
            
            return -1;
        }
    }
}
