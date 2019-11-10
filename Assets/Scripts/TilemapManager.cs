using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager Instance;
    [SerializeField]
    private TilemapEventHandler[] Tilemaps;
    [SerializeField]
    private TileBase[] tiles;

    private void Awake()
    {
        Instance = this;
    }

    public TilemapEventHandler GetTilemap(int index)
    {
        return Tilemaps[index];
    }

    public TileBase GetTile(TileType type)
    {
        return tiles[(int)type];
    }
    
}

public enum TileType
{
    Gray = 0,
    DarkGray = 1,
    White = 2
}
