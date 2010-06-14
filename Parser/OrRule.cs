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
    /// Matches if any of the given subrules matches
    /// </summary>
    public class OrRule : AbstractRule
    {
        public const string OR_RULE_ID = "SequenceRule";

        public OrRule( Object left, Object right)
        {
            Debug.Assert(left != null);
            Debug.Assert(left is string || left is IRule);
            Debug.Assert(right != null);
            Debug.Assert(right is string || right is IRule);
            
            m_subRules      = new IRule[2];
            
            addRule(left, 0);
            addRule(right, 1);
            
            base.setRuleID(OR_RULE_ID);
        }

        public OrRule( Object[] ruleSet)
        {
            Debug.Assert(ruleSet != null);
            Debug.Assert(ruleSet.Length > 0);

            m_subRules      = new IRule[ruleSet.Length];
            
            for (int i = 0; i < ruleSet.Length; ++i)
            {
                addRule(ruleSet[i], i);
            }

            base.setRuleID(OR_RULE_ID);
        }

        public void addRule(Object rule, int index)
        {
            Debug.Assert(rule != null);
            Debug.Assert(rule is string || rule is IRule);

            IRule addedRule  = rule is IRule 
                             ? rule as IRule 
                             : new MatchLiteralRule(rule as string); 

            m_subRules[index] = addedRule;
        }

        public override int isMatch(string text, int index)
        {
            for (int i = 0; i < m_subRules.Length; ++i)
            {
                IRule rule = m_subRules[i];
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
            for (int i = 0; i < m_subRules.Length; ++i)
            {
                IRule rule = m_subRules[i];

                if (rule.includeParseTreeNode())
                {
                    ParseTreeNode result = rule.parse(text, index);
                    
                    if (result != null)
                    {
                        ParseTreeNode node = new ParseTreeNode(this, index);
                        node.addChild(result);
                        node.setLength(result.getLength());
                        return node;
                    }
                }
                else
                {
                    int length = rule.isMatch(text, index);

                    if (length >= 0)
                    {
                        ParseTreeNode node = new ParseTreeNode(this, index);
                        node.setLength(length);
                        return node;
                    }
                }
            }

            return null;
        }

    }
}
