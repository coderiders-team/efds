using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpiredFilesDestructorService
{
    public static class Helper
    {
        public static string getAppSetting(string name)
        {
            string[] fileLines = File.ReadAllLines(@"config.txt");
            string settingValue;
            Dictionary<string, string> settings = new Dictionary<string, string>();
            foreach (var line in fileLines)
            {
                string[] entries = line.Split('=');
                settings.Add(entries[0], entries[1]);
            }
            bool keyExists = settings.TryGetValue(name, out settingValue);
            if(!keyExists)
            {
                settingValue = "";
            }
            return settingValue;
        }
    }
}
