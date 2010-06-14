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
    /// Abstract base implementing the most basic functionality of
    /// all rules.
    /// </summary>
    public abstract class AbstractRule : IRule
    {
        public const string INCLUDE_IN_PARSE_TREE_PROPERTY = "includeInParseTree";
        public const string ABSTRACT_RULE_ID               = "AbstractRule";

        protected ruleMatchCallback m_callback;
        protected string            m_ruleID;
        protected bool              m_includeInParseTree;
        protected IRule[]           m_subRules;

        protected bool              m_debug     = false;
        protected string            m_debugText = null;

        /// <summary>
        /// Default constructor, sets the includeParseTreeNode to true
        /// and the ruleID to ABSTRACT_RULE_ID
        /// </summary>
        public AbstractRule()
        {
            m_includeInParseTree = true;
            m_ruleID             = ABSTRACT_RULE_ID;
            m_callback           = null;
            m_subRules           = null;
        }

        /// <summary>
        /// unimplemented - has to be implemented by subclasses
        /// </summary>
        abstract public int isMatch(string text, int index);

        /// <summary>
        /// unimplemented - has to be implemented by subclasses
        /// </summary>
        abstract public ParseTreeNode parse( string text, int index );

        /// <summary>
        /// Sets the callback, can be null.
        /// </summary>
        public void setMatchCallback(ruleMatchCallback callback)
        {
            m_callback           = callback;
        }

        /// <summary>
        /// Sets the id of this node, can be null.
        /// </summary>
        public void setRuleID( string name )
        {
            m_ruleID = name;
        }

        /// <summary>
        /// Gets the id of this node, can be null.
        /// </summary>
        public string getRuleID() 
        {
            return m_ruleID;
        }

        /// <summary>
        /// Sets a property to a value. The Abstract rule supports
        /// the AbstractRule.INCLUDE_IN_PARSE_TREE_PROPERTY.
        /// Both property and value cannot be null
        /// </summary>
        public virtual void setProperty(string property, string value)
        {
            Debug.Assert( property != null );
            Debug.Assert( value != null );

            if (property.Equals(INCLUDE_IN_PARSE_TREE_PROPERTY))
            {
                m_includeInParseTree = bool.Parse( value );
            }
        }

        /// <summary>
        /// If set to true, the non-null result of the parse call has
        /// to be included in the resulting parse tree. False otherwise.
        /// </summary>
        public void setIncludeParseTreeNode(bool value)
        {
            m_includeInParseTree = value;

            // no use trying to create child nodes as they
            // will eventually be discarded anyway. So
            // propagate the setting
            if (!value && m_subRules != null)
            {
                for (int i = 0; i < m_subRules.Length; ++i)
                {
                    m_subRules[i].setIncludeParseTreeNode( false );
                }
            }
        }

        /// <summary>
        /// If true, the non-null result of the parse call has
        /// to be included in the resulting parse tree. False otherwise.
        /// </summary>
        public virtual bool includeParseTreeNode()
        {
            return m_includeInParseTree;
        }
    }
}
