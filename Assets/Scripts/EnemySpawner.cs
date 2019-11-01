using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [SerializeField][Tooltip("Max enemies on map")] private int targetEnemiesOnMap = 10;
    [SerializeField][Tooltip("Enemy GameObject")] private GameObject enemyGameObject;

    private List<EnemyEventHandler> enemiesOnMap = new List<EnemyEventHandler>();
    private float timeMultipler;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.visible = true;
        InvokeRepeating("InstantiateEnemy", 1, 1);
    }

    private void InstantiateEnemy()
    {
        if (enemiesOnMap.Count < targetEnemiesOnMap)
        {
            int x = Random.Range(-65, 40);
            int y = 20;
            EnemyEventHandler enemy = Instantiate(enemyGameObject, new Vector3(x, y, 2), Quaternion.identity).GetComponent<EnemyEventHandler>();
            enemy.setTimeMultipler(timeMultipler);
            enemiesOnMap.Add(enemy);
        }
    }

    public EnemyEventHandler[] GetEnemies()
    {
        return enemiesOnMap.ToArray();
    }

    public void KilledEnemy(EnemyEventHandler enemy)
    {
        enemiesOnMap.Remove(enemy);
    }

    public void SetTargetEnemies(int value)
    {
        targetEnemiesOnMap = value;
    }
    public void setTimeMultipler(float value)
    {
        timeMultipler = value;
    }
}