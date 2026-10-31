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
            // MAX: the shipped database has a version 0 row, and update scripts insert a row per version.
            string command = $"SELECT MAX(Version) from Database";
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand(command, connection))
                {
                    if (cmd.ExecuteScalar() is long storedVersion)
                        version = storedVersion;
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
            catch (SqliteException e) when (e.Message.Contains("duplicate column name"))
            {
                // The shipped database already contains the column, so only the version is missing.
                using (var cmd = new SqliteCommand("INSERT INTO [Database] (Version) VALUES (@version)", con))
                {
                    cmd.Parameters.AddWithValue("@version", version);
                    cmd.ExecuteNonQuery();
                }
                logHandler.LogMessage($"Database upgrade {version} was already applied.", client);
            }
            catch (Exception e)
            {
                logHandler.LogMessage($"Database upgrade exception {e.Message}", client);
            }
        }
    }
}
