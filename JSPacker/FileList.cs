using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JSPacker
{
    public class FileUtil
    {
        public string rootDir;
        public List<string> fileList = new List<string>();
        public bool searchSubDirectories;
        private List<string> m_excludeFiles;

        public int collect(string root, string matchString)
        {
            string[] result = Directory.GetFiles(root, matchString);
            
            for (int i = 0; i < result.Length; ++i)
            {
                if (!isOnExcludeList(result[i]))
                {
                    fileList.Add(result[i]);
                }
            }

            if (searchSubDirectories)
            {
                result = Directory.GetDirectories(root);

                for (int i = 0; i < result.Length; ++i)
                {
                    collect(result[i], matchString);
                }
            }

            return fileList.Count;

        }

        public void addExcludeFile(string name)
        {
            if (m_excludeFiles == null)
            {
                m_excludeFiles = new List<string>();
            }

            m_excludeFiles.Add(name);
        }

        public bool isOnExcludeList(string name)
        {
            foreach (string excludeName in m_excludeFiles)
            {
                if (name.IndexOf(excludeName) >= 0)
                {
                    return true;
                }
            }

            return false;

        }


        public string readFile(string filename)
        {
            StreamReader reader = new StreamReader(File.Open(filename, FileMode.Open));

            string result = reader.ReadToEnd();

            reader.Close();

            return result;
        }

        public void appendToFile(string filename, string value)
        {
            StreamWriter writer = new StreamWriter(File.Open(filename, FileMode.Append));

            writer.Write(value);

            writer.Close();
        }

    }

}
