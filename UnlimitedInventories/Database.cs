using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using static UnlimitedInventories.UnlimitedInventories;

namespace UnlimitedInventories
{
    // Rewrite this
    public class Database
    {
        public static IDbConnection db;

        internal static void DBConnect()
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] dbHost = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            dbHost[0],
                            dbHost.Length == 1 ? "3306" : dbHost[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)

                    };
                    break;

                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "tshock.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;

            }

            SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("UnlimitedInventories",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("Inventory", MySqlDbType.Text)));
        }

        internal static int InventoryCount(int userid)
        {
            int count = 0;
            using (QueryResult reader = db.QueryReader("SELECT * FROM UnlimitedInventories WHERE UserID=@0;", userid.ToString()))
            {
                while (reader.Read())
                {
                    count++;
                }
            }

            return count;
        }

        internal static bool HasInventory(int userid, string name)
        {
            using (QueryResult reader = db.QueryReader("SELECT * FROM UnlimitedInventories WHERE UserID=@0 AND Name=@1;", userid.ToString(), name))
            {
                if (reader.Read())
                    return true;
                else
                    return false;
            }
        }

        internal static List<string> GetInventories(int userid)
        {
            List<string> inventories = new List<string>();
            using (QueryResult reader = db.QueryReader("SELECT Name FROM UnlimitedInventories WHERE UserID=@0;", userid.ToString()))
            {
                while (reader.Read())
                {
                    inventories.Add(reader.Get<string>("Name"));
                }
            }

            return inventories;
        }

        internal static void SaveInventory(TSPlayer tsplayer, string name)
        {
            int index;
            NetItem[] Inventory = new NetItem[NetItem.MaxInventory];
            if (tsplayer != null && tsplayer.TPlayer != null)
            {
                var tPlayer = tsplayer.TPlayer;

                Item[] playerInventory = tPlayer.inventory;
                Item[] playerArmor = tPlayer.armor;
                Item[] playerDye = tPlayer.dye;
                Item[] playerMiscEquips = tPlayer.miscEquips;
                Item[] playerMiscDyes = tPlayer.miscDyes;

                for (int i = 0; i < NetItem.MaxInventory; i++)
                {
                    if (i < NetItem.InventorySlots)
                    {
                        Inventory[i] = (NetItem)playerInventory[i];
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots)
                    {
                        index = i - NetItem.InventorySlots;
                        Inventory[i] = (NetItem)playerArmor[index];
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots)
                    {
                        index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);
                        Inventory[i] = (NetItem)playerDye[index];
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots)
                    {
                        index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                        Inventory[i] = (NetItem)playerMiscEquips[index];
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
                    {
                        index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                        Inventory[i] = (NetItem)playerMiscDyes[index];
                    }
                }

                if (!HasInventory(tsplayer.User.ID, name))
                {
                    db.Query("INSERT INTO UnlimitedInventories (UserID, Name, Inventory) VALUES (@0, @1, @2);", tsplayer.User.ID.ToString(), name, string.Join("~", Inventory));
                }
                else
                {
                    db.Query("UPDATE UnlimitedInventories SET Inventory=@0 WHERE UserID=@1 AND Name=@2;", string.Join("~", Inventory), tsplayer.User.ID.ToString(), name);
                }
            }
        }

        internal static void LoadInventory(TSPlayer player, string name)
        {
            int index;
            using (QueryResult reader = db.QueryReader("SELECT * FROM UnlimitedInventories WHERE Name=@0;", name))
            {
                if (reader.Read())
                {
                    NetItem[] inventory = reader.Get<string>("Inventory").Split('~').Select(NetItem.Parse).ToArray();

                    Main.ServerSideCharacter = true;
                    NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");

                    for (int i = 0; i < NetItem.MaxInventory; i++)
                    {
                        if (i < NetItem.InventorySlots)
                        {
                            player.TPlayer.inventory[i].netDefaults(inventory[i].NetId);

                            if (player.TPlayer.inventory[i].netID != 0)
                            {
                                player.TPlayer.inventory[i].stack = inventory[i].Stack;
                                player.TPlayer.inventory[i].prefix = inventory[i].PrefixId;
                            }

                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, Main.player[player.Index].inventory[i].name, player.Index, i, Main.player[player.Index].inventory[i].prefix);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, Main.player[player.Index].inventory[i].name, player.Index, i, Main.player[player.Index].inventory[i].prefix);
                        }
                        else if (i < NetItem.InventorySlots + NetItem.ArmorSlots)
                        {
                            index = i - NetItem.InventorySlots;

                            player.TPlayer.armor[index].netDefaults(inventory[i].NetId);

                            if (player.TPlayer.armor[index].netID != 0)
                            {
                                player.TPlayer.armor[index].stack = inventory[i].Stack;
                                player.TPlayer.armor[index].prefix = inventory[i].PrefixId;
                            }

                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, Main.player[player.Index].armor[index].name, player.Index, i, Main.player[player.Index].armor[index].prefix);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, Main.player[player.Index].armor[index].name, player.Index, i, Main.player[player.Index].armor[index].prefix);
                        }
                        else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots)
                        {
                            index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);

                            player.TPlayer.dye[index].netDefaults(inventory[i].NetId);

                            if (player.TPlayer.dye[index].netID != 0)
                            {
                                player.TPlayer.dye[index].stack = inventory[i].Stack;
                                player.TPlayer.dye[index].prefix = inventory[i].PrefixId;
                            }

                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, Main.player[player.Index].dye[index].name, player.Index, i, Main.player[player.Index].dye[index].prefix);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, Main.player[player.Index].dye[index].name, player.Index, i, Main.player[player.Index].dye[index].prefix);
                        }
                        else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots)
                        {
                            index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);

                            player.TPlayer.miscEquips[index].netDefaults(inventory[i].NetId);

                            if (player.TPlayer.miscEquips[index].netID != 0)
                            {
                                player.TPlayer.miscEquips[index].stack = inventory[i].Stack;
                                player.TPlayer.miscEquips[index].prefix = inventory[i].PrefixId;
                            }

                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, Main.player[player.Index].miscEquips[index].name, player.Index, i, Main.player[player.Index].miscEquips[index].prefix);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, Main.player[player.Index].miscEquips[index].name, player.Index, i, Main.player[player.Index].miscEquips[index].prefix);
                        }
                        else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
                        {
                            index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);

                            player.TPlayer.miscDyes[index].netDefaults(inventory[i].NetId);

                            if (player.TPlayer.miscDyes[index].netID != 0)
                            {
                                player.TPlayer.miscDyes[index].stack = inventory[i].Stack;
                                player.TPlayer.miscDyes[index].prefix = inventory[i].PrefixId;
                            }

                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, Main.player[player.Index].miscDyes[index].name, player.Index, i, Main.player[player.Index].miscDyes[index].prefix);
                            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, Main.player[player.Index].miscDyes[index].name, player.Index, i, Main.player[player.Index].miscDyes[index].prefix);
                        }
                    }

                    Main.ServerSideCharacter = false;
                    NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");
                }
            }
        }
    }
}
