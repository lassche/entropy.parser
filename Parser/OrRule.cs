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
    /// Matches if any of the given subrules matches
    /// </summary>
    public class OrRule : AbstractRule
    {
        private Object[]         m_ruleSet;
        private MatchLiteralRule m_literalHelper;

        public OrRule( Object left, Object right)
        {
            m_ruleSet       = new Object[]{left, right};
            m_literalHelper = new MatchLiteralRule("");
            base.setRuleID("or");
        }

        public OrRule( Object[] ruleSet)
        {
            m_ruleSet = ruleSet;
            base.setRuleID("or");
        }

        public override int isMatch(string text, int index)
        {
            for (int i = 0; i < m_ruleSet.Length; ++i)
            {
                IRule rule = null;

                if (m_ruleSet[i] is string)
                {
                    rule = m_literalHelper;
                    m_literalHelper.setLiteral( (string) m_ruleSet[i] );
                }
                else
                {
                    rule = (IRule) m_ruleSet[i];
                }

                
                int result = rule.isMatch(text, index);

                if (result >= 0)
                {
                    if (m_callback != null)
                    {
                        m_callback(this, text, index, result);
                    }

                    return result;
                }
            }

            return -1;
        }

        public override ParseTreeNode parse( string text, int index )
        {
            ParseTreeNode node = new ParseTreeNode(this, index);

            for (int i = 0; i < m_ruleSet.Length; ++i)
            {
                IRule rule = null;

                if (m_ruleSet[i] is string)
                {
                    rule = m_literalHelper;
                    m_literalHelper.setLiteral( (string) m_ruleSet[i] );
                }
                else
                {
                    rule = (IRule) m_ruleSet[i];
                }

                
                ParseTreeNode result = rule.parse(text, index);
                
                if (result != null)
                {
                    if (rule.includeParseTreeNode())
                    {
                        node.addChild(result);
                    }

                    node.setLength(result.getLength());
                    return node;
                }
            }

            return null;
        }

    }
}
