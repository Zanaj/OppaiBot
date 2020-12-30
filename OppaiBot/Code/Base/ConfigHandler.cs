using Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ConfigHandler
{
    public static string BASE_PATH
    {
        get
        {
            #if DEBUG
                return Path.Combine(Environment.CurrentDirectory, CONFIG_FOLDER_NAME);
            #else
                return Environment.CurrentDirectory + "\\..\\" + CONFIG_FOLDER_NAME;
            #endif
        }
    }

    public const string CONFIG_FOLDER_NAME = "Configs";

    public static BaseConfig baseConfig;
    public const string BASE_CONFIG_NAME = "baseConfig.json";

    public static EconomyConfig economyConfig;
    public const string ECONOMY_CONFIG_NAME = "economyConfig.json";

    public static LevelConfig levelConfig;
    public const string LEVEL_CONFIG_NAME = "levelConfig.json";

    public static FileStream BaseConfigStream;
    public static FileStream EconomyConfigStream;
    public static FileStream LevelConfigStream;
    public static FileStream UserDatabaseStream;

    public static void InitializeConfigs()
    {
        string path = BASE_PATH;
        Console.WriteLine("PATH: "+path);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string baseStr = GetJsonSettingsAsync(path, BASE_CONFIG_NAME, new BaseConfig());
        string economyStr = GetJsonSettingsAsync(path, ECONOMY_CONFIG_NAME, new EconomyConfig());
        string levelStr = GetJsonSettingsAsync(path, LEVEL_CONFIG_NAME, new LevelConfig());

        baseConfig = JsonConvert.DeserializeObject<BaseConfig>(baseStr);
        economyConfig = JsonConvert.DeserializeObject<EconomyConfig>(economyStr);
        levelConfig = JsonConvert.DeserializeObject<LevelConfig>(levelStr);

        BaseConfigStream = File.OpenWrite(Path.Combine(path, BASE_CONFIG_NAME));
        EconomyConfigStream = File.OpenWrite(Path.Combine(path, ECONOMY_CONFIG_NAME));
        LevelConfigStream = File.OpenWrite(Path.Combine(path, LEVEL_CONFIG_NAME));
        UserDatabaseStream = File.OpenWrite(path + "\\..\\" + User.SAVE_FOLDER + "\\" + User.USER_FILE_NAME);
    }

    private static string GetJsonSettingsAsync(string basePath, string configName, IConfigurable config)
    {
        string path = Path.Combine(basePath, configName);

        if (!File.Exists(path))
        {
            var stream = File.Create(path);
            using (stream)
            using (StreamWriter sw = new StreamWriter(stream, new UTF8Encoding(false)))
                sw.Write(config.GetDefualtJsonString());
        }

        string json = "";
        using (FileStream fs = File.OpenRead(path))
        using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
            json = sr.ReadToEnd();

        return json;
    }

    public static void Save()
    {
        try
        {
            string path = Path.Combine(Environment.CurrentDirectory, CONFIG_FOLDER_NAME);
            DoSaving(path, BASE_CONFIG_NAME, baseConfig, BaseConfigStream);
            DoSaving(path, ECONOMY_CONFIG_NAME, economyConfig, EconomyConfigStream);
            DoSaving(path, LEVEL_CONFIG_NAME, levelConfig, LevelConfigStream);

            Console.WriteLine("Saved!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
    }

    private static void DoSaving(string basePath, string fileName, object type, FileStream fs)
    {
        string path = Path.Combine(basePath, fileName);
        string json = JsonConvert.SerializeObject(type);

        using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false)))
            sw.Write(json);

    }
}