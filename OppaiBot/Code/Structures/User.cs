using DSharpPlus.Entities;
using MessageReqDefinations;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ExpType
{
    Voice,
    Message,
    Admin,
}

public class User : IMessageCallback
{
    public const string SAVE_FOLDER = "Data";
    public const string USER_FILE_NAME = "users.json";

    public int lotteryTickets { get; set; }
    public ulong Id { get; set; }
    public int points { get; set; }
    public float voice_exp { get; set; }
    public float message_exp { get; set; }
    public int level { get; set; }
    public float exp { get; set; }
    public int quantityBought { get; set; }
    public int quantitySold { get; set; }
    public int lastRecordedLevel { get; set; }
    public string shopName { get; set; }
    public bool hasShop { get; set; }
    public string paymentLink { get; set; }
    public string currency { get; set; }
    public string shopDescription { get; set; }

    public List<ItemEntity> items { get; set; }

    public DateTime lastClaimWork { get; set; }
    public DateTime lastClaimDaily { get; set; }
    public string name { get; set; }

    public User() { }

    public User(DiscordMember member)
    {
        Id = member.Id;
        name = member.DisplayName;
        points = 0;
        hasShop = false;
        shopName = "NA";
        currency = "$";
        paymentLink = "www.google.dk";
        items = new List<ItemEntity>();

        lastRecordedLevel = 1;

        lastClaimWork = DateTime.MinValue;
        lastClaimDaily = DateTime.MinValue;
    }

    public void Callback(MessageSettingRequest request)
    {
        MessageRequirement[] msg = request.queue.ToArray();
        string mode = (string)request.extraInfo;

        try
        {
            switch (mode)
            {
                case "add":
                    AddItem(msg,request.channel);
                    break;

                case "remove":
                    RemoveItem(msg, request.channel);
                    break;

                case "edit":
                    EditItem(msg, request.channel);
                    break;

                case "change":
                    EditItem(msg, request.channel);
                    break;
                case "restock":
                    RestockItem(msg, request.channel);
                    break;
                case "shop":
                    SetupShop(msg, request.channel);
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public void GiveExp(float exp, DiscordMember member, ExpType type)
    {
        switch (type)
        {
            case ExpType.Voice:
                voice_exp += exp;
                break;
            case ExpType.Message:
                message_exp += exp;
                break;
        }

        this.exp += exp;
        float nextLevelExp = ConfigHandler.levelConfig.GetExpForLevel(level + 1);

        if (!member.IsBot)
        {
            if (exp >= nextLevelExp)
            {
                if (ConfigHandler.baseConfig.levelUpChannel > 100)
                {
                    level++;

                    DiscordChannel channel = Bot.guild.Channels[ConfigHandler.baseConfig.levelUpChannel];
                    string desc = ConfigHandler.baseConfig.levelMsg;
                    desc = desc.Replace("{user}", member.DisplayName);
                    desc = desc.Replace("{level}", level.ToString());

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                    builder.Title = member.DisplayName + " Leveled up!";
                    builder.Description = desc;

                    channel.SendMessageAsync("", false, builder.Build());

                    exp = 0;
                }
            }
        }
    }

    public void AddItem(MessageRequirement[] msg, DiscordChannel channel)
    {
        string aName = (string)msg[0].value;

        if (!items.Exists(x => x.name == aName))
        {
            string aDesc = (string)msg[1].value;
            int aPrice = (int)msg[2].value;
            int aStock = (int)msg[3].value;
            bool aUnit = (bool)msg[4].value;

            ItemEntity item = new ItemEntity()
            {
                name = aName,
                description = aDesc,
                price = aPrice,
                maxQuantity = aStock,
                quantity = aStock,
                requiresRealCash = aUnit,
            };

            items.Add(item);
        }
        else { Bot.SendBasicEmbed(channel, "Whoops", "An item with that name already exist in your shop!", EmbedColor.error); }
    }

    public void RemoveItem(MessageRequirement[] msg, DiscordChannel channel)
    {
        string rName = (string)msg[0].value;

        if (items.Exists(x => x.name == rName))
        {
            items.RemoveAt(items.FindIndex(x => x.name == rName));
        }
        else { Bot.SendBasicEmbed(channel, "Whoops", "An item with that name doesnt exist in your shop!", EmbedColor.error); }
    }

    public void EditItem(MessageRequirement[] msg, DiscordChannel channel)
    {
        string cName = (string)msg[0].value;

        if (items.Exists(x => x.name == cName))
        {
            ItemEntity item = items.Find(x => x.name == cName);

            if (((string)msg[1].value).ToLower() != "skip")
                item.description = (string)msg[1].value;

            if (((int)msg[2].value) != -1)
                item.price = (int)msg[2].value;

            if (((int)msg[3].value) != -1)
            {
                int stock = (int)msg[3].value;
                if (item.quantity > stock)
                    item.quantity = stock;

                item.maxQuantity = stock;
            }

            item.requiresRealCash = (bool)msg[4].value;
        }
        else { Bot.SendBasicEmbed(channel, "Whoops", "An item with that name doesnt exist in your shop!", EmbedColor.error); }
    }

    public void RestockItem(MessageRequirement[] msg, DiscordChannel channel)
    {
        string rsName = (string)msg[0].value;
        if (items.Exists(x => x.name == rsName))
        {
            ItemEntity item = items.Find(x => x.name == rsName);
            item.quantity = item.maxQuantity;
        }
        else { Bot.SendBasicEmbed(channel, "Whoops", "An item with that name doesnt exist in your shop!", EmbedColor.error); }
    }

    public void SetupShop(MessageRequirement[] msg, DiscordChannel channel)
    {
        shopName = (string)msg[0].value;
        shopDescription = (string)msg[1].value;
        currency = (string)msg[2].value;
        paymentLink = (string)msg[3].value;
    }

}