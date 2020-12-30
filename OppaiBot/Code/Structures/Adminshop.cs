using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MessageReqDefinations;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AdminShopItem
{
    public string name;
    public string desc;
    public int price;
    public string type;
    public ulong msgID;
    public DiscordEmoji emoji;
    public string value;
}

public class Adminshop : IMessageCallback
{
    public static Adminshop instance;
    public List<AdminShopItem> items;

    public static void Initialize()
    {
        if (instance == null)
        {
            instance = new Adminshop();
            Adminshop.instance.items = new List<AdminShopItem>();
            //TODO: load all the previous stuff etc.
        }
    }

    public void Callback(MessageSettingRequest request)
    {
        if (request.extraInfo.ToString() == "add")
            AddItem(request);
    }

    public void AddItem(MessageSettingRequest request)
    {
        MessageRequirement[] msg = request.queue.ToArray();

        try
        {
            string name = (string)msg[0].value;
            string desc = (string)msg[1].value;
            int price = (int)msg[2].value;
            string type = (string)msg[3].value;
            string value = (string)msg[4].value;
            string emojiID = (string)msg[5].value;
            ulong channelID = (ulong)msg[6].value;

            DiscordChannel ch = Bot.guild.GetChannel(channelID);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = name;

            ulong id = 0;

            if (type == "role")
            {
                builder.Title = name;
                builder.Description += "__**Unlocks:**__ \n ";
                
                string str = value
                    .Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("@", string.Empty)
                    .Replace("&", string.Empty);

                
                if(ulong.TryParse(str, out id))
                {
                    builder.Description += Bot.guild.Roles[id].Mention + "\n \n";
                }
                else { throw new InvalidCastException(); }

            }
            builder.Description += "__**Description:**__ \n";
            builder.Description += desc + "\n \n";
            builder.Description += "__**Price:**__ \n";
            builder.Description += price.ToString() + " ";
            builder.Description += ConfigHandler.economyConfig.currency;

            DiscordMessage message = ch.SendMessageAsync("", false, builder.Build()).Result;
            DiscordEmoji rectionEmoji = DiscordEmoji.FromName(Bot.client, ":sip:");

            if (emojiID.Contains('<'))
            {
                string[] split = emojiID.Split(':');
                rectionEmoji = DiscordEmoji.FromName(Bot.client, ":"+split[1]+":");
            }
            else
            {
                rectionEmoji = DiscordEmoji.FromUnicode(Bot.client, emojiID);
            }

            message.CreateReactionAsync(rectionEmoji);

            AdminShopItem item = new AdminShopItem
            {
                name = name,
                desc = desc,
                price = price,
                type = type,
                msgID = message.Id,
                emoji = rectionEmoji,
                value = value,
            };

            if (type == "role")
                item.value = id.ToString();

            items.Add(item);
            //TODO: save database
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: " + e.Message);
            throw;
        }
    }

    public void CheckRection(MessageReactionAddEventArgs e)
    {
        if (items.Exists(x => x.msgID == e.Message.Id && x.emoji == e.Emoji))
        {
            AdminShopItem item = items.Find(x => x.msgID == e.Message.Id);
            if (!e.User.IsBot)
            {
                User user = Bot.GetUserByID(e.User);
                if (user.points >= item.price)
                {
                    e.Message.DeleteReactionAsync(item.emoji, e.User);

                    if (item.type == "role")
                    {
                        ulong id = user.Id;
                        DiscordMember member = Bot.guild.Members[id];

                        ulong roleID = ulong.Parse(item.value);
                        DiscordRole role = Bot.guild.Roles[roleID];

                        if (!member.Roles.Contains(role))
                        {
                            member.GrantRoleAsync(role);
                            user.points -= item.price;
                        }
                    }
                }
            }
        }
    }
}
