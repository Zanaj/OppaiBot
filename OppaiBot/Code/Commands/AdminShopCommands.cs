using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MessageReqDefinations;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AdminShopCommands : BaseCommandModule
{
    [Admin]
    [Command("AdminShopAdd")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task AddItemToShop(CommandContext ctx)
    {
        string specialTypeMsg = "What should happen when buying item:" +
            "\n write `dm` to dm me about people who bought item." +
            "\n write `role` to give the person a role" +
            "\n write `msg` to write information in a channel";

        string specialTextMsg = "If you selected dm last time just do something here." +
            "\n if you selected role then tag the role which you wish this item to unlock" +
            "\n if you selected msg last time you tag the channel which you wish the info to appear in";

        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement("What is the name of the item?", ArguementType.STRING),
            new MessageRequirement("What is the description of the item?", ArguementType.STRING),
            new MessageRequirement("What is the cost of the item?", ArguementType.INT),
            new MessageRequirement(specialTypeMsg, ArguementType.STRING),
            new MessageRequirement(specialTextMsg, ArguementType.STRING),
            new MessageRequirement("Which emoji do you want to be reacted with?", ArguementType.STRING),
            new MessageRequirement("Tag which channel you want this item to be displayed at", ArguementType.CHANNEL),

        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, Adminshop.instance, requirements);
        request.extraInfo = "add";

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("AdminShopRemove")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task RemoveItemFromShop(CommandContext ctx, DiscordChannel channel, ulong id)
    {
        if(Adminshop.instance.items.Exists(x => x.msgID == id))
        {
            AdminShopItem item = Adminshop.instance.items.Find(x => x.msgID == id);
            Adminshop.instance.items.Remove(item);

            DiscordMessage message = await channel.GetMessageAsync(id);
            if(message != null)
            {
                await channel.DeleteMessageAsync(message);
                //TODO: Save database
            }
        }
    }
}
