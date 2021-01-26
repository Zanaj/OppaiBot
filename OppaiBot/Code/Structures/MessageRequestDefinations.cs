using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageReqDefinations
{
    public enum ArguementType
    {
        INT,
        FLOAT,
        STRING,
        USER,
        MEMBER,
        CHANNEL,
        BOOLEAN,
        ROLE,
    }

    public class MessageRequirement
    {
        public string valueName;
        public ArguementType type;
        public bool isArray;
        public object value;

        public MessageRequirement(string valueName, ArguementType type, bool isArray = false)
        {
            this.valueName = valueName;
            this.type = type;
            this.isArray = isArray;
        }
    }

    public class MessageSettingRequest
    {
        public DiscordMember caller;
        public DiscordChannel channel;
        public int step = 0;
        public IMessageCallback callback;
        public List<MessageRequirement> queue;
        public DiscordMessage lastDisplay;
        public object extraInfo;

        public MessageSettingRequest(DiscordMember caller, DiscordChannel channel, IMessageCallback callback, params MessageRequirement[] steps)
        {
            this.caller = caller;
            this.channel = channel;
            this.callback = callback;
            queue = new List<MessageRequirement>();
            queue.AddRange(steps);
        }

        public async Task DisplayStep()
        {
            DiscordEmbed embed = Bot.GetBasicEmbed("Asking for a friend: ",queue[step].valueName);

            lastDisplay = await channel.SendMessageAsync("", false, embed);
        }

        public bool CheckStep(MessageCreateEventArgs e)
        {
            bool rtn = false;

            var curr = queue[step];

            if (lastDisplay != null)
                lastDisplay.DeleteAsync();

            e.Message.DeleteAsync();

            if (queue[step].isArray)
                rtn = CheckArray(queue[step].type, e);
            else
                rtn = CheckNonArray(queue[step].type, e);

            return rtn;
        }
        
        public bool CheckArray(ArguementType expected, MessageCreateEventArgs e)
        {
            bool rtn = false;
            string str = e.Message.Content;
            string[] array = str.Split(','); 

            switch (queue[step].type)
            {
                case ArguementType.INT:
                    rtn = array.Length > 0;
                    if (rtn)
                    {
                        List<int> values = new List<int>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            string currStr = array[i];
                            int val = 0;

                            int.TryParse(currStr, out val);
                            values.Add(val);
                        }

                        queue[step].value = values.ToArray();
                    }
                    break;
                case ArguementType.FLOAT:
                    rtn = array.Length > 0;
                    if (rtn)
                    {
                        List<float> values = new List<float>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            string currStr = array[i];
                            float val = 0;

                            float.TryParse(currStr, out val);
                            values.Add(val);
                        }

                        queue[step].value = values.ToArray();
                    }
                    break;
                case ArguementType.STRING:
                    rtn = false;
                    break;
                case ArguementType.USER:
                    rtn = e.MentionedUsers.Count > 0;
                    if (rtn)
                        queue[step].value = e.MentionedUsers.ToArray();
                    break;
                case ArguementType.MEMBER:
                    break;
                case ArguementType.CHANNEL:
                    rtn = e.MentionedChannels.Count > 0;
                    if (rtn)
                        queue[step].value = e.MentionedChannels.Select(x => x.Id).ToArray();
                    break;
                case ArguementType.ROLE:
                    rtn = e.MentionedRoles.Count > 0;
                    if (rtn)
                        queue[step].value = e.MentionedRoles.ToArray();
                    break;
                default:
                    rtn = false;
                    break;
            }

            return rtn;
        }
        
        public bool CheckNonArray(ArguementType expected, MessageCreateEventArgs e)
        {
            bool rtn = false;
            string str = e.Message.Content;
            
            switch (queue[step].type)
            {
                case ArguementType.INT:
                    int iVal = 0;
                    rtn = int.TryParse(str, out iVal);
                    queue[step].value = iVal;
                    break;
                case ArguementType.FLOAT:
                    float fVal = 0;
                    rtn = float.TryParse(str, out fVal);
                    queue[step].value = fVal;
                    break;
                case ArguementType.STRING:
                    queue[step].value = str;
                    rtn = true;
                    break;
                case ArguementType.USER:
                    rtn = e.MentionedUsers.Count > 0;
                    if (rtn)
                        queue[step].value = e.MentionedUsers[0].Id;
                    break;

                case ArguementType.CHANNEL:
                    rtn = e.MentionedChannels.Count > 0;
                    if (rtn)
                        queue[step].value = e.MentionedChannels[0].Id;
                    break;
                case ArguementType.ROLE:
                    rtn = e.MentionedRoles.Count > 0;
                    if (rtn)
                        queue[step].value = e.MentionedRoles[0].Id;
                    break;
                case ArguementType.BOOLEAN:
                    string input = str.ToLower();
                    if (input == "yes" || input == "true" || input == "1")
                    {
                        queue[step].value = true;
                        rtn = true;
                    }
                    else if (input == "no" || input == "false" || input == "0")
                    {
                        queue[step].value = false;
                        rtn = true;
                    }
                    else { rtn = false; }
                    break;

                default:
                    rtn = false;
                    break;
            }

            return rtn;
        }
    }

    public interface IMessageCallback
    {
        void Callback(MessageSettingRequest request);
    }
}
