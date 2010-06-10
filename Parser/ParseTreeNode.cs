//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace entropy.parser
{
    /// <summary>
    /// Basic datastructure that is produced by calling parse on rules
    /// </summary>
    public class ParseTreeNode
    {
        private ParseTreeNode       m_parent;
        private List<ParseTreeNode> m_children;
        private IRule               m_matchedRule;
        private int                 m_index;
        private int                 m_length;

        public ParseTreeNode(IRule rule, int index)
        {
            m_matchedRule   = rule;
            m_index         = index;
            m_length        = -1;
            m_parent        = null;
            m_children      = null;
        }

        public ParseTreeNode(IRule rule, int index, int length)
        {
            m_matchedRule = rule;
            m_index       = index;
            m_length      = length;
            m_parent      = null;
            m_children    = null;
        }

        /// <summary>
        ///  Signature used for the callback methods in the handler object
        ///  when calling process.
        /// </summary>
        public delegate bool handleRuleMatch( string text, ParseTreeNode match );

        /// <summary>
        /// Iterate through this node and its children. If a rule is encountered
        /// a method with a similar name in handler is searched and if found
        /// called. The methods in this handler must have the signature of
        /// the handleRuleMatch delegate.
        /// </summary>
        public void process( string text, Object handler )
        {
            Debug.Assert( text != null );
            Debug.Assert( handler != null );

            string      nodeName    = m_matchedRule.getRuleID();
            Type        handlerType = handler.GetType();
            MethodInfo  info        = handlerType.GetMethod(nodeName);

            if (info != null)
            {
                bool result = (bool) info.Invoke( handler, new Object[]{ text, this } );

                if (!result) return;
            }

            for (int i = 0; m_children != null && i < m_children.Count; ++i)
            {
                m_children[i].process( text, handler );
            }
        }

        /// <summary>
        /// Find a rule matching the given id up to the given maxdepth
        /// </summary>
        public ParseTreeNode findRule( string ruleID, int maxDepth )
        {
            Debug.Assert( ruleID != null );
            Debug.Assert( maxDepth >= 0 );

            ParseTreeNode result    = null;
            string        nodeName = m_matchedRule.getRuleID();

            if ( ruleID.Equals( nodeName ) )
            {
                result = this;
            }
            else if (maxDepth > 0)
            {
                for (int i = 0; m_children != null && i < m_children.Count; ++i)
                {
                    result = m_children[i].findRule( ruleID, maxDepth - 1);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the string value yielded by the index and length given
        /// the provided text
        /// </summary>
        public string getValue( string text )
        {
            return text.Substring(m_index, m_length);
        }

        /// <summary>
        /// Returns the embedded rule
        /// </summary>
        /// <returns></returns>
        public IRule getRule()
        {
            return m_matchedRule;
        }

        /// <summary>
        /// Dumps the content of this node and its children to the console
        /// </summary>
        /// <param name="indentCount"></param>
        /// <param name="text"></param>
        public void dumpToConsole(int indentCount, string text)
        {
            indent(indentCount);
            Console.WriteLine("node: " + m_matchedRule.getRuleID());

            indent(indentCount);
            Console.Write("index: " + m_index + ", length: " + m_length + "\n");

            indent(indentCount);

            if (m_length > 60)
            {
                Console.Write("text: \"" + text.Substring(m_index, 30) + "\" ... \"" 
                                + text.Substring(m_index + m_length - 30, 30) + "\"\n");
            }
            else
            {
                Console.WriteLine("text: \"" + text.Substring(m_index, m_length) + "\"" );
            }

            for (int i = 0; m_children != null && i < m_children.Count; ++i)
            {
                m_children[i].dumpToConsole(indentCount + 4, text);
            }
        }

        /// <summary>
        /// Returns the Nth child of this node
        /// </summary>
        public ParseTreeNode getChild( int index )
        {
            Debug.Assert( m_children != null );
            Debug.Assert(index >= 0 && index < m_children.Count);

            return m_children[index];
        }

        /// <summary>
        /// Sets the length of the matched item
        /// </summary>
        public void setLength( int length )
        {
            m_length = length;
        }

        /// <summary>
        /// Gets the length of the matched item
        /// </summary>
        public int getLength()
        {
            return m_length;
        }

        /// <summary>
        /// Adds the given length to the current length
        /// </summary>
        /// <param name="delta"></param>
        public void addLength( int value )
        {
            m_length += value;
        }

        /// <summary>
        /// Adds a child to this node
        /// </summary>
        /// <param name="node"></param>
        public void addChild( ParseTreeNode node )
        {
            if (m_children == null)
            {
                m_children = new List<ParseTreeNode>();
            }

            m_children.Add( node );
            node.m_parent = this;
        }

        private void indent(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                Console.Write(" ");
            }
        }

        
    }
}
