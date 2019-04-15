#region USING DIRECTIVES

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Sol.Common;
using Sol.Extensions;

#endregion USING DIRECTIVES

namespace Sol
{
    public sealed class Shard
    {
        public static IReadOnlyList<(string Name, Command Command)> Commands;

        public static void UpdateCommandsList(CommandsNextExtension cnext)
        {
            Commands = cnext.GetAllRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToList()
                .AsReadOnly();
        }

        public int Id { get; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public SharedData SharedData { get; private set; }

        public Shard(int sid, SharedData shared)
        {
            this.Id = sid;
            this.SharedData = shared;
        }
    }
}
