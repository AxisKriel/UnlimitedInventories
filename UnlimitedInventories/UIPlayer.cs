using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using static TShockAPI.NetItem;

namespace UnlimitedInventories
{
    public class UIPlayer
    {
        public int UserID;
        public Dictionary<string, NetItem[]> Inventories = new Dictionary<string, NetItem[]>();

        public UIPlayer()
        {
            UserID = new int();
            Inventories = new Dictionary<string, NetItem[]>();
        }

        public UIPlayer(int userID, Dictionary<string, NetItem[]> inventories)
        {
            UserID = userID;
            Inventories = inventories;
        }

        public int InventoryCount()
        {
            return this.Inventories.Keys.Count();
        }

        public bool HasInventory(string name)
        {
            return this.Inventories.Keys.Contains(name);
        }

        public List<string> GetInventories()
        {
            return this.Inventories.Keys.ToList();
        }

        public void CreateInventory(string name)
        {
            int index;
            NetItem[] PlayerInventory = new NetItem[MaxInventory];

            TSPlayer player = FindTSPlayerByUserID(UserID);
            if (player != null)
            {
                Item[] inventory = player.TPlayer.inventory;
                Item[] armor = player.TPlayer.armor;
                Item[] dye = player.TPlayer.dye;
                Item[] miscEquips = player.TPlayer.miscEquips;
                Item[] miscDyes = player.TPlayer.miscDyes;

                for (int i = 0; i < MaxInventory; i++)
                {
                    if (i < InventorySlots)
                    {
                        PlayerInventory[i] = (NetItem)inventory[i];
                    }
                    else if (i < InventorySlots + ArmorSlots)
                    {
                        index = i - InventorySlots;
                        PlayerInventory[i] = (NetItem)armor[index];
                    }
                    else if (i < InventorySlots + ArmorSlots + DyeSlots)
                    {
                        index = i - (InventorySlots + ArmorSlots);
                        PlayerInventory[i] = (NetItem)dye[index];
                    }
                    else if (i < InventorySlots + ArmorSlots + DyeSlots + MiscEquipSlots)
                    {
                        index = i - (InventorySlots + ArmorSlots + DyeSlots);
                        PlayerInventory[i] = (NetItem)miscEquips[index];
                    }
                    else if (i < InventorySlots + ArmorSlots + DyeSlots + MiscEquipSlots + MiscDyeSlots)
                    {
                        index = i - (InventorySlots + ArmorSlots + DyeSlots + MiscEquipSlots);
                        PlayerInventory[i] = (NetItem)miscDyes[index];
                    }
                }

                Database.SaveInventory(UserID, name, PlayerInventory, HasInventory(name));
            }
        }

        public void LoadInventory(string name)
        {
            int index;
            NetItem[] Inventory = this.Inventories[name];

            TSPlayer player = FindTSPlayerByUserID(UserID);
            if (player != null)
            {
                Main.ServerSideCharacter = true;
                NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");

                for (int i = 0; i < MaxInventory; i++)
                {
                    if (i < InventorySlots)
                    {
                        player.TPlayer.inventory[i].netDefaults(Inventory[i].NetId);

                        if (player.TPlayer.inventory[i].netID != 0)
                        {
                            player.TPlayer.inventory[i].prefix = Inventory[i].PrefixId;
                            player.TPlayer.inventory[i].stack = Inventory[i].Stack;
                        }

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.inventory[i].name, player.Index, i, player.TPlayer.inventory[i].prefix);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.inventory[i].name, player.Index, i, player.TPlayer.inventory[i].prefix);
                    }
                    else if (i < InventorySlots + ArmorSlots)
                    {
                        index = i - InventorySlots;

                        player.TPlayer.armor[index].netDefaults(Inventory[i].NetId);

                        if (player.TPlayer.armor[index].netID != 0)
                        {
                            player.TPlayer.armor[index].prefix = Inventory[i].PrefixId;
                            player.TPlayer.armor[index].stack = Inventory[i].Stack;
                        }

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.armor[index].name, player.Index, i, player.TPlayer.armor[index].prefix);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.armor[index].name, player.Index, i, player.TPlayer.armor[index].prefix);
                    }
                    else if (i < InventorySlots + ArmorSlots + DyeSlots)
                    {
                        index = i - (InventorySlots + ArmorSlots);

                        player.TPlayer.dye[index].netDefaults(Inventory[i].NetId);

                        if (player.TPlayer.dye[index].netID != 0)
                        {
                            player.TPlayer.dye[index].prefix = Inventory[i].PrefixId;
                            player.TPlayer.dye[index].stack = Inventory[i].Stack;
                        }

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.dye[index].name, player.Index, i, player.TPlayer.dye[index].prefix);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.dye[index].name, player.Index, i, player.TPlayer.dye[index].prefix);
                    }
                    else if (i < InventorySlots + ArmorSlots + DyeSlots + MiscEquipSlots)
                    {
                        index = i - (InventorySlots + ArmorSlots + DyeSlots);

                        player.TPlayer.miscEquips[index].netDefaults(Inventory[i].NetId);

                        if (player.TPlayer.miscEquips[index].netID != 0)
                        {
                            player.TPlayer.miscEquips[index].prefix = Inventory[i].PrefixId;
                            player.TPlayer.miscEquips[index].stack = Inventory[i].Stack;
                        }

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.miscEquips[index].name, player.Index, i, player.TPlayer.miscEquips[index].prefix);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.miscEquips[index].name, player.Index, i, player.TPlayer.miscEquips[index].prefix);
                    }
                    else if (i < InventorySlots + ArmorSlots + DyeSlots + MiscEquipSlots + MiscDyeSlots)
                    {
                        index = i - (InventorySlots + ArmorSlots + DyeSlots + MiscEquipSlots);

                        player.TPlayer.miscDyes[index].netDefaults(Inventory[i].NetId);

                        if (player.TPlayer.miscDyes[index].netID != 0)
                        {
                            player.TPlayer.miscDyes[index].prefix = Inventory[i].PrefixId;
                            player.TPlayer.miscDyes[index].stack = Inventory[i].Stack;
                        }

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.miscDyes[index].name, player.Index, i, player.TPlayer.miscDyes[index].prefix);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.miscDyes[index].name, player.Index, i, player.TPlayer.miscDyes[index].prefix);
                    }
                }

                Main.ServerSideCharacter = false;
                NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");
            }
        }

        public void DeleteInventory(string name)
        {
            this.Inventories.Remove(name);
            Database.DeleteInventory(UserID, name);
        }

        #region Miscellaneous
        private TSPlayer FindTSPlayerByUserID(int UserID)
        {
            return TShock.Players.Where(p => p.User != null && p.User.ID == UserID).First();
        }
        #endregion
    }
}
