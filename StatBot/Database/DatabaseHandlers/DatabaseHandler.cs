// ***********************************************************************
// Assembly         : StatBot
// Author           : Jeroen Heijster
// Created          : 17-05-2022
//
// Last Modified By : Jeroen Heijster
// Last Modified On : 30-05-2022
// ***********************************************************************
// <copyright file="DatabaseHandler.cs">
//     Copyright ©  2022
// </copyright>
// <summary></summary>
// ***********************************************************************
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using StatBot.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatBot.Database.DatabaseHandlers
{
    /// <summary>
    /// Class DatabaseHandler.
    /// </summary>
    internal class DatabaseHandler
    {
        /// <summary>
        /// The database version
        /// </summary>
        private static int databaseVersion = 1;
        /// <summary>
        /// The database file
        /// </summary>
        internal const string DatabaseFile = "Database/Statbot.db";
        /// <summary>
        /// The database connection string
        /// </summary>
        internal const string ConnectionString = $"Data Source={DatabaseFile}";
        /// <summary>
        /// Creates the database.
        /// </summary>
        internal static void CreateDatabase()
        {
            if (!File.Exists(DatabaseFile))
            {
                try
                {
                    new SqliteConnection(ConnectionString);
                }
                catch { }
                using var con = new SqliteConnection(ConnectionString);
                con.Open();
                var sqlScript = File.ReadAllText("Database/Statbot.db.sql");
                using (var cmd = new SqliteCommand(sqlScript, con))
                {
                    cmd.ExecuteNonQuery();

                }
            }
        }
        /// <summary>
        /// Updates the database.
        /// </summary>
        /// <param name="logHandler">The log handler.</param>
        /// <param name="client">The client.</param>
        internal static void UpdateDatabase(LogHandler logHandler, DiscordSocketClient client)
        {
            long version = 0;
            try
            {
                new SqliteConnection(ConnectionString);
            }
            catch { }
            string command = $"SELECT Version from Database";
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand(command, connection))
                {
                    var reader = cmd.ExecuteReader();
                    if (reader != null &&
                        reader.HasRows)
                    {
                        reader.Read();
                        version = (long)reader[0];
                    }
                }
            }
            for (long i = version; i < databaseVersion; i++)
                UpdateVersion(i+1, logHandler, client);
        }

        /// <summary>
        /// Updates the version in the database.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <param name="client">The client.</param>
        private static void UpdateVersion(long version, LogHandler logHandler, DiscordSocketClient client)
        {
            using var con = new SqliteConnection(ConnectionString);
            try
            {
                con.Open();
                var sqlScript = File.ReadAllText($"Database/UpdateScripts/{version}.sql");
                using (var cmd = new SqliteCommand(sqlScript, con))
                {
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception e)
            {
                logHandler.LogMessage($"Database upgrade exception {e.Message}", client);
            }
        }
    }
}
