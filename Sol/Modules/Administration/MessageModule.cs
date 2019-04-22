#region USING DIRECTIVES

using Sol.Common;
using Sol.Common.Attributes;
using Sol.Database;
using Sol.Exceptions;
using Sol.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using System;
using System.Threading.Tasks;

#endregion USING DIRECTIVES

namespace Sol.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("m", "msg")]
    [RequireOwnerOrPermissions(Permissions.ManageMessages)]
    [Description("Utility commands for text-channels.")]
    public class MessageModule : AppModule
    {
        public MessageModule(SharedData shared, DatabaseContextBuilder dbb) 
            : base(shared, dbb)
        {
            this.ModuleColor = DiscordColor.Green;
        }

        [Command("pin")]
        public async Task PinMessageAsync(CommandContext ctx, long? message)
        {
            if (message == null || message == 0) {
                message = 123456789012;

                var emb = new DiscordEmbedBuilder() {
                    Title = "Error!",
                    Description = "Invalid command usage! This command must include a message ID number.\n" + 
                                 $"Example: {message}"
                };

                await ctx.RespondAsync();

                throw new InvalidCommandUsageException("Invalid command usage!");
            }

            await (ctx.Message as DiscordMessage).PinAsync();
        }
    }
}
