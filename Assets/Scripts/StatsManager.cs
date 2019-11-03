using UnityEngine;
using System.Collections;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    protected Stats stats = new Stats();

    void Awake()
    {
        Instance = this;
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
