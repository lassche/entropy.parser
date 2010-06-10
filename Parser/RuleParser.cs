//
// entropy.parser
// (c) 2010 ML
//
// released under the creative commons attribution-non commerical license, see
// http://69.162.108.50/~marklass/license.html
//

using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace entropy.parser
{
    /// <summary>
    /// The rule parser allows for rules being build based
    /// on a string specification.
    /// 
    /// The string specification has to have the form
    /// 
    /// ruleID = ruleExpression;
    /// 
    /// eg 
    ///  digit = '0123456789';
    ///  functionKeyword = "function";
    ///  letter = ('abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ' );
    ///  httpAddress = [ protocol, address ];
    /// 
    /// </summary>
    public class RuleParser
    {
        private MatchAnyRule   MATCH_ANY;
        private MatchCharRule  IDENTIFIER_CHAR;
        private SequenceRule   IDENTIFIER;

        private OrRule         LITERAL_CHARS;
        private MatchUntilRule LITERAL_CONTENT;
        private SequenceRule   LITERAL_RULE;

        private MatchCharRule  MATCH_ANY_RULE;

        private SequenceRule   RULE_PARAM;

        private MatchCharRule  OR_RULE_BEGIN;
        private MatchCharRule  OR_RULE_END;
        private CountRule      OR_RULE_REMAINDER;
        private SequenceRule   OR_RULE;

        private  MatchCharRule  SEQUENCE_RULE_BEGIN;
        private  MatchCharRule  SEQUENCE_RULE_END;
        private  CountRule      SEQUENCE_RULE_REMAINDER;
        private  SequenceRule   SEQUENCE_RULE;

        private  OrRule         MATCH_CHAR_CHARS;
        private  MatchUntilRule MATCH_CHAR_CONTENT;
        private  SequenceRule   MATCH_CHAR_RULE;

        private  MatchCharRule  MATCH_UNTIL_RULE_BEGIN;
        private  MatchCharRule  MATCH_UNTIL_RULE_END;
        private  SequenceRule   MATCH_UNTIL_RULE;

        private  SequenceRule   REFERENCE_RULE;

        private  SequenceRule   NOT_RULE;

        private  MatchCharRule  DIGIT;
        private  CountRule      SIGN_RULE;
        private  SequenceRule   NUMBER;
        private  SequenceRule   COUNT_MIN;
        private  SequenceRule   COUNT_MAX;

        private  CountRule      COUNT_MAX_SYNTAX;
        private  CountRule      COUNT_REMAINDER_SYNTAX;

        private  MatchCharRule  COUNT_RULE_BEGIN;
        private  MatchCharRule  COUNT_RULE_END;
        private  SequenceRule   COUNT_RULE;

        private  MatchLiteralRule EOF_RULE;

        private  OrRule           RULE;
        private  CountRule        RULE_LIST;


        private Dictionary<string, IRule>     m_ruleRegistry;
        private IRule                         m_ruleParser;

        private string                        m_ruleID;
        private int                           m_min;
        private int                           m_max;
        private List< List<IRule> >           m_parsedRuleStack;

        private string                        m_propertyID;
        private string                        m_propertyValue;
        private string                        m_propertyRuleID;

        public RuleParser()
        {
            initialize();
        }

        public RuleParser(string fileName)
        {
            Debug.Assert( fileName != null && fileName.Length > 0);
            Debug.Assert( File.Exists( fileName ) );

            initialize();
            readRules( fileName );
        }

        
        

        /// <summary>
        /// Read the rules of a parser from the given file
        /// </summary>
        /// <param name="file"></param>
        public void readRules(string file)
        {
            StreamReader reader = new StreamReader(File.Open(file, FileMode.Open));
            string ruleText = reader.ReadToEnd();
            reader.Close();

            int index = 0;

            while (index >= 0)
            {
                m_parsedRuleStack.Clear();
                m_parsedRuleStack.Add( new List<IRule>() );

                var test = m_ruleParser.isMatch( ruleText, index );
                
                if (test >= 0 )
                {
                    // was it a comment ?
                    if (m_parsedRuleStack[0].Count > 0)
                    {
                        IRule rule = m_parsedRuleStack[0][0];

                        rule.setRuleID( m_ruleID );
                        m_ruleRegistry[m_ruleID] = rule;
                    }

                    index += test;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Find the rule with the given ruleID
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public IRule findRule(string ruleID)
        {
            return m_ruleRegistry[ruleID];
        }

        /// <summary>
        /// Link match callback handler to the rules
        /// </summary>
        /// <param name="handler"></param>
        public void linkHandler( Object handler)
        {
            Type t = handler.GetType();

            foreach (MethodInfo info in t.GetMethods())
            {
                string name = info.Name;

                if (m_ruleRegistry.ContainsKey( name ))
                {
                    ruleMatchCallback delegateHandler = (ruleMatchCallback)
                        Delegate.CreateDelegate(typeof(ruleMatchCallback), handler, info);
                    m_ruleRegistry[name].setMatchCallback( delegateHandler );
                }
            }
        }

        /// <summary>
        /// Build a rule based on the given string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IRule buildRule( string input )
        {
            return buildRule( input, 0 );
        }

        /// <summary>
        /// Build a rule based on the given start starting at the given index
        /// </summary>
        /// <param name="input"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public IRule buildRule( string input, int index )
        {
            m_parsedRuleStack.Clear();
            m_parsedRuleStack.Add( new List<IRule>() );
            
            if (m_ruleParser.isMatch( input, index ) >= 0
                && m_parsedRuleStack[0].Count > 0)
            {
                IRule rule = m_parsedRuleStack[0][0];

                rule.setRuleID( m_ruleID );
                m_ruleRegistry[m_ruleID] = rule;
                return rule;
            }

            return null;
        }

        private void initialize()
        {
            m_ruleRegistry  = new Dictionary<string,IRule>();
            m_parsedRuleStack = new List<List<IRule>>();

            initializeRules();
        }

        private void initializeRules()
        {
            MATCH_ANY         = new MatchAnyRule();
            IDENTIFIER_CHAR   = new MatchCharRule("_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
            IDENTIFIER        = new SequenceRule(new Object[]{IDENTIFIER_CHAR, new MatchUntilRule(new NotRule(IDENTIFIER_CHAR), IDENTIFIER_CHAR)});

            LITERAL_CHARS     = new OrRule("\\\"", MATCH_ANY);
            LITERAL_CONTENT   = new MatchUntilRule ("\"", LITERAL_CHARS);
            LITERAL_RULE      = new SequenceRule( new Object[]{"\"", LITERAL_CONTENT, "\""} );

            MATCH_ANY_RULE    = new MatchCharRule("*");

            RULE_PARAM        = new SequenceRule();

            OR_RULE_BEGIN     = new MatchCharRule("(");
            OR_RULE_END       = new MatchCharRule(")");
            OR_RULE_REMAINDER = new CountRule();
            OR_RULE           = new SequenceRule();

            SEQUENCE_RULE_BEGIN     = new MatchCharRule("[");
            SEQUENCE_RULE_END       = new MatchCharRule("]");
            SEQUENCE_RULE_REMAINDER = new CountRule();
            SEQUENCE_RULE           = new SequenceRule();

            MATCH_CHAR_CHARS   = new OrRule("\\'", MATCH_ANY);
            MATCH_CHAR_CONTENT = new MatchUntilRule ("'", MATCH_CHAR_CHARS);
            MATCH_CHAR_RULE    = new SequenceRule( new Object[]{"'", MATCH_CHAR_CONTENT, "'"} );

            MATCH_UNTIL_RULE_BEGIN  = new MatchCharRule("{");
            MATCH_UNTIL_RULE_END    = new MatchCharRule("}");
            MATCH_UNTIL_RULE        = new SequenceRule();

            REFERENCE_RULE          = new SequenceRule(new Object[]{IDENTIFIER_CHAR, new MatchUntilRule(new NotRule(IDENTIFIER_CHAR), IDENTIFIER_CHAR)});

            NOT_RULE                = new SequenceRule();

            DIGIT                   = new MatchCharRule("0123456789");
            SIGN_RULE               = new CountRule(new OrRule("-", "+"), 0, 1);
            NUMBER                  = new SequenceRule(new Object[]{SIGN_RULE, new MatchUntilRule(new NotRule(DIGIT), DIGIT)});
            COUNT_MIN               = new SequenceRule(new Object[]{NUMBER});
            COUNT_MAX               = new SequenceRule(new Object[]{NUMBER});

            COUNT_MAX_SYNTAX        = new CountRule(new SequenceRule(new Object[]{",", COUNT_MAX}), -1, -1);
            COUNT_REMAINDER_SYNTAX  = new CountRule(new SequenceRule(new Object[]{",", COUNT_MIN, COUNT_MAX_SYNTAX}), -1, -1);

            COUNT_RULE_BEGIN        = new MatchCharRule("<");
            COUNT_RULE_END          = new MatchCharRule(">");
            COUNT_RULE              = new SequenceRule();

            EOF_RULE                = new MatchLiteralRule("~");

            RULE                    = new OrRule( new Object[]{LITERAL_RULE, MATCH_ANY_RULE, OR_RULE, MATCH_CHAR_RULE, SEQUENCE_RULE, REFERENCE_RULE, MATCH_UNTIL_RULE, NOT_RULE, COUNT_RULE, EOF_RULE });
            RULE_LIST               = new CountRule(RULE, 1, int.MaxValue);

            RULE_PARAM.initialize( new Object[] { ",", RULE } );
            RULE_PARAM.setRuleID("RULE");
            
            IDENTIFIER.setMatchCallback(setRuleID);
            IDENTIFIER.setRuleID("IDENTIFIER");

            LITERAL_RULE.setMatchCallback(pushLiteral);
            LITERAL_RULE.setRuleID("LITERAL_RULE");

            MATCH_ANY_RULE.setMatchCallback(pushMatchAny);
            MATCH_ANY_RULE.setRuleID("MATCH_ANY_RULE");
            
            MATCH_CHAR_RULE.setMatchCallback(pushMatchChars);
            MATCH_CHAR_RULE.setRuleID("MATCH_CHAR_RULE");
            
            SEQUENCE_RULE_BEGIN.setMatchCallback(pushNewStack);
            SEQUENCE_RULE_REMAINDER.initialize( RULE_PARAM, 0, int.MaxValue );
            SEQUENCE_RULE_REMAINDER.setRuleID("SEQUENCE_RULE_REMAINDER");
            SEQUENCE_RULE.setRuleID("SEQUENCE_RULE");
            SEQUENCE_RULE.setMatchCallback(pushSequence);
            SEQUENCE_RULE.initialize( new Object[] { SEQUENCE_RULE_BEGIN, RULE, SEQUENCE_RULE_REMAINDER, SEQUENCE_RULE_END } );

            OR_RULE_REMAINDER.initialize( RULE_PARAM, 0, int.MaxValue );
            OR_RULE_REMAINDER.setRuleID("OR_RULE_REMAINDER");
            OR_RULE.initialize( new Object[] { OR_RULE_BEGIN, RULE, OR_RULE_REMAINDER, OR_RULE_END } );
            OR_RULE.setRuleID("OR_RULE");
            OR_RULE.setMatchCallback(pushOr);
            OR_RULE_BEGIN.setMatchCallback(pushNewStack);

            REFERENCE_RULE.setMatchCallback(pushReference);
            REFERENCE_RULE.setRuleID("REFERENCE_RULE");

            MATCH_UNTIL_RULE.initialize( new Object[] { MATCH_UNTIL_RULE_BEGIN, RULE, "...", RULE,  MATCH_UNTIL_RULE_END } );
            MATCH_UNTIL_RULE_BEGIN.setMatchCallback(pushNewStack);
            MATCH_UNTIL_RULE.setRuleID("MATCH_UNTIL_RULE");
            MATCH_UNTIL_RULE.setMatchCallback(pushMatchUntil);

            NOT_RULE.initialize( new Object[] { "!", RULE } );
            NOT_RULE.setMatchCallback(pushNot);
            NOT_RULE.setRuleID("NOT_RULE");

            COUNT_RULE.initialize( new Object[] { COUNT_RULE_BEGIN, RULE, COUNT_REMAINDER_SYNTAX, COUNT_RULE_END } );
            COUNT_RULE.setRuleID("COUNT_RULE");
            COUNT_RULE.setMatchCallback(pushCount);
            COUNT_RULE_BEGIN.setMatchCallback(beginCountRule);
            COUNT_MIN.setMatchCallback(onMin);
            COUNT_MAX.setMatchCallback(onMax);

            EOF_RULE.setMatchCallback(pushEOF);

            m_ruleParser    = new SequenceRule(new Object[]{IDENTIFIER, "=", RULE_LIST, ";"});
            
            registerPredefinedRules();

            // the rest of the rules can be build using a more consise syntax
            IRule comment = buildRule("__comment  = [\"//\", {* ... (eof, eoln)}];");
            
            IRule ruleID      = buildRule("__ruleID = [ (\"_\", letter ), < (letter, digit, \"_\") >];");
            ((SequenceRule)ruleID).setIgnoreWhiteSpace( false );
            ruleID.setMatchCallback(onPropertyRuleID);

            IRule propertyID      = buildRule("__propertyID    = [ (\"_\", letter ), < (letter, digit, \"_\") >];");
            ((SequenceRule)propertyID).setIgnoreWhiteSpace( false );
            propertyID.setMatchCallback(onPropertyID);

            IRule propertyValue   = buildRule("__propertyValue = [ (\"_\", letter ), < (letter, digit, \"_\") >];");
            ((SequenceRule)propertyValue).setIgnoreWhiteSpace( false );
            propertyValue.setMatchCallback(onPropertyValue);

            IRule propertySetting = buildRule("__property      = [__ruleID, \".\", __propertyID, \"=\", __propertyValue, \";\" ];");
            propertySetting.setMatchCallback(onPropertySetting);

            m_ruleParser = new OrRule(new Object[]{m_ruleParser, propertySetting, comment});
        }

        private void registerPredefinedRules()
        {
            m_ruleRegistry["eof"] = new MatchEOFRule();
            m_ruleRegistry["matchAny"] = new MatchAnyRule();
            m_ruleRegistry["eoln"] = new MatchCharRule("\n\r");
            m_ruleRegistry["whitespace"] = new MatchCharRule("\n\r \t");
            m_ruleRegistry["lowercase"] = new MatchCharRule("abcdefghijklmnopqrstuvwxyz");
            m_ruleRegistry["uppercase"] = new MatchCharRule("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            m_ruleRegistry["letter"] = new OrRule(m_ruleRegistry["lowercase"], m_ruleRegistry["uppercase"]);
            m_ruleRegistry["digit"] = new MatchCharRule("0123456789");

            m_ruleRegistry["letter"].setIncludeParseTreeNode( false );
            m_ruleRegistry["digit"].setIncludeParseTreeNode( false );
            m_ruleRegistry["matchAny"].setIncludeParseTreeNode( false );
        }

        private void setRuleID( IRule rule, string text, int index, int length )
        {
            m_ruleID = text.Substring(index, length);
        }

        private string getEscapeChar(char symbol)
        {
            switch (symbol)
            {
                case '"':
                    return "\"";

                case 'n':
                    return "\n";

                case 'r':
                    return "\r";

                case 't':
                    return "\t";
            }

            return "";
        }

        private string replaceEscapeChars(string input)
        {
            int begin = 0;
            int index = input.IndexOf('\\', 0);
            int length = index;
            string result = "";

            while (index >= 0)
            {
                if (length > 0)
                {
                    result += input.Substring(begin, length);
                }

                if (input.Length > index+1)
                {
                    result += getEscapeChar(input[index+1]);
                }

                begin = index + 2;
                index = input.IndexOf("\\", index + 2);
                length = index - begin;
            }
            
            return result.Length > 0 ? result : input;
        }

        private void pushLiteral( IRule rule, string text, int index, int length )
        {
            string input = text.Substring(index + 1, length - 2);
            IRule literalRule = new MatchLiteralRule(replaceEscapeChars(input));
            
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( literalRule );
        }

        private void pushEOF( IRule rule, string text, int index, int length )
        {
            IRule eofRule = new MatchEOFRule();
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( eofRule );
        }

        private IRule popTopRule()
        {
            List<IRule> currentStack = m_parsedRuleStack[m_parsedRuleStack.Count-1];
            int last = currentStack.Count - 1;
            IRule result = currentStack[last];
            currentStack.RemoveAt(last);
            return result;
        }

        private void pushNot( IRule rule, string text, int index, int length )
        {
            IRule notRule = new NotRule(popTopRule());
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( notRule );
        }

        private void pushMatchChars( IRule rule, string text, int index, int length )
        {
            IRule matchRule = new MatchCharRule(text.Substring(index + 1, length - 2));
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( matchRule );
        }

        private void pushMatchAny( IRule rule, string text, int index, int length )
        {
            IRule anyRule = new MatchAnyRule();
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( anyRule  );
        }

        private void pushReference( IRule rule, string text, int index, int length )
        {
            string id = text.Substring(index, length);
            ReferenceRule refRule = new ReferenceRule(id, this) ;
            refRule.setRuleID(refRule.getRuleID() + " " + id );
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( refRule);
        }

        private void pushNewStack( IRule rule, string text, int index, int length )
        {
            m_parsedRuleStack.Add( new List<IRule>() );
        }
        
        private void pushOr( IRule rule, string text, int index, int length )
        {
            IRule orRule = new OrRule( m_parsedRuleStack[m_parsedRuleStack.Count-1].ToArray() );
            m_parsedRuleStack.RemoveAt(m_parsedRuleStack.Count-1);
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( orRule );
        }

        private void onMin( IRule rule, string text, int index, int length )
        {
            m_min = int.Parse(text.Substring(index, length));
        }

        private void onMax( IRule rule, string text, int index, int length )
        {
            m_max = int.Parse(text.Substring(index, length));
        }

        private void pushCount( IRule rule, string text, int index, int length )
        {
            IRule countRule = new CountRule(popTopRule(), m_min, m_max);
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( countRule );
        }

        private void pushMatchUntil( IRule rule, string text, int index, int length )
        {
            IRule matchRule = new MatchUntilRule(m_parsedRuleStack[m_parsedRuleStack.Count-1][1], m_parsedRuleStack[m_parsedRuleStack.Count-1][0]);

            m_parsedRuleStack.RemoveAt(m_parsedRuleStack.Count-1);
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( matchRule );
        }

        private void pushSequence( IRule rule, string text, int index, int length )
        {
            IRule sequenceRule = new SequenceRule( m_parsedRuleStack[m_parsedRuleStack.Count-1].ToArray() );
            m_parsedRuleStack.RemoveAt(m_parsedRuleStack.Count-1);
            m_parsedRuleStack[m_parsedRuleStack.Count-1].Add( sequenceRule );
        }

        private void beginCountRule( IRule rule, string text, int index, int length )
        {
            m_max = -1;
            m_min = -1;
        }

        private void onPropertyID( IRule rule, string text, int index, int length )
        {
            m_propertyID = text.Substring(index, length);
        }

        private void onPropertyValue( IRule rule, string text, int index, int length )
        {
            m_propertyValue = text.Substring(index, length);
        }

        private void onPropertyRuleID( IRule rule, string text, int index, int length )
        {
            m_propertyRuleID = text.Substring(index, length);
        }

        private void onPropertySetting( IRule rule, string text, int index, int length )
        {
            m_ruleRegistry[m_ruleID].setProperty(m_propertyID, m_propertyValue);
        }
    }
}
