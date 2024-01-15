public class HealthManager : StatManager
{
    public readonly PersistentStat Health;
    public readonly DamageManager DamageManager;
    public bool Invincible
    {
        get => Health.Paused;
        set => Health.Paused = value;
    }

    public HealthManager(StatsCollectionManager stats, Stat stat) : base(stats, stat)
    {
        Health = stat as PersistentStat;
        DamageManager = new(this);
    }
}

