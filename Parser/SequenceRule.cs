//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

using System;

namespace entropy.parser
{
    /// <summary>
    /// Matches if all subrules are matched in the exact same order
    /// </summary>
    public class SequenceRule : AbstractRule
    {
        public const string SEQUENCE_RULE_ID = "SequenceRule";

        private Object[]         m_rules;
        private bool             m_ignoreWhiteSpace;
        private MatchLiteralRule m_literalHelper;

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
            m_rules = rules;
            m_ignoreWhiteSpace  = true;
            
            if ( m_literalHelper == null )
            {
                m_literalHelper     = new MatchLiteralRule("");
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
            
            for (int i = 0; i < m_rules.Length; ++i)
            {
                IRule rule = null;

                if (m_ignoreWhiteSpace)
                {
                    result += skipWhiteSpace(text, index + result);
                }                

                if (m_rules[i] is string)
                {
                    m_literalHelper.setLiteral( (string) m_rules[i] );
                    rule = m_literalHelper;
                }
                else
                {
                    rule = (IRule) m_rules[i];
                }

                ParseTreeNode test = rule.parse(text, index + result);

                if (test == null)
                {
                    return null;
                }
                else
                {
                    result += test.getLength();

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
            
            for (int i = 0; i < m_rules.Length; ++i)
            {
                int temp = 0;

                if (m_ignoreWhiteSpace)
                {
                    result += skipWhiteSpace(text, index + result);
                }
                
                if (m_rules[i] is string)
                {
                    m_literalHelper.setLiteral( (string) m_rules[i] );
                    temp = m_literalHelper.isMatch(text, index + result);
                }
                else
                {
                    temp = ((IRule)m_rules[i]).isMatch(text, index + result);
                }

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
