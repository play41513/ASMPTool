using ASMPTool.Model;
using System;
using System.IO;
using System.Collections.Generic;

namespace ASMPTool.DAL
{

    public class INIFileDAL()
    {

        static public string ReadString(string _filePath, string section, string key, string defaultValue = "")
        {
            string[] lines = File.ReadAllLines(_filePath);
            bool inSection = false;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[" + section + "]"))
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    // 如果遇到新的 [section]，表示目前 section 結束
                    if (trimmedLine.StartsWith('['))
                        break;

                    // 解析 key=value 格式
                    string[] parts = trimmedLine.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Trim() == key)
                    {
                        return parts[1].Trim();
                    }
                }
            }
            return defaultValue;
        }

        static public int ReadInteger(string _filePath, string section, string key, int defaultValue = 0)
        {
            string value = ReadString(_filePath,section, key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return int.Parse(value);
        }

        static public bool ReadBoolean(string _filePath, string section, string key, bool defaultValue = false)
        {
            string value = ReadString(_filePath, section, key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return value == "1";
        }
    }
}