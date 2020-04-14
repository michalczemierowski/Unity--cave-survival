using UnityEngine;

[CreateAssetMenu(menuName = "Weapon", fileName = "Weapon_0")]
public class Weapon : ScriptableObject
{
    [Tooltip("Bullet velocity multipler")] public float bulletSpeed = 5;
    [Tooltip("Bullet damage")] public int damage = 5;
    public float radius = 5;
    [Tooltip("Time between shots")] public float timeBetweenShoots = 0.3f;
    public GameObject bulletPrefab;
    public WeaponType weaponType;
}
public enum WeaponType
{
    PROJECTILE,
    EXPLOSIVE,
    RAYCAST
}
