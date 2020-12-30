using DSharpPlus.Entities;
using MessageReqDefinations;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

public class GiveawayMaster : IMessageCallback
{
    public string title;
    public DiscordChannel channel;
    public static GiveawayMaster instance;
    public int startingPool;
    public int cost;
    public int interval;
    public DateTime postedAt;

    public DiscordMessage lotteryTicketMessage;

    public static void Initialize(GiveawayMaster master)
    {
        if (instance == null)
        {
            instance = master;
            Thread t = new Thread(GiveawayMaster.instance.Update);
            t.Start();
        }
    }

    private void Update()
    {
        //TODO: make closing down flag
        while (true)
        {
            OnGiveawayTick();
            Thread.Sleep(1000);
        }
    }

    private async Task OnGiveawayTick()
    {
        if (GiveawayMaster.instance.lotteryTicketMessage != null)
        {
            if (instance.interval - 1000 <= 0)
            {
                User[] lottryHolders = Bot.users.FindAll(x => x.lotteryTickets > 0).ToArray();
                if (lottryHolders.Length > 0)
                {
                    int lastValue = -1;
                    int amount = Bot.users.Sum(x => x.lotteryTickets);
                    int winningNumber = new Random().Next(0, amount - 1);

                    for (int i = 0; i < lottryHolders.Length; i++)
                    {
                        User holder = lottryHolders[i];
                        int min = lastValue + 1;
                        int max = holder.lotteryTickets - 1;

                        if (winningNumber <= max && winningNumber >= min)
                        {
                            instance.OnUserWon(holder, amount);
                            break;
                        }
                        else { lastValue = holder.lotteryTickets; }
                    }
                }
                else
                {
                    DiscordEmbed embed = Bot.GetBasicEmbed("Whoops", "Nobody bought any tickets!");
                    await GiveawayMaster.instance.lotteryTicketMessage.ModifyAsync("", embed);
                }
            }
            else
            {
                DiscordMessage msg = GiveawayMaster.instance.lotteryTicketMessage;
                string[] lines = msg.Content.Split(new[] { Environment.NewLine}, StringSplitOptions.None);

                string lastLine = lines.Last();
                string content = "";

                for (int i = 0; i < lines.Length-1; i++)
                {
                    content += lines[i] + "\n";
                }

                lastLine = "Time left: \n `" + GiveawayMaster.instance.GetTimeLeftString().Result + "`";
                content += lastLine;

                DiscordEmbed embed = await GiveawayMaster.instance.GetLotteryEmbed();
                await GiveawayMaster.instance.lotteryTicketMessage.ModifyAsync("", embed);
                instance.interval -= 1000;
            }
        }
    }

    public void Callback(MessageSettingRequest request)
    {
        MessageRequirement[] msg = request.queue.ToArray();

        try
        {
            title = (string)msg[0].value;
            startingPool = (int)msg[1].value;
            int mult = (int)msg[2].value;
            int val = (int)msg[3].value;
            cost = (int)msg[4].value;
            ulong channelID = (ulong)msg[5].value;

            interval = mult > 0 ? 1000 : 1;
            interval *= mult > 1 ? 60 : 1;
            interval *= mult > 2 ? 60 : 1;
            interval *= mult > 3 ? 24 : 1;
            interval *= mult > 4 ? 7 : 1;

            interval *= val;

            channel = Bot.guild.Channels[channelID];
            DiscordEmbed embed = GetLotteryEmbed().Result;

            lotteryTicketMessage = channel.SendMessageAsync("",false, embed).Result;
            postedAt = DateTime.Now;
        }
        catch (InvalidCastException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task<string> GetTimeLeftString()
    {
        TimeSpan span = DateTime.Now - postedAt;

        int inv = interval;
        double seconds = (interval - span.Milliseconds) / 1000;

        int days = 0;
        int hours = 0;
        int minutes = 0;
        int secs = 0;

        if(seconds >= 86400)
        {
            days = (int)Math.Floor(seconds / 86400);
            seconds -= days * 86400;
        }

        if (seconds >= 3600)
        {
            hours = (int)Math.Floor(seconds / 3600);
            seconds -= hours * 3600;
        }
        
        if (seconds >= 60)
        {
            minutes = (int)Math.Floor(seconds / 60);
            seconds -= minutes * 60;
        }

        secs = (int)(Math.Floor(seconds));

        string daysStr = days < 10 ? "0" + days : days.ToString();
        string hourStr = hours < 10 ? "0" + hours : hours.ToString();
        string minuteStr = minutes < 10 ? "0" + minutes : minutes.ToString();
        string secondsStr = secs < 10 ? "0" + secs : secs.ToString();

        string rtn = "```Time left: ";
        rtn += daysStr + ":" + hourStr + ":" + minuteStr + ":" + secondsStr;
        rtn += "```";

        rtn += "\n _Format: DD:HH:MM:SS_";
        return rtn;
    }

    public async Task<DiscordEmbed> GetLotteryEmbed()
    {
        User[] lottryHolders = Bot.users.FindAll(x => x.lotteryTickets > 0).ToArray();
        int lotteryTickets = lottryHolders.Sum(x => x.lotteryTickets);

        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
        builder.Title = title.ToLower() == "skip" ? "Lottery" : title;
        builder.Description += ":ticket: Total Tickets: " + lotteryTickets;
        builder.Description += "\n \n";
        builder.Description += ":coin: Ticket Cost: " + cost + " ";
        builder.Description += ConfigHandler.economyConfig.currency;
        builder.Description += "\n \n";
        builder.Description += ":moneybag: Total Winnings: " + (startingPool + (cost * lotteryTickets)) + " ";
        builder.Description += ConfigHandler.economyConfig.currency;
        builder.Description += "\n \n";
        builder.Description += "**Use " + ConfigHandler.baseConfig.prefix + " tickets [amount] to buy tickets! \n**";
        builder.Description +=  GetTimeLeftString().Result;

        builder.Color = ConfigHandler.baseConfig.serverColor;

        return builder.Build();
    }

    public void OnUserWon(User winner, int lotteryTickets)
    {
        DiscordMember member = Bot.guild.Members[winner.Id];
        int pool = startingPool;
        pool += lotteryTickets * cost;

        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
        builder.Title = title.ToLower() == "skip" ? "Lottery" : title;
        builder.Title += " is over!";

        builder.Description = ":tada: __**Winner:" + member.Mention + "**__ :tada";
        builder.Description += "\n \n";
        builder.Description += ":moneybag: **Total Winnings:** " + pool + " " + ConfigHandler.economyConfig.currency;
        builder.Footer = new EmbedFooter() { Text = "Please stay tuned for the next one!" };

        channel.SendMessageAsync("", false, builder.Build());

        lotteryTicketMessage.DeleteAsync();
        lotteryTicketMessage = null;

        for (int i = 0; i < Bot.users.Count; i++)
        {
            Bot.users[i].lotteryTickets = 0;
        }

        winner.points += pool;
    }
}