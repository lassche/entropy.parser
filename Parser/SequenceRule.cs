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
    /// Matches if all subrules are matched in the exact same order
    /// </summary>
    public class SequenceRule : AbstractRule
    {
        public const string SEQUENCE_RULE_ID = "SequenceRule";

        private bool m_ignoreWhiteSpace;
        // if the rule fails to match after matching N other items
        // a ParserException will be thrown. If the value is -1
        // no exceptions will be thrown
        private int  m_raiseErrorAfter = -1;

        public SequenceRule(  )
        {
            initialize(null);
        }

        public SequenceRule( Object[] rules )
        {
            initialize(rules);
        }

        public void initialize( Object[] rules )
        {
            base.setRuleID( SEQUENCE_RULE_ID );

            m_ignoreWhiteSpace  = true;

            if (rules != null)
            {
                m_subRules          = new IRule[rules.Length];
                
                for (int i = 0; i < rules.Length; ++i)
                {
                    if (rules[i] is string )
                    {
                        m_subRules[i] = new MatchLiteralRule(rules[i] as string);
                    }
                    else if (rules[i] is IRule)
                    {
                        m_subRules[i] = rules[i] as IRule;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        public void setIgnoreWhiteSpace(bool value)
        {
            m_ignoreWhiteSpace = value;
        }

        private int skipWhiteSpace(string text, int index)
        {
            string whitespace = " \n\t\r";
            int    count = 0;

            for (; index + count < text.Length; count++)
            {
                if (whitespace.IndexOf(text[index+count]) == -1)
                {
                    break;
                }
            }

            return count;
        }

        public override ParseTreeNode parse( string text, int index )
        {
            if (m_debug)
            {
                m_debugText = text.Substring(index, text.Length - index);
            }

            ParseTreeNode node = new ParseTreeNode(this, index);
            int result = 0;
            
            for (int i = 0; i < m_subRules.Length; ++i)
            {
                IRule         rule       = m_subRules[i];
                int           testLength = -1;
                ParseTreeNode test       = null;

                if (m_ignoreWhiteSpace)
                {
                    result += skipWhiteSpace(text, index + result);
                }                

                if (rule.includeParseTreeNode())
                {
                    test = rule.parse(text, index + result);
                    testLength = (test == null)? -1 : test.getLength();
                }
                else
                {
                    testLength = rule.isMatch(text, index + result);
                }
                
                if (testLength == -1)
                {
                    if (i >= m_raiseErrorAfter)
                    {
                        string errorMessage = "Failed to match: " + m_ruleID;
                        errorMessage += "\nExpecting: " + rule.getRuleID();

                        throw new ParserException("Failed to match: " + m_ruleID);
                    }

                    return null;
                }
                else
                {
                    result += testLength;

                    if (rule.includeParseTreeNode())
                    {
                        node.addChild( test );
                    }
                }
            }

            node.setLength( result );
            return node;
        }

        public override int isMatch(string text, int index)
        {
            int result = 0;
            
            for (int i = 0; i < m_subRules.Length; ++i)
            {
                int temp = 0;

                if (m_ignoreWhiteSpace)
                {
                    result += skipWhiteSpace(text, index + result);
                }
                
                temp = m_subRules[i].isMatch(text, index + result);

                if (temp == -1)
                {
                    return -1;
                }
                else
                {
                    result += temp;
                }
            }

            if (m_callback != null )
            {
                m_callback(this, text, index, result);
            }

            return result;
        }

        public override void setProperty(string property, string value)
        {
            base.setProperty(property, value);

            if (property.Equals("skipWhiteSpace"))
            {
                m_ignoreWhiteSpace = bool.Parse(value);
            }
        }
    }
}
