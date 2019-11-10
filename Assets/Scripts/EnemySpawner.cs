using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    [SerializeField] private GameObject player;
    [Space]
    [SerializeField] [Tooltip("Objects with this tag will be used as spawnpoints")] private string spawnPointsTag = "SpawnPoint";
    [SerializeField][Tooltip("Maximum distance from the player in which they will be spawned, 0 for no limit")] float maxDistanceToPlayer = 50f;
    [SerializeField][Tooltip("Enemy GameObject")] private GameObject[] enemyGameObjects;
    [SerializeField][Tooltip("Maximum number of enemies on the map per level, 0 or empty field means no change")] private int[] targetEnemiesOnMap = { 5, 7, 10, 13, 15 };

    private List<EnemyEventHandler> enemiesOnMap = new List<EnemyEventHandler>();
    private Vector3[] spawnPoints;

    private int currentTargetEnemiesOnMap;
    private float timeMultipler;

    private int distanceCheckCount = 5;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.transform.position, maxDistanceToPlayer);
    }

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

        GameObject[] temp = GameObject.FindGameObjectsWithTag(spawnPointsTag);
        spawnPoints = new Vector3[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            spawnPoints[i] = temp[i].transform.position;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn positions were found. Disabling EnemySpawner");
            this.enabled = false;
        }
        else
            Debug.LogFormat("Found {0} spawnpoints", spawnPoints.Length);
    }

    private void InstantiateEnemy()
    {
        if (enemiesOnMap.Count < currentTargetEnemiesOnMap)
        {
            GameObject randomEnemy = enemyGameObjects[Random.Range(0, enemyGameObjects.Length)];
            Vector3 spawnPos;

            if (maxDistanceToPlayer > 0)
            {
                List<Vector3> inRadius = new List<Vector3>();
                foreach (var position in spawnPoints)
                {
                    float distance = Vector2.Distance(position, player.transform.position);
                    if (distance <= maxDistanceToPlayer)
                        inRadius.Add(position);
                }
                spawnPos = inRadius[Random.Range(0, inRadius.Count)];
            }
            else
            {
                spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

            EnemyEventHandler enemy = Instantiate(randomEnemy, spawnPos, Quaternion.identity).GetComponent<EnemyEventHandler>();
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