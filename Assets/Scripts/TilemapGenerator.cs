using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapGenerator : MonoBehaviour
{

    //JUST FOR TEST. NOT USED
    public GameObject Grid;
    public Tilemap tilemapPrefab;
    public Tile whiteTile, grayTile;
    public int width = 100;
    int offset;

    private GameObject lastTilemap;
    private bool[] tilemaps = new bool[256];
    private int halfSize;
    private GameObject[] tilemapGameObjects;
    
    void Start()
    {
        offset = Random.Range(0, 100000);
        halfSize = tilemaps.Length / 2;
        tilemapGameObjects = new GameObject[tilemaps.Length];
        GenerateMap();
    }

    [ContextMenu("Generate")]
    public void GenerateMap()
    {
        int X = ceilToWidth(Mathf.CeilToInt(transform.position.x));
        if (!tilemaps[X / width + halfSize])
        {
            tilemaps[X / width + halfSize] = true;
            Tilemap tilemap = Instantiate(tilemapPrefab, new Vector3(X, 0, 0), Quaternion.identity, Grid.transform);
            lastTilemap = tilemap.gameObject;
            tilemapGameObjects[X / width + halfSize] = lastTilemap;
            tilemap.ClearAllTiles();

            for (int x = -width/2; x < width/2; x++)
            {
                int y = Mathf.CeilToInt(Mathf.PerlinNoise((float)(x + X) / 100, offset) * 100);
                if (x + X >= tilemaps.Length * width - width) y = 100;
                tilemap.SetTile(new Vector3Int(x, y, 5), whiteTile);
                for (int Y = y - 1; Y > 0; Y--)
                {
                    tilemap.SetTile(new Vector3Int(x, Y, 5), grayTile);
                }
            }
        }
        else if(!tilemapGameObjects[X / width + halfSize].activeSelf)
        {
            tilemapGameObjects[X / width + halfSize].SetActive(true);
        }
    }

    void Update()
    {
        if (lastTilemap == null)
            return;
        if(Mathf.Abs(transform.position.x - lastTilemap.transform.position.x) > -width/2)
        {
            print((float)width / 4);
            GenerateMap();
        }
        for (int i = 0; i < tilemapGameObjects.Length; i++)
        {
            GameObject gobject = tilemapGameObjects[i];
            if (gobject != null && Mathf.Abs(gobject.transform.position.x - transform.position.x) > width * 2)
                gobject.SetActive(false);
        }
    }

    private int ceilToWidth(int value)
    {
        return ((value + width) / width) * width;
    }
}
