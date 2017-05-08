using System.IO;
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

		public static Config Read(string path)
		{
			return !File.Exists(path)
				? new Config()
				: JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
		}
	}
}
