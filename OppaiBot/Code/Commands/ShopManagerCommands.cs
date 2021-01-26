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
using static DSharpPlus.Entities.DiscordEmbedBuilder;

public class ShopAttribute : Attribute { }
public class ShopManagerCommands : BaseCommandModule
{
    [Shop]
    [Command("SetupShop")]
    [Description("This will initialize a setup process to guide you through the different settings for your shop!")]
    public async Task SetupShop(CommandContext ctx)
    {
        User user = Bot.users.Find(x => x.Id == ctx.User.Id);
        if (user.hasShop)
        {
            MessageRequirement[] requirements = new MessageRequirement[]
            {
                new MessageRequirement("Please enter the name you want for your shop", ArguementType.STRING),
                new MessageRequirement("Please enter the description you want for your shop", ArguementType.STRING),
                new MessageRequirement("Please enter the prefered currency for real payment. Write `Null` if you dont plan on using real currency.", ArguementType.STRING),
                new MessageRequirement("Please enter your payment link just write `Null` if you dont plan on using real currency ", ArguementType.STRING),
            };

            MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, user, requirements);
            request.extraInfo = "shop";

            await request.DisplayStep();
            QuerryHandler.requests.Add(request);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message, "Whoops", "You don't own a shop so what are you trying to change? Pretty sus.", EmbedColor.error);
        }
    }

    [Shop]
    [Command("Item")]
    [Description("This command needs a mode to function. The available modes are: 'add', 'remove', 'change' / 'edit' and 'restock'. Example: Item add. This will initialize a setup to follow which will guide you through.")]
    public async Task Item(CommandContext ctx, string mode)
    {
        User user = Bot.users.Find(x => x.Id == ctx.User.Id);

        if (user.hasShop)
        {
            mode = mode.ToLower();
            MessageRequirement[] req = new MessageRequirement[0];

            int cnt = mode == "add" ? 1 : 0;
            cnt += mode == "remove" ? 1 : 0;
            cnt += mode == "edit" ? 1 : 0;
            cnt += mode == "change" ? 1 : 0;
            cnt += mode == "restock" ? 1 : 0;

            if (cnt > 0)
            {
                switch (mode)
                {
                    case "add":
                        req = new MessageRequirement[]
                        {
                        new MessageRequirement("Please enter the name you wish the item to have", ArguementType.STRING),
                        new MessageRequirement("Please enter the description of the item", ArguementType.STRING),
                        new MessageRequirement("Please enter the price of the item", ArguementType.INT),
                        new MessageRequirement("Please enter the amount stock of the item", ArguementType.INT),
                        new MessageRequirement("Does this item require real cash or server currency. Write yes/true if require real cash else write no/false if server currency.", ArguementType.BOOLEAN),
                        };
                        break;

                    case "remove":
                        req = new MessageRequirement[]
                        {
                        new MessageRequirement("Please enter the name of the item you wish to remove", ArguementType.STRING),
                        };
                        break;

                    case "edit":
                        req = new MessageRequirement[]
                        {
                        new MessageRequirement("Please enter the name of the item you want to edit", ArguementType.STRING),
                        new MessageRequirement("Please enter the description of the item. Write skip to have the same as before", ArguementType.STRING),
                        new MessageRequirement("Please enter the price of the item. Write `-1` to have the same as before", ArguementType.INT),
                        new MessageRequirement("Please enter the amount stock of the item. Write `-1` to have the same as before", ArguementType.INT),
                        new MessageRequirement("Does this item require real cash or server currency. Write yes/true if require real cash else write no/false if server currency.", ArguementType.BOOLEAN),
                        };
                        break;

                    case "change":
                        req = new MessageRequirement[]
                        {
                        new MessageRequirement("Please enter the name of the item you want to edit", ArguementType.STRING),
                        new MessageRequirement("Please enter the description of the item. Write skip to have the same as before", ArguementType.STRING),
                        new MessageRequirement("Please enter the price of the item. Write `-1` to have the same as before", ArguementType.INT),
                        new MessageRequirement("Please enter the amount stock of the item. Write `-1` to have the same as before", ArguementType.INT),
                        new MessageRequirement("Does this item require real cash or server currency. Write yes/true if require real cash else write no/false if server currency.", ArguementType.BOOLEAN),
                        };
                        break;
                    case "restock":
                        req = new MessageRequirement[]
                        {
                        new MessageRequirement("Please enter the name of the item you want to restock", ArguementType.STRING),
                        };
                        break;
                }

                MessageSettingRequest request = new MessageSettingRequest(ctx.Member, ctx.Channel, user, req);
                request.extraInfo = mode;

                await request.DisplayStep();
                QuerryHandler.requests.Add(request);
            }
            else
            {
                await Bot.SendBasicEmbed(ctx.Message, "Whoops", mode + " is not a valid mode try `add`, `remove`, `edit`");
            }
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message, "Whoops", "You don't own a shop so what are you trying to change? Pretty sus.", EmbedColor.error);
        }
    }

    [Shop]
    [Command("MyShop")]
    [Description("Shortcut to show your own store if you have one!")]
    public async Task MyShop(CommandContext ctx)
    {
        User user = Bot.users.Find(x => x.Id == ctx.User.Id);
        if (user.hasShop)
        {
            await DisplayShop(user, ctx);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message, "Whoops", "You don't own a shop so what are you trying to change? Pretty sus.", EmbedColor.error);
        }
    }

    [Shop]
    [Command("Shop")]
    [Description("Use this to show a list of all stores. You can also use a shop name to look at that shops inventory. Alternatively you can use an user's ID or tagging them for similiar results!")]
    public async Task Shop(CommandContext ctx)
    {
        User[] shopOwners = Bot.users.FindAll(x => x.hasShop).ToArray();
        if(shopOwners.Length > 0)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            for (int i = 0; i < shopOwners.Length; i++)
            {
                User shopOwner = shopOwners[i];
                Console.WriteLine(shopOwner.Id);
                DiscordMember member = Bot.guild.Members[shopOwner.Id];
                builder.AddField("Owner: __" + member.DisplayName + "__", shopOwner.shopName);
            }

            DiscordMessage msg = await ctx.RespondAsync("", false, builder.Build());
            DeleteRequest req = new DeleteRequest(msg, 20);
        }
    }

    [Shop]
    [Command("Shop")]
    public async Task Shop(CommandContext ctx, DiscordMember member)
    {
        if (Bot.users.Exists(x => x.Id == member.Id))
        {
            await DisplayShop(Bot.users.Find(x => x.Id == member.Id), ctx);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message, "Whoops", "That user doesnt own a shop.", EmbedColor.error);
        }
    }

    [Shop]
    [Command("Shop")]
    public async Task Shop(CommandContext ctx, string shopName)
    {
        if(Bot.users.Exists(x => x.shopName == shopName))
        {
            await DisplayShop(Bot.users.Find(x => x.shopName == shopName), ctx);
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message, "Whoops", "A shop with that name doesn't exist.", EmbedColor.error);
        }
    }

    private async Task DisplayShop(User user, CommandContext ctx)
    {
        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
        builder.Title = user.shopName;
        
        for (int i = 0; i < user.items.Count; i++)
        {
            ItemEntity item = user.items[i];

            string priceIcon = item.requiresRealCash ? user.currency : ConfigHandler.economyConfig.currency;
            
            string fieldText = "**" + item.name + "**\n";
            fieldText += "**· · - ┈┈━━ ˚ . ✿ . ˚ ━━┈┈ - · ·** \n";
            fieldText += "> **Desc:** `" + item.description + "`\n";
            fieldText += "**· · - ┈┈━━ ˚ . ✿ . ˚ ━━┈┈ - · ·** \n";

            fieldText += "> **Item ID:** " + (i + 1) + "\n";
            fieldText += "> **Item Price:** " + item.price + " " + priceIcon + "\n";
            fieldText += "> **Stock Available:** " + item.quantity + "\n";
            fieldText += "\n";

            builder.Description += fieldText;
        }

        await ctx.RespondAsync("", false, builder.Build());
    }

    [Shop]
    [Command("Buy")]
    [Description("Use this to buy certain items from shops. First you write the name of the shop then the name of the item. If any of these have spaces in their name please surround name with quotation marks.")]
    public async Task Buy(CommandContext ctx, string shopName, string itemName)
    {
        if(Bot.users.Exists(x => x.shopName == shopName))
        {
            User owner = Bot.users.Find(x => x.shopName == shopName);
            User buyer = Bot.GetUserByID(ctx.Member);
;
            DiscordMember member = Bot.guild.Members[owner.Id];

            if (owner.items.Exists(x => x.name == itemName))
            {
                ItemEntity item = owner.items.Find(x => x.name == itemName);
                if (!item.requiresRealCash)
                {
                    if (item.price <= buyer.points)
                    {
                        if (item.quantity > 0)
                        {
                            buyer.points -= item.price;
                            item.quantity--;

                            string desc = "Congrats you bought " + itemName + "! Now " + member.Mention + " its your turn to send the goods to " + ctx.Member.Mention;

                            await Bot.SendBasicEmbed(ctx.Channel, "Hurray!", desc);
                        }
                        else { await Bot.SendBasicEmbed(ctx.Channel, "Whoops", "There is not any stock of this item. Wait for owner to use item restock!"); }
                    }

                    else { await Bot.SendBasicEmbed(ctx.Channel, "Whoops!", "Sorry you dont have enough for this!"); }
                }
                else
                {
                    string transcationID = Guid.NewGuid().ToString();

                    string desc = "Thanks for order " + itemName + " please use following link for payment: \n";
                    desc += "`" + owner.paymentLink + "` \n";
                    desc += "In the notes please write: ";
                    desc += "`" + transcationID + "` \n";
                    desc += "This is to confirm your payment";

                    await Bot.SendBasicEmbed(ctx.Channel, "Ok here we go!", desc);

                    DiscordChannel channel = await member.CreateDmChannelAsync();
                    if (channel != null)
                    {
                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                        builder.Description = "An user tried to buy an item we need you to double check if everything is in order. \n";
                        builder.Description += "below you can find the order details make sure the note on the transcation is same is order id";

                        builder.AddField("Server", Bot.guild.Name, true);
                        builder.AddField("Buyer", ctx.Member.DisplayName + "#" + ctx.Member.Discriminator, true);
                        builder.AddField("Item Bought", item.name + " " + item.price + owner.currency, true);
                        builder.AddField("Order ID", transcationID, true);

                        builder.WithFooter("The reactions doesn't do anything (yet) but they help you keep track of which orders are still pending!");

                        DiscordMessage msg = await channel.SendMessageAsync("",false, builder.Build());

                        DiscordEmoji thumpsUp = DiscordEmoji.FromName(Bot.client, ":thumbsup:");
                        DiscordEmoji thumpsDown = DiscordEmoji.FromName(Bot.client, ":thumbsdown:");

                        await msg.CreateReactionAsync(thumpsUp);
                        await msg.CreateReactionAsync(thumpsDown);
                    }
                    else
                    {
                        Console.WriteLine("User dont have dm rights :C");
                    }
                }
            }
            else
            {
                await Bot.SendBasicEmbed(ctx.Message, "Whoops!", "The item `" + itemName + "` Doesnt exist in this shop!", EmbedColor.error);
            }
        }
        else
        {
            await Bot.SendBasicEmbed(ctx.Message, "Whoops", "A shop with that name doesn't exist.", EmbedColor.error);
        }
    }
}
