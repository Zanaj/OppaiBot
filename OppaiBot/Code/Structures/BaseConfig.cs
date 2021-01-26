using DSharpPlus.Entities;
using Interfaces;
using MessageReqDefinations;
using Newtonsoft.Json;
using OppaiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BaseConfig : IConfigurable, IMessageCallback
{
    public string token { get; set; }
    public string prefix { get; set; }
    public string levelMsg { get; set; }
    public ulong levelUpChannel { get; set; }
    public ulong[] activityChannel { get; set; }
    
    public float serverColorR { get; set; }
    public float serverColorG { get; set; }
    public float serverColorB { get; set; }

    public DiscordColor serverColor { get { return new DiscordColor(serverColorR, serverColorG, serverColorB); } }
    public int serverTickRate { get; set; }

    public void Callback(MessageSettingRequest request)
    {
        MessageRequirement[] msg = request.queue.ToArray();
        if(msg.Length >= 3)
        {
            try
            {
                activityChannel = (ulong[])msg[0].value;

                serverColorR = (float)msg[1].value;
                serverColorG = (float)msg[2].value;
                serverColorB = (float)msg[3].value;

                if (serverColorR > 1)
                    serverColorR /= 255;

                if (serverColorG > 1)
                    serverColorG /= 255;

                if (serverColorB > 1)
                    serverColorB /= 255;


                serverTickRate = (int)msg[4].value;

                //Bot.serverTickTimer.Stop();
                //Bot.serverTickTimer.Interval = serverTickRate;
                //Bot.serverTickTimer.Start();

                ConfigHandler.Save();
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        else
        {
            Console.WriteLine("Wrong amount of arguements got " + request.queue.Count);
        }
    }

    public string GetDefualtJsonString()
    {
        BaseConfig defualt = new BaseConfig()
        {
            token = "",
            prefix = "oq",
            levelMsg = "Congrats {user} you leveled to {level}!",
            levelUpChannel = 0,
            activityChannel = new ulong[] { 0},

            serverColorR = 0,
            serverColorG = 0,
            serverColorB = 1,
            serverTickRate = 250,

};

        return JsonConvert.SerializeObject(defualt);
    }
}
