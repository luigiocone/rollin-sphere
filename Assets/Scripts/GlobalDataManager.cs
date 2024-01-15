using UnityEngine;

public class GlobalDataManager 
{
    const string k_StatsSettingsPath = "Stats/StatsSettings";
    static SerializableStatSettings s_SettingsData;

    static void LoadIfNull()
    {
        if (s_SettingsData) 
	        return;

        s_SettingsData = Resources.Load<SerializableStatSettings>(k_StatsSettingsPath);
        if (s_SettingsData == null)
            Debug.LogError("Non existent path: " + k_StatsSettingsPath);
    }

    public static StatSettings GetStatSettings(StatId id)
    {
        LoadIfNull();
        var dict = s_SettingsData.dictionary;

        bool present = dict.TryGetValue(id, out StatSettings settings);
        if (present) 
	        return settings;
        return s_SettingsData.DefaultSettings;
    }
}
