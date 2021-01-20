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

public class AdminAttribute : Attribute { }

public class AdminCommands : BaseCommandModule
{
    [Admin]
    [Command("AddLevelRole")]
    [Description("Adds a level role to the list of roles. There can only be one role per level. You need to provide the role or id then the level at which you get the role")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task MakeLevelRole(CommandContext ctx, DiscordRole role, int level)
    {
        if (!Bot.levelRoles.Keys.Contains(level))
            Bot.levelRoles.Add(level, 0);

        Bot.levelRoles[level] = role.Id;
        //TODO: add roles to all people who already is this level.

        await Bot.SendBasicEmbed(ctx.Channel, "There we go", "We added the requested level role!", EmbedColor.server, true);
    }

    [Admin]
    [Command("RemoveLevelRole")]
    [Description("Adds a level role to the list of roles. There can only be one role per level. You need to provide the role or id then the level at which you get the role")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task RemoveLevelRole(CommandContext ctx, int level)
    {
        if (Bot.levelRoles.Keys.Contains(level))
        {
            Bot.levelRoles.Remove(level);
            await Bot.SendBasicEmbed(ctx.Channel, "There we go", "We removed the level " + level + " role!");
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Channel, "Whoops", "There were not a level role assigned to that level!", EmbedColor.server, true);
        }
    }

    [Admin]
    [Command("BaseSettings")]
    [Description("Starts the process of setting server's uncategorized settings. This requires no arguements.")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task SetSettings(CommandContext ctx)
    {
        string levelMsgText = "What message should display as level up?";
        levelMsgText += "You can use {user} for username and {level} to show the NEW level";

        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement(levelMsgText, ArguementType.STRING),
            new MessageRequirement("Mention the channel you wish for levels to appear", ArguementType.CHANNEL),
            new MessageRequirement("Mention all channels that are activity channels", ArguementType.CHANNEL, true),
            new MessageRequirement("Server color RED amount", ArguementType.FLOAT),
            new MessageRequirement("Server color GREEN amount", ArguementType.FLOAT),
            new MessageRequirement("Server color BLUE amount", ArguementType.FLOAT),
            new MessageRequirement("The server update rate (in ms) keep this over 150 and below 3000 recommended is 250", ArguementType.INT),
        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, ConfigHandler.baseConfig, requirements);
        
        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("EconomySettings")]
    [Description("Starts the process of setting server's other economy settings. This requires no arguements.")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task SetEconemy(CommandContext ctx)
    {
        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement("Currency", ArguementType.STRING),
            new MessageRequirement("How many points does 1 exp cost?", ArguementType.INT),
        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, ConfigHandler.economyConfig, requirements);
        request.extraInfo = "other";

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("WorkSettings")]
    [Description("Starts the process of setting server's work economy settings. This requires no arguements.")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task SetWork(CommandContext ctx)
    {
        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement("Daily work cooldown in seconds", ArguementType.INT),
            new MessageRequirement("Work minimum yield", ArguementType.INT),
            new MessageRequirement("Work maximum yield", ArguementType.INT),
        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, ConfigHandler.economyConfig, requirements);
        request.extraInfo = "work";

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("DailySettings")]
    [Description("Starts the process of setting server's daily economy settings. This requires no arguements.")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task SetDaily(CommandContext ctx)
    {
        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement("Daily reward Cooldown", ArguementType.INT),
            new MessageRequirement("Daily minimum yield", ArguementType.INT),
            new MessageRequirement("Daily maximum yield", ArguementType.INT),
        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, ConfigHandler.economyConfig, requirements);
        request.extraInfo = "daily";

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("DropSettings")]
    [Description("Starts the process of setting server's drop economy settings. This requires no arguements.")]
    [RequirePermissions(Permissions.Administrator)]
    public async Task SetDrop(CommandContext ctx)
    {
        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement("Drop cooldown", ArguementType.INT),
            new MessageRequirement("Drop minimum yield", ArguementType.INT),
            new MessageRequirement("Drop maximum yield", ArguementType.INT),
        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, ConfigHandler.economyConfig, requirements);
        request.extraInfo = "drop";

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("LevelSettings")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("Starts the process of setting server's level related settings. This requires no arguements.")]
    public async Task SetLevel(CommandContext ctx)
    {
        MessageRequirement[] requirements = new MessageRequirement[]
       {
            new MessageRequirement("Minimum exp a mesage can give. Write -1 if you wish to skip.", ArguementType.INT),
            new MessageRequirement("Maximum exp a mesage can give. Write -1 if you wish to skip.", ArguementType.INT),
            new MessageRequirement("Minimum exp being in vc gives. Write -1 if you wish to skip.", ArguementType.INT),
            new MessageRequirement("Maximum exp being in vc gives. Write -1 if you wish to skip.", ArguementType.INT),
            new MessageRequirement("Max level. Write -1 if you wish to skip.", ArguementType.INT),
            new MessageRequirement("How much extra exp pr level. Write -1 if you wish to skip.", ArguementType.FLOAT),
            new MessageRequirement("Exp needed for level 0 to 1. Write -1 if you wish to skip.", ArguementType.FLOAT),
            new MessageRequirement("How many entries should leaderboard show. Write -1 if you wish to skip.", ArguementType.INT),
       };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, ConfigHandler.baseConfig, requirements);

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }

    [Admin]
    [Command("GiveShop")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("You'll need to tag the person who gets the shop. They of course cannot already have a shop!")]
    public async Task GiveShop(CommandContext ctx, DiscordMember member)
    {
        string name = Guid.NewGuid().ToString();

        User user = Bot.GetUserByID(member);
        if (!user.hasShop)
        {
            if (!Bot.users.Exists(x => x.shopName == name))
            {
                user.hasShop = true;
                user.shopName = name;

                string description = "Congrats ";
                description += member.Mention;
                description += " you just got a shop!";
                description += "\n";
                description += "You can edit your shop name with ";
                description += ConfigHandler.baseConfig.prefix + " setupshop";

                await Bot.SendBasicEmbed(ctx.Channel, "", description, EmbedColor.server, true);
            }
            else
                await Bot.SendBasicEmbed(ctx.Channel, "", "That name is already taken!", EmbedColor.server, true);
        }
        else
            await Bot.SendBasicEmbed(ctx.Channel, "", "User already have a shop!", EmbedColor.server, true);
    }

    [Admin]
    [Command("ManageBalance")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("Allows an admin to add or remove currency from an user. If you want to remove just give negative amount. You need to tag the user then after write the amount you need.")]
    public async Task ManageBalance(CommandContext ctx, DiscordMember member, int amount)
    {
        User user = Bot.GetUserByID(member);
        user.points += amount;

        string verb = amount < 0 ? "removed from" : "added to";
        amount = Math.Abs(amount);

        string desc = amount.ToString();
        desc += " " + ConfigHandler.economyConfig.currency;
        desc += " were " + verb + " ";
        desc += member.DisplayName + "'s balance.";

        await Bot.SendBasicEmbed(ctx.Channel, "Done", desc, EmbedColor.server, true);
    }

    [Admin]
    [Command("ManageXp")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("Allows an admin to add or remove xp from an user. If you want to remove just give negative amount.. You need to tag the user then after write the amount you need.")]
    public async Task ManageExp(CommandContext ctx, DiscordMember member, float amount)
    {
        User user = Bot.GetUserByID(member);
        user.GiveExp(amount, member, ExpType.Admin);
        
        string verb = amount < 0 ? "removed from" : "added to";
        amount = Math.Abs(amount);

        string desc = amount.ToString();
        desc += " " + ConfigHandler.economyConfig.currency;
        desc += " were " + verb + " ";
        desc += member.DisplayName + "'s exp.";

        await Bot.SendBasicEmbed(ctx.Channel, "Done", desc, EmbedColor.server, true);
    }

    [Admin]
    [Command("LevelUp")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("This levels up the user by one unless otherwise is specified. If you insert a number it will set the user's level to that.")]
    public async Task LevelUp(CommandContext ctx, DiscordMember member)
    {
        User user = Bot.GetUserByID(member);
        float nextLevelExp = ConfigHandler.levelConfig.GetExpForLevel(user.level + 1);

        float expNeeded = nextLevelExp - user.exp;
        user.GiveExp(expNeeded + 1, member, ExpType.Admin);

        string desc = "Leveled " + member.DisplayName + " up to" + user.level + "!";
        string title = "Woo level up";

        await Bot.SendBasicEmbed(ctx.Channel, title, desc, EmbedColor.server, true);
    }

    [Admin]
    [Command("LevelUp")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("This levels up the user by one unless otherwise is specified. If you insert a number it will set the user's level to that.")]
    public async Task LevelUp(CommandContext ctx, DiscordMember member, int level)
    {
        User user = Bot.GetUserByID(member);
        user.exp = 0;
        user.level = 0;

        string desc = "Leveled " + member.DisplayName + " up to" + level + "!";
        string title = "Woo level up";

        await Bot.SendBasicEmbed(ctx.Channel, title, desc, EmbedColor.server, true);
    }

    [Admin]
    [Command("Lottery")]
    [RequirePermissions(Permissions.Administrator)]
    [Description("")]
    public async Task MakeLottery(CommandContext ctx)
    {
        MessageRequirement[] requirements = new MessageRequirement[]
        {
            new MessageRequirement("What should be the name of the lottery?", ArguementType.STRING),
            new MessageRequirement("How much should starting pool be?", ArguementType.INT),
            new MessageRequirement("Which unit of time is the time in? \n Write 1 for seconds \n Write 2 for minutes \n Write 3 for hours \n Write 4 for days \n Write 5 for weeks", ArguementType.INT),
            new MessageRequirement("How long should the lottery count down be for?", ArguementType.INT),
            new MessageRequirement("How much does a ticket cost?", ArguementType.INT),
            new MessageRequirement("Which channel do you want this message to appear in?", ArguementType.CHANNEL),
        };

        MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, GiveawayMaster.instance, requirements);
        request.extraInfo = ctx.Channel;

        await request.DisplayStep();
        QuerryHandler.requests.Add(request);
    }
}
