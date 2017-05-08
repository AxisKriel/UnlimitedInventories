using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using static UnlimitedInventories.UnlimitedInventories;

namespace UnlimitedInventories
{
	public class Database
	{
		public static IDbConnection db;

		public void DBConnect()
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

		public void LoadDatabase()
		{
			Instance.Players.Clear();

			using (QueryResult reader = db.QueryReader("SELECT * FROM UnlimitedInventories"))
			{
				while (reader.Read())
				{
					int UserID = reader.Get<int>("UserID");
					string Name = reader.Get<string>("Name");
					NetItem[] Inventory = reader.Get<string>("Inventory").Split('~').Select(NetItem.Parse).ToArray();

					if (Instance.Players.ContainsKey(UserID))
						Instance.Players[UserID].Inventories.Add(Name, Inventory);
					else
						Instance.Players.Add(UserID, new UIPlayer(UserID, new Dictionary<string, NetItem[]>() { [Name] = Inventory }));
				}
			}
		}

		public void SaveInventory(int UserID, string Name, NetItem[] Inventory, bool updateExisting = false)
		{
			if (!updateExisting)
			{
				Instance.Players[UserID].Inventories.Add(Name, Inventory);
				db.Query("INSERT INTO UnlimitedInventories (UserID, Name, Inventory) VALUES (@0, @1, @2);", UserID.ToString(), Name, string.Join("~", Inventory));
			}
			else
			{
				Instance.Players[UserID].Inventories[Name] = Inventory;
				db.Query("UPDATE UnlimitedInventories SET Inventory=@0 WHERE UserID=@1 AND Name=@2;", string.Join("~", Inventory), UserID.ToString(), Name);
			}
		}

		public void DeleteInventory(int UserID, string Name)
		{
			db.Query("DELETE FROM UnlimitedInventories WHERE UserID=@0 AND Name=@1;", UserID.ToString(), Name);
		}
	}
}