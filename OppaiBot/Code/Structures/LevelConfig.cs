using Interfaces;
using MessageReqDefinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class LevelConfig : IConfigurable, IMessageCallback
{
    public int min_msg_exp { get; set; }
    public int max_msg_exp { get; set; }

    public int min_vc_exp { get; set; }
    public int max_vc_exp { get; set; }

    public int maxLevel { get; set; }

    public float a { get; set; }
    public float b { get; set; }

    public int visableAmountLeaderboard { get; set; }

    public ulong[] levelUpRoles { get; set; }
    public int[] levelRoleRequirment { get; set; }

    public void Callback(MessageSettingRequest request)
    {
        MessageRequirement[] msg = request.queue.ToArray();
        
            try
            {
                min_msg_exp = (int)msg[0].value;
                max_msg_exp = (int)msg[1].value;
                min_vc_exp = (int)msg[2].value;
                max_vc_exp = (int)msg[3].value;
                maxLevel = (int)msg[4].value;
                a = (float)msg[5].value;
                b = (float)msg[6].value;
                visableAmountLeaderboard = (int)msg[7].value;
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);
            }
    }

    public string GetDefualtJsonString()
    {
        LevelConfig defualt = new LevelConfig()
        {
            min_msg_exp = 10,
            max_msg_exp = 50,

            min_vc_exp = 1,
            max_vc_exp = 5,

            maxLevel = 50,

            a = 300,
            b = 50,

            visableAmountLeaderboard = 4,
        };

        return JsonConvert.SerializeObject(defualt);
    }

    public int GetLevel(float exp)
    {
        float mathStuff = exp;
        mathStuff -= b;
        mathStuff /= a;

        return (int)Math.Floor(mathStuff);
    }
    public float GetExpForLevel(int level)
    {
        return (level * a) + b;
    }
}
