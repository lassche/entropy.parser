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
    /// References to another rule specified in a ruleparser
    /// </summary>
    public class ReferenceRule : AbstractRule
    {
        private string   m_reference;
        private RuleParser m_util;

        public ReferenceRule( string reference, RuleParser util)
        {
            m_reference = reference;
            m_util      = util;
            base.setRuleID("reference");
        }

        public override bool includeParseTreeNode()
        {
            return m_util.findRule(m_reference).includeParseTreeNode();
        }

        public override int isMatch(string text, int index)
        {
            int result = m_util.findRule(m_reference).isMatch(text, index);

            if (result != -1 && m_callback != null)
            {
                m_callback(this, text, index, result);
            }

            return result;
        }

        public override ParseTreeNode parse( string text, int index )
        {
            return m_util.findRule(m_reference).parse(text, index);
        }
   }
}
