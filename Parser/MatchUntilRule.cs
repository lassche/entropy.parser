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
    /// Rule that matches when a certain termination condition has been reached.
    /// If that condition has not been reached an intermediate conditon must be
    /// met.
    /// </summary>
    public class MatchUntilRule : AbstractRule
    {
        public const string MATCH_UNTIL_RULE_ID = "MatchUntilRule";

        private IRule m_terminationRule;
        private IRule m_remainderRule;
        private bool  m_inclusive;

        /// <summary>
        /// Default constructor, sets the termination and remainderrule.
        /// Both rules cannot be null. The termination rule string will be 
        /// created as a matchliteral rule.
        /// 
        /// The "inclusive" setting will be set to false
        /// </summary>
        public MatchUntilRule(string terminationRule, IRule remainderRule)
        {
            Debug.Assert( terminationRule != null );
            Debug.Assert( terminationRule.Length > 0 );
            Debug.Assert( remainderRule != null );

            m_terminationRule = new MatchLiteralRule(terminationRule);
            m_remainderRule   = remainderRule;
            m_inclusive       = false;
            base.setRuleID(MATCH_UNTIL_RULE_ID );
        }

        /// <summary>
        /// Default constructor, sets the termination and remainderrule.
        /// Both rules cannot be null. 
        /// The "inclusive" setting will be set to false
        /// </summary>
        public MatchUntilRule(IRule terminationRule, IRule remainderRule)
        {
            Debug.Assert( terminationRule != null );
            Debug.Assert( remainderRule != null );

            m_terminationRule = terminationRule;
            m_remainderRule   = remainderRule;
            m_inclusive       = false;
            
            base.setRuleID(MATCH_UNTIL_RULE_ID );
        }

        /// <summary>
        /// Default constructor, sets the termination and remainderrule.
        /// Both rules cannot be null. 
        /// The "inclusive" setting will be set to false
        /// </summary>
        public MatchUntilRule(IRule terminationRule, IRule remainderRule, bool isInclusive)
        {
            Debug.Assert( terminationRule != null );
            Debug.Assert( remainderRule != null );

            m_terminationRule = terminationRule;
            m_remainderRule   = remainderRule;
            m_inclusive       = isInclusive;
            
            base.setRuleID(MATCH_UNTIL_RULE_ID );
        }

        /// <summary>
        /// Matches if the text at the given index matches the remainder
        /// rule followed by an eventual termination match or termination rule.
        /// If the rule is set to inclusive the length will be the remainder
        /// rule match(es) including the termination rule match otherwise it
        /// will just be the the remainder matches.
        /// 
        /// Throws a parser exception if the remainder rule yields 0
        /// </summary>
        /// 
        public override int isMatch(string text, int index)
        {
            Debug.Assert( text != null );
            Debug.Assert( index >= 0 );

            int result = -1;
            int temp   = 0;

            while ( true )
            {
                int test = m_terminationRule.isMatch(text, index+temp);

                if (test >= 0)
                {
                    if (m_inclusive)
                    {
                        result = temp + test;
                    }
                    else
                    {
                        result = temp;
                    }

                    break;
                }
                else
                {
                    test = m_remainderRule.isMatch(text, index+temp);

                    if (test > 0)
                    {
                        temp += test;
                    }
                    else if (test < 0)
                    {
                        result = -1;
                        break;
                    }
                    else
                    {
                        throw new ParserException("Fatal error: inconclusive MatchUntilRule ( " + getRuleID() + " ) ");
                    }
                }
            }
            
            if (m_callback != null && result >= 0)
            {
                m_callback(this, text, index, result);
            }

            return result;
        }

        /// <summary>
        /// Matches if the text at the given index matches the remainder
        /// rule followed by an eventual termination match or termination rule.
        /// If the rule is set to inclusive the length will be the remainder
        /// rule match(es) including the termination rule match otherwise it
        /// will just be the the remainder matches.
        /// </summary>
        public override ParseTreeNode parse( string text, int index )
        {
            Debug.Assert( text != null );
            Debug.Assert( index >= 0 );

            ParseTreeNode node = new ParseTreeNode(this, index);
            
            int result     = -1;
            int temp       = 0;
            int testLength = -1;

            while ( true )
            {
                ParseTreeNode test = null;

                if (m_terminationRule.includeParseTreeNode() && m_inclusive)
                {
                    test = m_terminationRule.parse(text, index+temp);
                    testLength = test.getLength();
                }
                else
                {
                    testLength = m_terminationRule.isMatch(text, index+temp);
                }

                if (testLength >= 0)
                {
                    if (m_inclusive)
                    {
                        result = temp + testLength;
                        if (m_terminationRule.includeParseTreeNode())
                        {
                            node.addChild( test );
                        }
                    }
                    else
                    {
                        result = temp;
                    }

                    break;
                }
                else
                {
                    if (m_remainderRule.includeParseTreeNode())
                    {
                        test = m_remainderRule.parse(text, index+temp);
                        testLength = test != null ? test.getLength() : -1;
                    }
                    else
                    {
                        testLength = m_remainderRule.isMatch(text, index + temp);
                    }

                    if (testLength == 0)
                    {
                        throw new ParserException("Fatal error: inconclusive MatchUntilRule ( " + getRuleID() + " ) ");
                    }
                    else  if (testLength > 0)
                    {
                        temp += testLength;

                        if (m_remainderRule.includeParseTreeNode())
                        {
                            node.addChild( test );
                        }
                    }
                    else
                    {
                        result = -1;
                        break;
                    }
                }
            }
            
            if (result >= 0)
            {
                node.setLength( result );
                return node;
            }

            return null;
        }
    }
}
