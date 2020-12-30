using Interfaces;
using MessageReqDefinations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EconomyConfig : IConfigurable, IMessageCallback
{
    public string currency { get; set; }
    public int dailyCooldown { get; set; }
    public int minDailyReward { get; set; }
    public int maxDailyReward { get; set; }

    public int workCooldown { get; set; }
    public int minWorkReward { get; set; }
    public int maxWorkReward { get; set; }

    public int minDropReward { get; set; }
    public int maxDropReward { get; set; }

    public int appearCooldown { get; set; }

    public float pointsToExpRate { get; set; }

    public void Callback(MessageSettingRequest request)
    {
        MessageRequirement[] msg = request.queue.ToArray();

        try
        {
            if ((string)request.extraInfo == "work")
            {
                workCooldown = (int)msg[0].value;
                minWorkReward = (int)msg[1].value;
                maxWorkReward = (int)msg[2].value;
            }
            else if ((string)request.extraInfo == "daily")
            {
                dailyCooldown = (int)msg[0].value;
                minDailyReward = (int)msg[1].value;
                maxDailyReward = (int)msg[2].value;
            }
            else if ((string)request.extraInfo == "drop")
            {
                appearCooldown = (int)msg[0].value;
                minDropReward = (int)msg[1].value;
                maxDropReward = (int)msg[2].value;
            }
            else if ((string)request.extraInfo == "other")
            {
                currency = (string)msg[0].value;
                pointsToExpRate = (float)msg[1].value;
            }

            ConfigHandler.Save();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public string GetDefualtJsonString()
    {
        EconomyConfig defualt = new EconomyConfig()
        {
            currency = "Monies",
            dailyCooldown = 24 * 60 * 60,
            workCooldown = 10*60,
            appearCooldown = 5*60,
            minDailyReward = 100,
            maxDailyReward = 300,
            minWorkReward = 10,
            maxWorkReward = 30,
            minDropReward = 5,
            maxDropReward = 30,
            pointsToExpRate = 10,
        };

        return JsonConvert.SerializeObject(defualt);
    }

}