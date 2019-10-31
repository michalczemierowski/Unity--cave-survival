using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapEventHandler : MonoBehaviour
{
    private TilemapManager tileManager;
    Tilemap tilemap;

    private GameObject player;
    private Dictionary<Vector3Int, float> placedTiles = new Dictionary<Vector3Int, float>();

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tileManager = TilemapManager.Instance;
        player = EffectHandler.Instance.player;
    }

    private Vector3Int CalculatePosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x + 0.5f);
        int y = Mathf.FloorToInt(position.y + 0.5f);
        int X = Mathf.FloorToInt(player.transform.position.x);
        int Y = Mathf.FloorToInt(player.transform.position.y);

        return new Vector3Int(x + X, y + Y, 0);
    }

    public bool SetTile(Vector3 position, TileType type)
    {
        TileBase tile = tileManager.GetTile(type);
        Vector3Int tilePosition = CalculatePosition(position);

        if (tilemap.GetTile(tilePosition) != null)
            return false;
        if (placedTiles.ContainsKey(tilePosition))
            return false;
        placedTiles.Add(tilePosition, 1);
        tilemap.SetTile(tilePosition, tile);
        return true;
    }

    public bool RemoveTile(Vector3 position)
    {
        Vector3Int tilePosition = CalculatePosition(position);
        if (placedTiles.ContainsKey(tilePosition))
        {
            tilemap.SetTile(tilePosition, null);
            placedTiles.Remove(tilePosition);
            return true;
        }
        return false;
    }

    public bool DamageTile(Vector3 position, float damage)
    {
        Vector3Int tilePosition = new Vector3Int(Mathf.FloorToInt(position.x + 0.5f), Mathf.FloorToInt(position.y + 0.5f), 0);

        float hp;
        if(placedTiles.TryGetValue(tilePosition, out hp))
        {
            hp -= damage;
            if (hp <= 0)
            {
                tilemap.SetTile(tilePosition, null);
                placedTiles.Remove(tilePosition);
                return true;
            }
            else
                placedTiles[tilePosition] = hp;
        }
        return false;
    }
}
