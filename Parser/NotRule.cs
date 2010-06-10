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
    /// Matches if the given rule does not match
    /// </summary>
    public class NotRule : AbstractRule
    {
        public const string NOT_RULE_ID = "NotRule";

        private IRule m_rule;

        /// <summary>
        /// Constructor that takes a rule ot negate, rule cannot be null.
        /// The ruleid is set to NOT_RULE_ID
        /// </summary>
        /// <param name="aRule"></param>
        public NotRule (IRule aRule)
        {
            Debug.Assert( aRule != null );

            m_rule = aRule;
            base.setRuleID(NOT_RULE_ID);
        }

        /// <summary>
        /// Returns 0 result if the rule doesn't match
        /// -1 otherwise
        /// </summary>
        public override int isMatch(string text, int index)
        {
            return m_rule.isMatch(text, index) >= 0 ? -1 : 0;
        }

        /// <summary>
        /// Returns a ParseNode result if the rule doesn't match
        /// null otherwise
        /// </summary>
        public override ParseTreeNode parse( string text, int index )
        {
            return m_rule.parse(text, index) == null ? new ParseTreeNode(this, index, 0) : null;
        }
    }
}
