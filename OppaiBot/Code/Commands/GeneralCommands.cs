using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Text;

public class GeneralAttribute : Attribute { }
public class GeneralCommands : BaseCommandModule
{
    [General]
    [Command("Work")]
    [Description("A command you can use ever so often")]
    public async Task DoWork(CommandContext ctx)
    {
        User user = Bot.GetUserByID(ctx.Member);
        TimeSpan span = DateTime.Now - user.lastClaimWork;
        
        if(span.TotalSeconds >= ConfigHandler.economyConfig.workCooldown)
        {
            user.lastClaimWork = DateTime.Now;
            Random rng = new Random();

            int min = ConfigHandler.economyConfig.minWorkReward;
            int max = ConfigHandler.economyConfig.maxWorkReward;

            int value = rng.Next(min, max);

            user.points += value;

            await Bot.SendBasicEmbed(ctx.Channel, "Congrats", "Congrats you did some work and gained `" + value + "` " + ConfigHandler.economyConfig.currency, EmbedColor.server, true);
        }
        else
        {
            int cooldown = ConfigHandler.economyConfig.workCooldown;
            DateTime lastUsage = user.lastClaimWork;

            double floored = Math.Floor(span.TotalSeconds);
            double difference = cooldown - floored;

            await Bot.SendBasicEmbed(ctx.Channel, "Too early", "You can use this again in `" + difference + "` seconds!", EmbedColor.server, true);
        }
    }

    [General]
    [Command("Daily")]
    [Description("A command you can use ever so often")]
    public async Task DoDaily(CommandContext ctx)
    {
        User user = Bot.GetUserByID(ctx.Member);
        TimeSpan span = DateTime.Now - user.lastClaimDaily;
        if (span.TotalSeconds >= ConfigHandler.economyConfig.dailyCooldown)
        {
            user.lastClaimDaily = DateTime.Now;
            Random rng = new Random();

            int min = ConfigHandler.economyConfig.minDailyReward;
            int max = ConfigHandler.economyConfig.minDailyReward;

            int value = rng.Next(min, max);
            user.points += value;

            await Bot.SendBasicEmbed(ctx.Channel, "Congrats", "You gained your daily reward worth `" + value + "` " + ConfigHandler.economyConfig.currency, EmbedColor.server, true);
        }
        else
        {
            int cooldown = ConfigHandler.economyConfig.dailyCooldown;
            DateTime lastUsage = user.lastClaimDaily;

            double floored = Math.Floor(span.TotalSeconds);
            double difference = cooldown - floored;

            string unit = "";
            if (difference <= 60)
                unit = "seconds ";
            else if (difference <= 3600)
            {
                unit = "minutes ";
                difference = Math.Floor(difference / 60);
            }
            else
            {
                unit = "hours";
                difference = Math.Floor(difference / 3600);
            }

            await Bot.SendBasicEmbed(ctx.Channel, "Too early", "You can use this again in `" + difference + "` " + unit + "!", EmbedColor.server, true);
        }
    }

    [General]
    [Command("Leaderboard")]
    [Description("Shows leader board of top users. You can sort by: Money, Score, Level and Exp!")]
    public async Task ShowLeaderboard(CommandContext ctx)
    {
        Console.WriteLine("I am called without arguments");
        List<User> sortedUsers = Bot.users;

        sortedUsers = sortedUsers.OrderBy(x => x.exp).ToList();
        List<string> values = sortedUsers.Select(x => x.exp.ToString()).ToList();
        values.Reverse();

        Console.WriteLine("Called");

        await ShowLeaderboardEmbed(ctx, sortedUsers, values, "exp");
    }

    [General]
    [Command("Leaderboard")]
    public async Task ShowLeaderboard(CommandContext ctx, string sortBy)
    {
        Console.WriteLine("I am called with arguements");

        List<User> sortedUsers = Bot.users;
        List<string> values = new List<string>();
        LevelConfig cfg = ConfigHandler.levelConfig;

        switch (sortBy.ToLower())
        {
            case "currency":
                sortedUsers = sortedUsers.OrderBy(x => x.points).ToList();
                values = sortedUsers.Select(x => x.points.ToString()).ToList();
                values.Reverse();
                break;
            case "level":
                
                sortedUsers = sortedUsers.OrderBy(x => x.level).ToList();
                values = sortedUsers.Select(x => x.level.ToString()).ToList();
                values.Reverse();
                break;
            case "exp":
                sortedUsers = sortedUsers.OrderBy(x => cfg.GetExpForLevel(x.level) + x.exp).ToList();
                values = sortedUsers.Select(x => (cfg.GetExpForLevel(x.level) + x.exp).ToString()).ToList();
                values.Reverse();
                break;
            default:
                sortedUsers = sortedUsers.OrderBy(x => cfg.GetExpForLevel(x.level) + x.exp).ToList();
                values = sortedUsers.Select(x => (cfg.GetExpForLevel(x.level) + x.exp).ToString()).ToList();
                values.Reverse();
                sortBy = "exp";
                break;
        }

        await ShowLeaderboardEmbed(ctx, sortedUsers, values, sortBy);
    }

