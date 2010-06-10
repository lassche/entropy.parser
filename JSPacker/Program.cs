using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using entropy.parser;

namespace JSPacker
{
    public class JSParserHandler
    {
        private string m_document;

        public JSParserHandler()
        {
            m_document = "";
        }

        public string getDocument()
        {
            return m_document;
        }

        public bool statement( string text, ParseTreeNode node )
        {
            ParseTreeNode statementNode = node.getChild(0);
            string    statementType = statementNode.getRule().getRuleID();
            bool      foundAssert   = false;

            if (statementType.Equals("functionCall"))
            {
                ParseTreeNode functionNameNode = statementNode.getChild(1);
                string    value            = functionNameNode.getValue( text );

                if (value.Equals("assert")) 
                {
                    foundAssert = true;
                }
            }

            if (!foundAssert)
            {
                m_document += node.getValue( text );
            }

            return false;
        }

        public bool comment( string text, ParseTreeNode node )
        {
            return false;
        }

        public bool code( string text, ParseTreeNode node )
        {
            m_document += node.getValue( text );
            return false;
        }
    };
    
    class Program
    {
        static void Main(string[] args)
        {
            
            //File.Delete(@"D:\code\javascript\sandbox\src\__lib_compressed.js");
            File.Delete(@"test_lib_compressed.js");

            FileUtil util = new FileUtil();

            util.addExcludeFile("coreincludes.js");
            util.addExcludeFile("math.js");
            util.addExcludeFile("gfx.js");
            util.addExcludeFile("audio.js");
            util.addExcludeFile("gameincludes.js");
            util.addExcludeFile("guiincludes.js");
            util.addExcludeFile("skaincludes.js");
            util.addExcludeFile("sequencerincludes.js");
            
            util.searchSubDirectories = true;
            util.collect(@"D:\code\javascript\sandbox\src", "*.js");

            RuleParser ruleUtil   = new RuleParser("jspacker.txt");
            IRule      jsFileRule = ruleUtil.findRule( "jsFile" );

            for (int i = 0; i < util.fileList.Count; ++i)
            {
                JSParserHandler handler      = new JSParserHandler();
                string          fileContent  = util.readFile(util.fileList[i]);
                ParseTreeNode   documentTree = jsFileRule.parse( fileContent, 0 );

                Console.WriteLine("processing " + i + " / " + util.fileList.Count + ": " +  util.fileList[i]);

                documentTree.process( fileContent, handler );

                
                //util.appendToFile(@"D:\code\javascript\sandbox\src\__lib_compressed.js", handler.getDocument());
                util.appendToFile(@"test_lib_compressed.js", handler.getDocument());
            }
        }
    }
}
