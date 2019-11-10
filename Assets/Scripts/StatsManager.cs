using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    protected Stats stats = new Stats();

    void Awake()
    {
        if(StatsManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        Instance = this;
    }

    public void SaveStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "enemies_killed", Value = stats.enemiesKilled },
                new StatisticUpdate { StatisticName = "games", Value = stats.games },
                new StatisticUpdate { StatisticName = "levels", Value = stats.levels }
                //TODO: money statistic
                //new StatisticUpdate { StatisticName = "money", Value = stats.money },
            }
        },
            result => { Debug.Log("User statistics updated"); },
            error => { Debug.LogError(error.GenerateErrorReport());
        });
    }

    public void SetStatistics(List<StatisticValue> values)
    {
        foreach (var stat in values)
        {
            switch(stat.StatisticName) {
                case "enemies_killed":
                    stats.enemiesKilled = stat.Value;
                    break;
                case "games":
                    stats.games = stat.Value;
                    break;
                case "levels":
                    stats.levels = stat.Value;
                    break;
                default:
                    break;
            }
        }
    }


    public void Death()
    {
        stats.games += 1;
        SaveStats();
    }
    public void LvlUP()
    {
        stats.levels += 1;
    }

    public void Shoot()
    {
        stats.shotsFired += 1;
    }

    public void Dash()
    {
        stats.usedDashes += 1;
    }
    public void ReceiveDamage(int value)
    {
        stats.receivedDamage += value;
    }

    public void CauseDamage(int value)
    {
        stats.causedDamage += value;
    }

    public void KillEnemy()
    {
        stats.enemiesKilled += 1;
    }

    public Stats GetStats()
    {
        return stats;
    }
}
