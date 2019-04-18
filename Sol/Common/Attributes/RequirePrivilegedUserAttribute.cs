﻿#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using Sol.Database;
#endregion

namespace Sol.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequirePrivilegedUserAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.User.Id == ctx.Client.CurrentApplication.Owner.Id)
                return Task.FromResult(true);

            using (DatabaseContext db = ctx.Services.GetService<DatabaseContextBuilder>().CreateContext())
                return Task.FromResult(db.PrivilegedUsers.Any(u => u.UserId == ctx.User.Id));
        }
    }
}