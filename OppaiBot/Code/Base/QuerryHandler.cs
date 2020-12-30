using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MessageReqDefinations;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class QuerryHandler
{
    public static List<MessageSettingRequest> requests;

    public static void Initialize()
    {
        requests = new List<MessageSettingRequest>();
    }

    public static async Task HandleMessage(MessageCreateEventArgs e)
    {
        if (e.Guild == Bot.guild)
        {
            DiscordMember member = e.Guild.Members[e.Message.Author.Id];
            DiscordChannel channel = e.Channel;

            if (requests.Count > 0)
            {
                if (requests.Exists(x => x.caller == member))
                {
                    MessageSettingRequest req = requests.Find(x => x.caller == member);
                    if (req.channel == channel)
                    {
                        if (req.CheckStep(e))
                        {
                            req.step++;
                            if (req.step < req.queue.Count)
                            {
                                await req.DisplayStep();
                            }
                            else
                            {
                                string description = "We completed your request!";
                                description += "\n with the values of: \n";

                                for (int i = 0; i < req.queue.Count; i++)
                                {
                                    MessageRequirement requirement = req.queue[i];
                                    description += "`" + requirement.value.ToString() + "`, ";
                                }

                                try
                                {
                                    req.callback.Callback(req);
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(req.callback.GetType().FullName + " " + exception.Message);
                                }

                                await Bot.SendBasicEmbed(e.Channel, "Completed", description, EmbedColor.success, true);
                                requests.Remove(req);
                            }
                        }
                        else
                        {
                            string description = "Couldn't process: `" + e.Message + "` as a `";
                            description += req.queue[req.step].type;
                            description += req.queue[req.step].isArray ? "[]" : "";
                            description += "`";

                            await Bot.SendBasicEmbed(e.Channel, "Error", description, EmbedColor.error, true);
                            
                            requests.Remove(req);
                        }
                    }
                }
            }
        }
    }
}
