using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Newtonsoft.Json;

namespace UnlimitedInventories
{
    public class Config
    {
        public int MaxInventories = 3;
        public string BypassMaxPermission = "ui.bypass";

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public Config Read(string path)
        {
            return !File.Exists(path)
                ? new Config()
                : JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}
