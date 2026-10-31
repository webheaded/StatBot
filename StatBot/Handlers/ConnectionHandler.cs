// ***********************************************************************
// Assembly         : StatBot
// Author           : Jeroen Heijster
// Created          : 13-05-2022
//
// Last Modified By : Jeroen Heijster
// Last Modified On : 05-06-2022
// ***********************************************************************
// <copyright file="ConnectionHandler.cs">
//     Copyright ©  2022
// </copyright>
// <summary></summary>
// ***********************************************************************
using Discord;
using Discord.WebSocket;
using StatBot.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StatBot.Handlers
{
    /// <summary>
    /// Class ConnectionHandler. Reports connection changes; Discord.Net reconnects by itself.
    /// </summary>
    internal class ConnectionHandler
    {
        /// <summary>
        /// The client
        /// </summary>
        DiscordSocketClient _client;

        /// <summary>
        /// The log handler
        /// </summary>
        private LogHandler _logHandler;

        /// <summary>
        /// The bot settings
        /// </summary>
        BotSettings _botSettings;
        /// <summary>
        /// The message handler
        /// </summary>
        MessageHandler _messageHandler;
        /// <summary>
        /// The time the connection was lost, or null while connected
        /// </summary>
        DateTime? _disconnectDateTime;
        /// <summary>
        /// Whether the client has been ready before
        /// </summary>
        bool _hasBeenReady;
        /// <summary>
        /// Cancels the running dead chat check
        /// </summary>
        CancellationTokenSource _deadChatCancellation;
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHandler" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <param name="botSettings">The bot settings.</param>
        /// <param name="messageHandler">The message handler.</param>
        public ConnectionHandler(DiscordSocketClient client, LogHandler logHandler, BotSettings botSettings, MessageHandler messageHandler)
        {
            _client = client;
            _logHandler = logHandler;
            _botSettings = botSettings;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// Handles the event when the client is ready for the first time.
        /// </summary>
        public Task Client_Ready()
        {
            if (!_hasBeenReady)
            {
                _hasBeenReady = true;
                _logHandler.LogMessage($"Connected to the server at {DateTime.Now}.", _client);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the event when the client disconnects.
        /// </summary>
        /// <param name="exception">The reason for the disconnect.</param>
        public Task Client_Disconnected(Exception exception)
        {
            _deadChatCancellation?.Cancel();
            if (_disconnectDateTime == null)
            {
                var disconnectTime = DateTime.Now;
                _disconnectDateTime = disconnectTime;
                _ = Task.Run(() => LogDisconnectAsync(disconnectTime));
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the event when the client (re)connects.
        /// </summary>
        public Task Client_Connected()
        {
            if (_disconnectDateTime is DateTime disconnectTime)
            {
                _disconnectDateTime = null;
                if (DateTime.Now >= disconnectTime.AddMilliseconds(_botSettings.Application.NotificationDelay))
                    _logHandler.LogMessage($"Reconnected with the server at {DateTime.Now}.", _client);
                if (_botSettings.Application.DeadChatAfter > 0)
                {
                    _deadChatCancellation = new CancellationTokenSource();
                    _ = CheckDeadChatAsync(DateTime.Now, _deadChatCancellation.Token);
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Restarts the connection if no message arrives within DeadChatAfter ms after reconnecting.
        /// </summary>
        /// <param name="reconnectTime">The reconnect time.</param>
        /// <param name="cancellationToken">Cancelled when the connection is lost again.</param>
        private async Task CheckDeadChatAsync(DateTime reconnectTime, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(_botSettings.Application.DeadChatAfter, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            if (_messageHandler.LastMessage >= reconnectTime)
                return;
            _logHandler.LogMessage("Dead chat after disconnect detected. Restarting the connection.", _client);
            try
            {
                // Same client, so the event handlers stay attached.
                await _client.StopAsync();
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                _logHandler.LogMessage($"Restarting the connection failed: {ex.Message}", _client);
            }
        }

        /// <summary>
        /// Logs the disconnect if the connection isn't back within NotificationDelay ms.
        /// </summary>
        /// <param name="disconnectTime">The disconnect time.</param>
        private async Task LogDisconnectAsync(DateTime disconnectTime)
        {
            if (_botSettings.Application.NotificationDelay > 0)
                await Task.Delay(_botSettings.Application.NotificationDelay);
            if (_client.ConnectionState != ConnectionState.Connected)
                _logHandler.LogMessage($"The connection to the server has been lost at {disconnectTime}.", _client);
        }
    }
}
