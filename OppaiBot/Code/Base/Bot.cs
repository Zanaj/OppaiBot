using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using System.IO;
using DSharpPlus.Entities;
using System.Timers;
using Newtonsoft.Json;

namespace OppaiBot
{
    public class DeleteRequest
    {
        public DiscordMessage msg;
        public DateTime enqueuedAt;
        public int secondsLasting;

        public DeleteRequest(DiscordMessage _msg, int secs)
        {
            msg = _msg;
            enqueuedAt = DateTime.Now;
            secondsLasting = secs;
        }

        public bool shouldDequeue()
        {
            TimeSpan span = DateTime.Now - enqueuedAt;
            return span.TotalSeconds >= secondsLasting;
        }
    }

    public static class Bot
    {
        public static DiscordClient client;
        public static DiscordGuild guild;
        public static CommandsNextExtension commands;
        public static List<User> users;
        public static Timer serverTickTimer;

        public static DateTime lastDrop;
        public static DiscordMessage dropMessage;
        public static int amountDropped;
        public static List<DeleteRequest> toDelete;


        public static Dictionary<int, ulong> levelRoles; 
        public static async Task Initialize()
        {
            levelRoles = new Dictionary<int, ulong>();
            toDelete = new List<DeleteRequest>();

            ConfigHandler.InitializeConfigs();
            Adminshop.Initialize();
            QuerryHandler.Initialize();
            GiveawayMaster.Initialize(new GiveawayMaster());

            lastDrop = DateTime.MinValue;
            dropMessage = null;

            string path = System.Environment.CurrentDirectory;
            path += "\\" + User.SAVE_FOLDER + "\\" + User.USER_FILE_NAME;
            if (File.Exists(path))
                //users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(path));
                users = new List<User>();
            else
                users = new List<User>();
            
            DiscordConfiguration config = new DiscordConfiguration()
            {
                Token = ConfigHandler.baseConfig.token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Critical,
            };

            client = new DiscordClient(config);
            client.MessageCreated += OnMessageRecieved;
            client.MessageReactionAdded += OnReactionAdded;
            
            InitializeCommands();
            ImageProcesser.Initialize();
            commands.SetHelpFormatter<HelpFormatter>();

            Timer serverTickTimer = new Timer();
            serverTickTimer.Interval = ConfigHandler.baseConfig.serverTickRate;
            serverTickTimer.Elapsed += OnServerTick;
            serverTickTimer.AutoReset = true;
            serverTickTimer.Start();

            Console.WriteLine("Done Initializing");
            
            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task OnReactionAdded(MessageReactionAddEventArgs e)
        {
            Adminshop.instance.CheckRection(e);
            return Task.CompletedTask;
        }

        private static void OnServerTick(object sender, ElapsedEventArgs e)
        {
            for (int i = toDelete.Count - 1; i >= 0; i--)
            {
                if (toDelete[i].shouldDequeue())
                {
                    toDelete[i].msg.DeleteAsync();
                    toDelete.RemoveAt(i);
                }
            }

            int ping = client.Ping;

            if (users.Count > 0)
            {

                //string json = JsonConvert.SerializeObject(users);

                //using (StreamWriter sw = new StreamWriter(ConfigHandler.UserDatabaseStream, new UTF8Encoding(false)))
                //    sw.Write(json);

            }
        }

        public static DiscordEmbed GetBasicEmbed(string title, string description, EmbedColor color = EmbedColor.server)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Title = title;
            embed.Description = description;

            switch (color)
            {
                case EmbedColor.error:
                    embed.Color = new DiscordColor(1, 0, 0);
                    break;
                case EmbedColor.success:
                    embed.Color = new DiscordColor(0, 1, 0);
                    break;
                default:

                    BaseConfig cfg = ConfigHandler.baseConfig;
                    
                    float r = cfg.serverColorR;
                    float g = cfg.serverColorG;
                    float b = cfg.serverColorB;

                    embed.Color = new DiscordColor(r, g, b);
                    break;
            }

            return embed.Build();
        }

        public static async Task SendBasicEmbed(DiscordMessage msg, string title, string desc, EmbedColor color = EmbedColor.server)
        {
            DiscordEmbed embed = GetBasicEmbed(title, desc, color);
            await msg.RespondAsync("",false,embed);
        }

        public static async Task SendBasicEmbed(DiscordChannel channel, string title, string desc, EmbedColor color = EmbedColor.server, bool AutoDelete = false)
        {
            DiscordEmbed embed = GetBasicEmbed(title, desc, color);
            DiscordMessage msg = await channel.SendMessageAsync("", false, embed);

            if(AutoDelete)
            {
                DeleteRequest req = new DeleteRequest(msg, 15);
                toDelete.Add(req);
            }
        }

        private static void InitializeCommands()
        {
            var config = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "Oq", "OQ", "oq" },
                //StringPrefixes = new[] { "!" },
                EnableDms = false,
                EnableMentionPrefix = true,

            };

            commands = client.UseCommandsNext(config);
            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<GeneralCommands>();
            commands.RegisterCommands<ShopManagerCommands>();
            commands.RegisterCommands<AdminShopCommands>();
            commands.RegisterCommands<RouletteCommands>();
        }

        private static Task OnMessageRecieved(MessageCreateEventArgs e)
        {
            if (!e.Channel.IsPrivate)
            {
                QuerryHandler.HandleMessage(e);

                if (guild == null)
                    guild = e.Guild;

                DiscordMember member = e.Guild.GetMemberAsync(e.Author.Id).Result;
                User user = GetUserByID(member);

                float min = ConfigHandler.levelConfig.min_msg_exp;
                float max = ConfigHandler.levelConfig.max_msg_exp;

                float scaler = (float)new Random().NextDouble();
                float difference = max - min;
                float exp = difference * scaler;
                exp += min;

                user.GiveExp(exp, member, ExpType.Message);

                TimeSpan span = DateTime.Now - lastDrop;
                if (span.TotalSeconds >= ConfigHandler.economyConfig.appearCooldown)
                {
                    if (dropMessage == null)
                    {
                        int minDrop = ConfigHandler.economyConfig.minDropReward;
                        int maxDrop = ConfigHandler.economyConfig.maxDropReward;

                        amountDropped = new Random().Next(minDrop, maxDrop);
                        lastDrop = DateTime.Now;

                        string currencyName = ConfigHandler.economyConfig.currency;


                        DiscordEmbed embed = Bot.GetBasicEmbed
                            ("Oh no look there!", "Someone dropped " + amountDropped + " " + currencyName);

                        dropMessage = e.Message.Channel.SendMessageAsync("", false, embed).Result;
                    }
                }

                return Task.CompletedTask;
            }
            else { return Task.CompletedTask; }
        }

        public static User GetUserByID(DiscordMember member)
        {
            if (!users.Exists(x => x.Id == member.Id))
                users.Add(new User(member));

            return users.Find(x => x.Id == member.Id);
        }

        public static User GetUserByID(DiscordUser user)
        {
            DiscordMember member = guild.Members[user.Id];
            return GetUserByID(member);
        }
    }
}
