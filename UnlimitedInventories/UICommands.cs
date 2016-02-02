using System;
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
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax:");
                args.Player.SendErrorMessage("{0}inventory save <name> - saves/updates your current inventory", TShock.Config.CommandSpecifier);
                args.Player.SendErrorMessage("{0}inventory loads <name> - loads an inventory", TShock.Config.CommandSpecifier);
                args.Player.SendErrorMessage("{0}inventory list - lists all your inventories", TShock.Config.CommandSpecifier);
                return;
            }

            switch (args.Parameters[0].ToLower())
            {
                case "save":
                    #region Save Inventory
                    {
                        if (!args.Player.IsLoggedIn)
                        {
                            args.Player.SendErrorMessage("You must be logged in to do that.");
                            return;
                        }
                        else if (args.Parameters.Count < 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory save <inventory name>", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            args.Parameters.RemoveRange(0, 1); // Remove "save"
                            string inventoryName = string.Join(" ", args.Parameters.Select(x => x));
                            if (Database.InventoryCount(args.Player.User.ID) >= config.MaxInventories && !args.Player.Group.HasPermission(config.BypassMaxPermission))
                            {
                                args.Player.SendErrorMessage("You have reached the max amount of inventories.");
                                return;
                            }
                            else if (Database.HasInventory(args.Player.User.ID, inventoryName))
                            {
                                args.Player.SendErrorMessage("An inventory with the same name already exists.");
                                return;
                            }
                            else
                            {
                                Database.SaveInventory(args.Player, inventoryName);
                                args.Player.SendSuccessMessage($"Inventory saved ('{inventoryName}')!");
                            }
                        }
                    }
                    #endregion
                    break;
                case "load":
                    #region Load Inventory
                    {
                        if (!args.Player.IsLoggedIn)
                        {
                            args.Player.SendErrorMessage("You must be logged in to do that.");
                            return;
                        }
                        else if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory load <inventory name>", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            args.Parameters.RemoveRange(0, 1);
                            string inventoryName = string.Join(" ", args.Parameters.Select(x => x));
                            if (!Database.HasInventory(args.Player.User.ID, inventoryName))
                            {
                                args.Player.SendErrorMessage($"No inventories under the name of '{inventoryName}' where found. \nMake sure you spelled it correctly.");
                                return;
                            }
                            else
                            {
                                Database.LoadInventory(args.Player, inventoryName);
                                args.Player.SendSuccessMessage($"Loaded inventory '{inventoryName}'!");
                            }
                        }
                    }
                    #endregion
                    break;
                case "list":
                    #region List Inventories
                    {
                        if (!args.Player.IsLoggedIn)
                        {
                            args.Player.SendErrorMessage("You must be logged in to do that.");
                            return;
                        }
                        else if (args.Parameters.Count > 2)
                        {
                            args.Player.SendErrorMessage("Invalid syntax: {0}inventory list [page]", TShock.Config.CommandSpecifier);
                            return;
                        }
                        else
                        {
                            int pageNum;
                            if (!PaginationTools.TryParsePageNumber(args.Parameters, 2, args.Player, out pageNum))
                                return;

                            PaginationTools.SendPage(args.Player, pageNum, PaginationTools.BuildLinesFromTerms(Database.GetInventories(args.Player.User.ID)),
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
