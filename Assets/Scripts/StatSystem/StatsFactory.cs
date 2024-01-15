public class StatsFactory 
{
    public StatManager GetStatManager(Stat stat, StatsCollectionManager stats)
    {
        return stat.Id switch
        {
            StatId.HEALTH => new HealthManager(stats, stat),
            StatId.STAMINA => new StaminaManager(stats, stat),
            _ => new StatManager(stats, stat),
        };
    }

    public Stat GetStatInstance(StatId id, float baseValue, StatsCollectionManager stats)
    {
        var settings = GlobalDataManager.GetStatSettings(id);

        return settings.IsPersistent 
	        ? new PersistentStat(id, baseValue, settings)
            : new Stat(id, baseValue, settings);
    }

    public Stat GetStatInstance(Stat stat, StatsCollectionManager stats) =>
        GetStatInstance(stat.Id, stat.BaseValue, stats);
}

