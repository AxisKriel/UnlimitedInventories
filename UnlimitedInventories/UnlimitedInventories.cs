﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace UnlimitedInventories
{
    [ApiVersion(1, 22)]
    public class UnlimitedInventories : TerrariaPlugin
    {
        public override string Name { get { return "UnlimitedInventories"; } }
        public override string Author { get { return "Professor X"; } }
        public override string Description { get { return "Enables saving/loading multiple inventories."; } }
        public override Version Version { get { return new Version(1, 0, 1, 0); } }

        public static Dictionary<int, UIPlayer> players = new Dictionary<int, UIPlayer>();

        public static Config config = new Config();
        public static string configPath = Path.Combine(TShock.SavePath, "UnlimitedInventories.json");

        public UnlimitedInventories(Main game) : base(game)
        {

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
            (config = config.Read(configPath)).Write(configPath);
        }
        #endregion
    }
}