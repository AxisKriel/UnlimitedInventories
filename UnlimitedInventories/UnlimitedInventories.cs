using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace UnlimitedInventories
{
	[ApiVersion(2, 1)]
	public class UnlimitedInventories : TerrariaPlugin
	{
		public override string Name { get { return "UnlimitedInventories"; } }
		public override string Author { get { return "Professor X"; } }
		public override string Description { get { return "Enables saving/loading multiple inventories."; } }
		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		public static UnlimitedInventories Instance;

		public Config Configuration = new Config();
		public Database Database = new Database();
		public Dictionary<int, UIPlayer> Players = new Dictionary<int, UIPlayer>();

		public UnlimitedInventories(Main game) : base(game)
		{
			Instance = this;
		}

		#region Initialize/Dispose
		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);

			GeneralHooks.ReloadEvent += OnReload;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);

				GeneralHooks.ReloadEvent -= OnReload;
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Hooks
		private void OnInitialize(EventArgs args)
		{
			LoadConfig();
			Database.DBConnect();
			Database.LoadDatabase();

			Commands.ChatCommands.Add(new Command("ui.root", UICommands.InventoryCommand, "inventory", "inv") { AllowServer = false, HelpText = "Saves or loads and inventory." });
		}

		private void OnReload(ReloadEventArgs args)
		{
			LoadConfig();
			Database.LoadDatabase();
		}
		#endregion

		#region Load Config
		private void LoadConfig()
		{
			string configPath = Path.Combine(TShock.SavePath, "UnlimitedInventories.json");
			(Configuration = Config.Read(configPath)).Write(configPath);
		}
		#endregion
	}
}