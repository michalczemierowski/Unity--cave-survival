using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [SerializeField][Tooltip("Enemy GameObject")] private GameObject[] enemyGameObjects;
    [SerializeField] [Tooltip("Maximum number of enemies on the map per level, 0 or empty field means no change")] private int[] targetEnemiesOnMap = { 5, 7, 10, 13, 15 };

    private List<EnemyEventHandler> enemiesOnMap = new List<EnemyEventHandler>();
    private int currentTargetEnemiesOnMap;
    private float timeMultipler;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.visible = true;
        InvokeRepeating("InstantiateEnemy", 1, 1);
        if (targetEnemiesOnMap[0] <= 0)
        {
            Debug.LogError("First value of Target Enemies On Map should not be equal to 0\n" + gameObject.name);
            Debug.LogError("Setting default values");
            targetEnemiesOnMap[0] = 5;
        }
        else
        {
            currentTargetEnemiesOnMap = targetEnemiesOnMap[0];
        }
    }

    private void InstantiateEnemy()
    {
        if (enemiesOnMap.Count < currentTargetEnemiesOnMap)
        {
            int x = Random.Range(-65, 40);
            int y = 20;
            int random = Random.Range(0, enemyGameObjects.Length);
            EnemyEventHandler enemy = Instantiate(enemyGameObjects[random], new Vector3(x, y, 2), Quaternion.identity).GetComponent<EnemyEventHandler>();
            enemiesOnMap.Add(enemy);
        }
    }

    public void SetLevel(int value)
    {
        if (value < targetEnemiesOnMap.Length && targetEnemiesOnMap[value] != 0)
            SetTargetEnemies(targetEnemiesOnMap[value]);
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
        currentTargetEnemiesOnMap = value;
    }
}