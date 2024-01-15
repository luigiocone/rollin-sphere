using UnityEngine;

public class StatManager 
{
    public readonly Stat Stat;
    public readonly StatsCollectionManager Stats;

    public StatSettings Settings => Stat.Settings;
    public StatId StatId => Stat.Id;

    public StatManager(StatsCollectionManager stats, Stat stat)
    {
        Stats = stats;
        Stat = stat;
    }

    public void CheckDependencies()
    {
        Stat upperBound = null;
        if (Settings.HasUpperBound)
        {
            Stats.TryAddStat(Settings.UpperBoundId, Stat.BaseValue);
            upperBound = Stats.GetStat(Settings.UpperBoundId);
        }

        if (upperBound != null)
        { 
            Stat.GetMaxValue = upperBound.GetCurrValue;
            Stat.OnValueChange += (StatValueChangeEvent evt) => Stat.RaiseDirtyFlag();
	    }
        else 
	    { 
            Stat.GetMaxValue = () => Settings.Max;
	    }

        // TODO: Lower bound implementation currently not needed
        Stat.GetMinValue = () => Settings.Min;
    }
}
