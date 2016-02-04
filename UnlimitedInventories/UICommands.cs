﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using static UnlimitedInventories.UnlimitedInventories;

namespace UnlimitedInventories
{
    public static class UICommands
    {
        public static void InventoryCommand(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You must be logged in to do that.");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax:");
                args.Player.SendErrorMessage("{0}inventory save <name> - saves/updates your current inventory", TShock.Config.CommandSpecifier);
                args.Player.SendErrorMessage("{0}inventory load <name> - loads an inventory", TShock.Config.CommandSpecifier);
                args.Player.SendErrorMessage("{0}inventory delete <name> - deletes an inventory", TShock.Config.CommandSpecifier);
                args.Player.SendErrorMessage("{0}inventory list - lists all your inventories", TShock.Config.CommandSpecifier);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "save":
                    #region Save Inventory
                    {
                        if (args.Parameters.Count < 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory save <inventory name>", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            if (!players.ContainsKey(args.Player.User.ID))
                                players.Add(args.Player.User.ID, new UIPlayer(args.Player.User.ID, new Dictionary<string, NetItem[]>()));

                            args.Parameters.RemoveRange(0, 1); // Remove "save"
                            string inventoryName = string.Join(" ", args.Parameters.Select(x => x));
                            if (players.ContainsKey(args.Player.User.ID) && players[args.Player.User.ID].InventoryCount() >= config.MaxInventories && !args.Player.Group.HasPermission(config.BypassMaxPermission))
                            {
                                args.Player.SendErrorMessage("You have reached the max amount of inventories.");
                                return;
                            }
                            else
                            {
                                players[args.Player.User.ID].CreateInventory(inventoryName);
                                args.Player.SendSuccessMessage($"Inventory saved ('{inventoryName}')!");
                            }
                        }
                    }
                    #endregion
                    break;
                case "load":
                    #region Load Inventory
                    {
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory load <inventory name>", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            args.Parameters.RemoveRange(0, 1);
                            string inventoryName = string.Join(" ", args.Parameters.Select(x => x));
                            if (!players[args.Player.User.ID].HasInventory(inventoryName))
                            {
                                args.Player.SendErrorMessage($"No inventories under the name of '{inventoryName}' were found. \nMake sure you spelled it correctly.");
                                return;
                            }
                            else
                            {
                                players[args.Player.User.ID].LoadInventory(inventoryName);
                                args.Player.SendSuccessMessage($"Loaded inventory '{inventoryName}'!");
                            }
                        }
                    }
                    #endregion
                    break;
                case "del":
                case "rem":
                case "delete":
                case "remove":
                    #region Delete Inventory
                    {
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory delete <name>", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            args.Parameters.RemoveRange(0, 1);
                            string inventoryName = string.Join(" ", args.Parameters.Select(x => x));
                            if (!players[args.Player.User.ID].HasInventory(inventoryName))
                            {
                                args.Player.SendErrorMessage($"No inventories under the name of '{inventoryName}' were found. \nMake sure you spelled it correctly.");
                                return;
                            }
                            else
                            {
                                players[args.Player.User.ID].DeleteInventory(inventoryName);
                                args.Player.SendSuccessMessage($"Deleted inventory '{inventoryName}'!");
                            }
                        }
                    }
                    #endregion
                    break;
                case "list":
                    #region List Inventories
                    {
                        if (args.Parameters.Count > 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory list [page]", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            int pageNum;
                            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNum))
                                return;

                            PaginationTools.SendPage(args.Player, pageNum, PaginationTools.BuildLinesFromTerms(players[args.Player.User.ID].GetInventories()),
                                new PaginationTools.Settings
                                {
                                    HeaderFormat = "Inventories ({0}/{1})",
                                    FooterFormat = "Type {0}inventory list {{0}} for more.".SFormat(TShock.Config.CommandSpecifier),
                                    NothingToDisplayString = "You have no inventories to display."
                                });
                        }
                    }
                    #endregion
                    break;
            }
        }
    }
}