    [General]
    [Command("BuyExp")]
    [Description("You write how many points you want to spend and get a certain amount of exp back.")]
    public async Task ExchangeToExp(CommandContext ctx, int pointsUsed)
    {
        float exp = pointsUsed * ConfigHandler.economyConfig.pointsToExpRate;
        User user = Bot.GetUserByID(ctx.Member);

        user.GiveExp(exp, ctx.Member, ExpType.Admin);
        string desc = "You recieved " + exp + "!";
        await Bot.SendBasicEmbed(ctx.Channel, "Success", desc, EmbedColor.success, true);
    }

    [General]
    [Command("Pick")]
    [Description("If theres a drop you can use this to pick it up!")]
    public async Task Pickup(CommandContext ctx)
    {
        if(Bot.dropMessage != null)
        {
            await Bot.dropMessage.DeleteAsync();
            Bot.dropMessage = null;

            User user = Bot.GetUserByID(ctx.Member);
            user.points += Bot.amountDropped;
            string currencyName = ConfigHandler.economyConfig.currency;

            await ctx.Message.DeleteAsync();
            await Bot.SendBasicEmbed(ctx.Channel, "Woo", "Congrats " + ctx.Member.DisplayName + " you picked up " + Bot.amountDropped + " " + currencyName + "!", EmbedColor.server, true);

            Bot.amountDropped = 0;
        }
        else { await ctx.Message.DeleteAsync(); }
    }

    [General]
    [Command("Balance")]
    [Aliases("Bal")]
    public async Task Balance(CommandContext ctx)
    {
        User user = Bot.GetUserByID(ctx.Member);
        string currencyName = ConfigHandler.economyConfig.currency;
        await Bot.SendBasicEmbed(ctx.Channel, "Woo", ctx.Member.DisplayName + " currently have " + user.points + " " + currencyName + "!", EmbedColor.server, true);
    }
    public async Task ShowLeaderboardEmbed(CommandContext ctx, List<User> users, List<string> values, string sortedName)
    {
        DiscordMessage msg = await ctx.RespondAsync("Gathering informations!");

        string title = sortedName + " Leaderboard";
        string description = "This leaderboard is sorted by " + sortedName;

        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
        builder.Title = title;
        builder.Description = description;

        for (int i = 0; i < users.Count; i++)
        {
            builder.AddField(users[i].name, values[i], false);
        }

        await msg.DeleteAsync();
        await ctx.RespondAsync("", false, builder.Build());
    }

    [General]
    [Command("Profile")]
    [Aliases("P")]
    public async Task Profile(CommandContext ctx)
    {
        DiscordMessage msg = await ImageProcesser.GetLevelupProfile(ctx);
        DeleteRequest request = new DeleteRequest(msg, 15);
    }

    [General]
    [Command("Tickets")]
    [Aliases("BuyTickets")]
    public async Task BuyTickets(CommandContext ctx, int number)
    {
        bool isSuccess = false;
        string response = "";

        if(GiveawayMaster.instance.lotteryTicketMessage != null)
        {
            User user = Bot.GetUserByID(ctx.Member);
            if(user.points >= number * GiveawayMaster.instance.cost)
            {
                user.points -= number * GiveawayMaster.instance.cost;
                user.lotteryTickets += number;

                response = "You bought " + number + "Lottery tickets!";
                isSuccess = true;
            }
            else { response = "You dont have enough " + ConfigHandler.economyConfig.currency + " to buy " + number + " for " + (number * GiveawayMaster.instance.cost); }
        }
        else { response = "Sorry there is currently no lottery going!"; }

        await Bot.SendBasicEmbed(ctx.Channel, "Lottery Master:", response, isSuccess ? EmbedColor.success : EmbedColor.error);
    }

}