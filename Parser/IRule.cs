//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

namespace entropy.parser
{
    /// <summary>
    /// Callback signature for when a rule is matched 
    /// </summary>
    /// <param name="rule">Rule successfully matched</param>
    /// <param name="text">Complete text in which the rule was matched</param>
    /// <param name="index">Index of the match</param>
    /// <param name="length">Length of the match</param>
    public delegate void ruleMatchCallback( IRule rule, string text, int index, int length );

    /// <summary>
    /// Interface used to match text to rule
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Checks if this rule matches given text at the given index
        /// </summary>
        /// <returns> the length of the match or -1 if no match is found </returns>
        int         isMatch( string text, int index );

        /// <summary>
        /// Sets the callback that will be called when the isMatch function is 
        /// succesful
        /// </summary>
        void        setMatchCallback( ruleMatchCallback callback );

        /// <summary>
        /// Parses the given text from the index according to this rule and 
        /// returns the result as a parseTreeNode
        /// </summary>
        /// <returns>a node is succesful or null if the rule fails</returns>
        ParseTreeNode   parse( string text, int index );
        
        /// <summary>
        /// Returns whether or not this rule will need the resulting parse
        /// tree node to be included in the final resulting parseTree.
        /// </summary>
        bool        includeParseTreeNode();

        /// <summary>
        /// Sets whether or not this rule must be included in the final parseTree.
        /// </summary>
        void        setIncludeParseTreeNode(bool value);

        /// <summary>
        /// Sets a string identifier associated with this rule
        /// </summary>
        void        setRuleID(string name);

        /// <summary>
        /// Returns the string identifier associated with this rule
        /// </summary>
        string      getRuleID();

        /// <summary>
        /// Sets a property of this rule
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void        setProperty(string property, string value);
    }
}
