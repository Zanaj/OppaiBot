using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameAttribute : Attribute { }
public class RouletteCommands : BaseCommandModule
{
    public static List<int> reds = new List<int>
    {
            1, 3, 5, 7, 9, 12,
            14, 16, 18, 19, 21, 23,
            25, 27, 30, 32, 34, 36
    };

    public static List<int> blacks = new List<int>
    {
        2, 4, 6, 8, 10, 11,
        13, 15, 17, 20, 22, 24,
        26, 28, 29, 31, 33, 35
    };

    public bool CheckBet(CommandContext ctx, int bet)
    {
        //User user = Bot.GetUserByID(ctx.Member);
        //if(user.points >= bet)
        //{
        //    user.points -= bet;
        //    return true;
        //}
        //else { return false; }
        return true;
    }

    [Game]
    [Command("rStraight")]
    [Description("Bet on a single number first write the number then the bet")]
    public async Task Roulette_Straight(CommandContext ctx, int number, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            RouletteMaster.AddBet("Straight", ctx.Channel, ctx.Member, bet, 35, number);
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }   
    }

    [Game]
    [Command("rRow")]
    [Description("Bets on either 0 or 00. You only need to write how much you wanna bet")]
    public async Task Roulette_Row(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            RouletteMaster.AddBet("Row", ctx.Channel, ctx.Member, bet, 17, 0,-1);
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rSplit")]
    [Description("Bets on two ajoining numbers. Write the two numbers and then the bet")]
    public async Task Roulette_Split(CommandContext ctx, int a, int b, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            if (Math.Abs(a - b) == 1)
            {
                RouletteMaster.AddBet("Split", ctx.Channel, ctx.Member, bet, 17, a, b);
                await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
            }
            else
            {
                await Bot.SendBasicEmbed(ctx.Message.Channel, "Hold up.", "The numbers " + a + " and " + b + " is not next to eachother.", EmbedColor.server, true);
            }
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rStreet")]
    [Description("Bets on a three numbers. Write your middle number and the bet")]
    public async Task Roulette_Street(CommandContext ctx, int middle, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            int a = middle - 1;
            int b = middle;
            int c = middle + 1;

            int cnt = a <= 0 ? 1 : 0;
            cnt += c > 36 ? 1 : 0;

            if (cnt == 0)
            {
                RouletteMaster.AddBet("Street", ctx.Channel, ctx.Member, bet, 11, a, b, c);
                await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
            }
            else
            {
                string text = " You need to write the middle number of the 3. If you wanna bet on 4,5,6 then write 5.";
                await Bot.SendBasicEmbed(ctx.Message.Channel, "Hold up.", text, EmbedColor.server, true);
            }
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rBasket")]
    [Description("Bets on 0, 00, 1, 2 and 3. Just need to write how much you bet")]
    public async Task Roulette_Basket(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            RouletteMaster.AddBet("Basket", ctx.Channel, ctx.Member, bet, 6, 0, -1, 1, 2, 3);
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rColumn")]
    [Description("You bet on a whole column just write which column number you wanna bet on (1-3) then your bet")]
    public async Task Roulette_Column(CommandContext ctx, int number, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            List<int> vals = new List<int>();
            if (number < 1 || number > 3)
                return;

            number--;
            vals.Add(1 + number);
            vals.Add(4 + number);
            vals.Add(7 + number);
            vals.Add(10 + number);
            vals.Add(13 + number);
            vals.Add(16 + number);
            vals.Add(19 + number);
            vals.Add(22 + number);
            vals.Add(25 + number);
            vals.Add(28 + number);
            vals.Add(31 + number);
            vals.Add(34 + number);

            RouletteMaster.AddBet((number+1)+" Column", ctx.Channel, ctx.Member, bet, 2, vals.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rDozen")]
    [Description("You bet on a dozen just write which dozen you wanna bet on (1-3) then your bet")]
    public async Task Roulette_Dozen(CommandContext ctx, int number, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            List<int> vals = new List<int>();
            if (number < 1 || number > 3)
                return;

            number--;
            for (int i = 1; i < 12; i++)
            {
                int offset = number * 12;
                vals.Add(offset + i);
            }

            RouletteMaster.AddBet((number + 1) + " Dozen", ctx.Channel, ctx.Member, bet, 2, vals.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rEven")]
    [Description("You bet on all even numbers through 1-36 just write your bet")]
    public async Task Roulette_Even(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            List<int> vals = new List<int>();
            for (int i = 1; i < 36; i++)
            {
                if (i % 2 == 0)
                    vals.Add(i);
            }

            RouletteMaster.AddBet("Even", ctx.Channel, ctx.Member, bet, 1.01f, vals.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rOdd")]
    [Aliases("rOdds")]
    [Description("You bet on all odd numbers through 1-36 just write your bet")]
    public async Task Roulette_Odd(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            List<int> vals = new List<int>();
            for (int i = 1; i < 36; i++)
            {
                if (i % 2 != 0)
                    vals.Add(i);
            }

            RouletteMaster.AddBet("Odd", ctx.Channel, ctx.Member, bet, 1.01f, vals.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rRed")]
    [Aliases("rReds")]
    [Description("You bet on all the red numbers just write your bet")]
    public async Task Roulette_Red(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            RouletteMaster.AddBet("Red", ctx.Channel, ctx.Member, bet, 1.01f, reds.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rBlack")]
    [Aliases("rBlacks")]
    [Description("You bet on all the black numbers just write your bet")]
    public async Task Roulette_Black(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            RouletteMaster.AddBet("Black", ctx.Channel, ctx.Member, bet, 1.01f, blacks.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rFirstHalf")]
    [Description("You bet on all numbers from 1-18 just write your bet")]
    public async Task Roulette_FirstHalf(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            List<int> vals = new List<int>();
            for (int i = 1; i < 18; i++)
            {
                vals.Add(i);
            }

            RouletteMaster.AddBet("First Half", ctx.Channel, ctx.Member, bet, 1.01f, vals.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }

    [Game]
    [Command("rSecondHalf")]
    [Description("You bet on all numbers from 19-36 just write your bet")]
    public async Task Roulette_SecondHalf(CommandContext ctx, int bet)
    {
        if (CheckBet(ctx, bet))
        {
            List<int> vals = new List<int>();
            for (int i = 19; i < 36; i++)
            {
                vals.Add(i);
            }

            RouletteMaster.AddBet("Seconds Half", ctx.Channel, ctx.Member, bet, 1.01f, vals.ToArray());
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Good luck!", "Thanks for the bet!", EmbedColor.server, true);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message.Channel, "Too fast there", "You do not have enough for that bet.", EmbedColor.server, true);
        }
    }
}
