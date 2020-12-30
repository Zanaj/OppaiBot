using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using OppaiBot;

public class HelpFormatter : BaseHelpFormatter
{
    private string[] acceptedCategories = new string[]
    {
        "admin",
        "general",
        "shop",
        "game"
    };

    private DiscordEmbedBuilder EmbedBuilder { get; }
    public HelpFormatter(CommandContext ctx) : base(ctx)
    {
        this.EmbedBuilder = new DiscordEmbedBuilder();
        this.EmbedBuilder.Title = "__Help__";

        List<Command> c = ctx.CommandsNext.RegisteredCommands.Values.ToList();
        string[] splits = ctx.Message.Content.Split(' ');
        if(splits.Length <= 2)
        {
            ShowAllGroupCommands(c);
        }
        else
        {
            string arguement = splits[2].ToLower();
            if(c.Exists(cmd => cmd.Name.ToLower() == arguement))
            {
                AddCommandToEmbed(c.Find(x => x.Name.ToLower() == arguement));
            }
            else if (arguement == "admin"||arguement=="shop"||arguement=="general"||arguement=="game")
            {
                string name = "";
                List<Command> cmds = new List<Command>();

                switch (arguement)
                {
                    case "general":
                        name = "general";
                        cmds = c.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is GeneralAttribute));
                        break;

                    case "shop":
                        name = "shop";
                        cmds = c.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is ShopAttribute));
                        break;

                    case "admin":
                        name = "admin";
                        cmds = c.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is AdminAttribute));
                        Console.WriteLine(cmds.Count);
                        break;
                    case "game":
                        name = "game";
                        cmds = c.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is GameAttribute));
                        Console.WriteLine(cmds.Count);
                        break;

                }

                AddCommandsFromGroup(name, cmds.ToArray());

                ctx.RespondAsync("", false, EmbedBuilder.Build());
            }
            else { ShowAllGroupCommands(c); }
        }
    }

    public override CommandHelpMessage Build()
    {
        return new CommandHelpMessage(embed: this.EmbedBuilder.Build());
    }

    public override BaseHelpFormatter WithCommand(Command command){return this;}
    public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands) { return this; }

    public void ShowAllGroupCommands(List<Command> cmds)
    {
        List<Command> adminCmds = cmds.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is AdminAttribute));
        List<Command> shopCmds = cmds.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is ShopAttribute));
        List<Command> generalCmds = cmds.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is GeneralAttribute));
        List<Command> rouletteCmds = cmds.FindAll(x => x.CustomAttributes.ToList().Exists(a => a is GameAttribute));

        this.EmbedBuilder.Description += "**__Admin__** \n";
        for (int i = 0; i < adminCmds.Count; i++)
        {
            Command c = adminCmds[i];
            this.EmbedBuilder.Description += "`"+c.Name+"`";

            if (c != adminCmds.Last())
                this.EmbedBuilder.Description += ", ";
        }
        this.EmbedBuilder.Description += "\n";

        this.EmbedBuilder.Description += "**__General__** \n";
        for (int i = 0; i < generalCmds.Count; i++)
        {
            Command c = generalCmds[i];
            this.EmbedBuilder.Description += "`" + c.Name + "`";

            if (c != generalCmds.Last())
                this.EmbedBuilder.Description += ", ";
        }
        this.EmbedBuilder.Description += "\n";

        this.EmbedBuilder.Description += "**__Shop__** \n";
        for (int i = 0; i < shopCmds.Count; i++)
        {
            Command c = shopCmds[i];
            this.EmbedBuilder.Description += "`" + c.Name + "`";

            if (c != shopCmds.Last())
                this.EmbedBuilder.Description += ", ";
        }
        this.EmbedBuilder.Description += "\n";

        this.EmbedBuilder.Description += "**__Roulette Bets__** \n";
        for (int i = 0; i < rouletteCmds.Count; i++)
        {
            Command c = rouletteCmds[i];
            this.EmbedBuilder.Description += "`" + c.Name + "`";

            if (c != rouletteCmds.Last())
                this.EmbedBuilder.Description += ", ";
        }
        this.EmbedBuilder.Description += "\n";

    }

    public void AddCommandsFromGroup(string name, params Command[] cmds)
    {
        this.EmbedBuilder.Title = name;

        for (int j = 0; j < cmds.Length; j++)
        {
            Command c = cmds[j];
            AddCommandToEmbed(c);
        }
        this.EmbedBuilder.Description += "\n";
    }

    public void AddCommandToEmbed(Command command)
    {
        string fieldText = "**" + command.Name + "**\n";
        fieldText += "**· · - ┈┈━━ ˚ . ✿ . ˚ ━━┈┈ - · ·** \n";

        if (command.Aliases.Count > 0)
        {
            fieldText += "> **Aliases:** ";
            for (int i = 0; i < command.Aliases.Count; i++)
            {
                fieldText += "`" + command.Aliases[i] + "`";

                if (command.Aliases[i] != command.Aliases.Last())
                    fieldText += ", ";
            }
            fieldText += "\n";
        }
        
        fieldText += "> **Desc:** `" + command.Description + "`\n";
        fieldText += "\n";

        this.EmbedBuilder.Description += fieldText;
    }
}
