using DSharpPlus.Entities;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

public class Bet
{
    public string betName;
    public DiscordMember better;
    public int amount;
    public float winBack;
    public List<int> winningNumbers;

    public Bet(string betName, DiscordMember better, float winBack, int amount, params int[] numbers)
    {
        winningNumbers = new List<int>();

        this.betName = betName;
        this.better = better;
        this.amount = amount;
        this.winBack = winBack;
        this.winningNumbers.AddRange(numbers);
    }
}

public static class RouletteMaster
{
    //TODO: Clean up messages
    public static float turningTime = 1000*15;
    public static bool isRunning;
    public static List<Bet> bets;

    private static float TimeLeft = 0;
    private static bool hasInit = false;
    private static DiscordMessage message;
    private static DiscordChannel ch;
    private static Timer tickTimer;

    public static void AddBet(string betName, DiscordChannel channel, DiscordMember better, int amount, float winBack, params int[] winningNumbers)
    {
        if (!hasInit)
        {
            bets = new List<Bet>();
            hasInit = true;
        }

        if (!isRunning)
        {
            ch = channel;
            StartNewGame();
            message = ch.SendMessageAsync("", false, GetEmbed()).Result;
        }

        if(ch.Id != channel.Id)
        {
            Bot.SendBasicEmbed(channel, "Theres a game going on another place!", "Hey theres a game going on at " + channel.Mention + " please go place your bets there!");
        }
        else
        {
            bets.Add(new Bet(betName, better, winBack, amount, winningNumbers));
            message.ModifyAsync("", GetEmbed());
        }
    }

    private static void StartNewGame()
    {
        tickTimer = new Timer();
        tickTimer.Interval = 250;
        tickTimer.Elapsed += OnTick;
        tickTimer.AutoReset = true;

        isRunning = true;
        TimeLeft = turningTime;

        tickTimer.Start();
    }


    private static DiscordEmbed GetEmbed()
    {
        
        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
        builder.Title += ":red_circle: ";
        builder.Title += "Roulette ongoing!";
        builder.Title += " :red_circle:";

        builder.Description += "__**Current Bets:**__ \n";
        List<Bet> channelsBet = bets;

        for (int i = 0; i < channelsBet.Count; i++)
        {
            Bet bet = channelsBet[i];

            builder.Description += "**" + bet.better.DisplayName + "**: ";
            builder.Description += bet.betName + " for " + bet.amount;
            builder.Description += "\n";
        }

        return builder.Build();
    }
    private static void OnTick(object sender, ElapsedEventArgs e)
    {
        Console.WriteLine("Time Left: "+TimeLeft);
        if(TimeLeft - 250 <= 0)
        {
            isRunning = false;
            Random rng = new Random();
            int n = rng.Next(-1, 36);

            Bet[] winningBets = bets.FindAll(x => x.winningNumbers.Contains(n)).ToArray();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = "Winning number was " + n + "!";

            builder.Description += "__**Winning bets**__ \n";
            for (int i = 0; i < winningBets.Length; i++)
            {
                Bet bet = winningBets[i];
                builder.Description += "**"+bet.better.Username+"**:";
                builder.Description += bet.betName + " winning " + Math.Floor(bet.amount * bet.winBack);
                builder.Description += "\n";
            }

            if (n <= 0)
                builder.Color = DiscordColor.DarkGreen;
            else if (RouletteCommands.blacks.Contains(n))
                builder.Color = DiscordColor.Black;
            else
                builder.Color = DiscordColor.Red;

            ch.SendMessageAsync("", false, builder.Build());

            TimeLeft = turningTime;
            ch = null;
            bets.Clear();
            isRunning = false;

            tickTimer.Stop();
        }
        else { TimeLeft -= 250; }
    }
}
