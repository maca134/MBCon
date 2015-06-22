using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Proxy
{
    public class IniParser
    {
        private readonly Hashtable keyPairs = new Hashtable();
        private readonly List<SectionPair> tmpList = new List<SectionPair>();

        public readonly string Name;

        public IniParser(string iniPath)
        {
            string str2 = null;
            Name = Path.GetFileNameWithoutExtension(iniPath);

            if (!File.Exists(iniPath))
                throw new FileNotFoundException("Unable to locate " + iniPath);

            using (TextReader reader = new StreamReader(iniPath))
            {
                for (var str = reader.ReadLine(); str != null; str = reader.ReadLine())
                {
                    str = str.Trim();
                    if (str == "")
                        continue;

                    if (str.StartsWith("[") && str.EndsWith("]"))
                        str2 = str.Substring(1, str.Length - 2);
                    else
                    {
                        SectionPair pair;

                        if (str.StartsWith(";"))
                            str = str.Replace("=", "%eq%") + @"=%comment%";

                        var strArray = str.Split(new[] { '=' }, 2);
                        string str3 = null;
                        if (str2 == null)
                        {
                            str2 = "ROOT";
                        }
                        pair.Section = str2;
                        pair.Key = strArray[0];
                        if (strArray.Length > 1)
                        {
                            str3 = strArray[1];
                        }
                        keyPairs.Add(pair, str3);
                        tmpList.Add(pair);
                    }
                }
            }
        }

        public int Count()
        {
            return Sections.Length;
        }

        public void DeleteSetting(string sectionName, string settingName)
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            if (keyPairs.ContainsKey(pair))
            {
                keyPairs.Remove(pair);
                tmpList.Remove(pair);
            }
        }

        public string[] EnumSection(string sectionName)
        {
            return (from pair in tmpList where !pair.Key.StartsWith(";") where pair.Section == sectionName select pair.Key).ToArray();
        }

        public string[] Sections
        {
            get
            {
                return (from pair in tmpList
                        group pair by pair.Section into bySection
                        from IGrouping<string, SectionPair> g in bySection
                        select g.Key).ToArray<string>();
            }
        }

        public string GetSetting(string sectionName, string settingName, string defaultValue = "")
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            if (!keyPairs.ContainsKey(pair))
                return defaultValue;
            return ((string)keyPairs[pair]).Trim();
        }

        public bool GetBoolSetting(string sectionName, string settingName, bool defaultValue = false)
        {
            if (defaultValue)
                return (GetSetting(sectionName, settingName).ToLower() != "false");
            else
                return (GetSetting(sectionName, settingName).ToLower() == "true");
        }

        public bool ContainsSetting(string sectionName, string settingName)
        {
            SectionPair pair;
            pair.Section = sectionName;
            pair.Key = settingName;
            return keyPairs.Contains(pair);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SectionPair
        {
            public string Section;
            public string Key;
        }
    }
}
