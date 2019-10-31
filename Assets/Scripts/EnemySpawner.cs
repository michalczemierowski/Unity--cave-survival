using UnityEngine;
using UnityEditor;

public class EnemySpawner : MonoBehaviour
{
    public int enemiesOnMap = 10;
    public GameObject enemyGameObject;

    protected int enemyCount = 0;

    private void Start()
    {
        InvokeRepeating("Check", 1, 1);
    }

    public void Check()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (enemyCount < enemiesOnMap)
        {
            int x = Random.Range(-65, 40);
            int y = 20;
            Instantiate(enemyGameObject, new Vector3(x, y, 2), Quaternion.identity);
            enemyCount++;
        }
    }
}