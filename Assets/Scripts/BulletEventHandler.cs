using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEventHandler : MonoBehaviour
{
    public BulletType type;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (type == BulletType.Player)
        {
            if (collision.tag != "Player")
            {
                if (collision.tag == "Enemy")
                {
                    SFXObjects.Instance.PlaySound(transform.position, SoundType.Hurt);
                    SFXObjects.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.EnemyBlood);
                    var enemy = collision.GetComponent<EnemyEventHandler>() == null ? collision.GetComponentInParent<EnemyEventHandler>() : collision.GetComponent<EnemyEventHandler>();
                    enemy.Damage(10);
                    Destroy(gameObject);
                }
                else if (collision.tag == "Scene")
                {
                    SFXObjects.Instance.PlaySound(transform.position, SoundType.Hit);
                    SFXObjects.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.WallPlayer);
                    Destroy(gameObject);
                }
            }
        }
        else if(type == BulletType.Enemy)
        {
            if (collision.tag != "Enemy")
            {
                if (collision.tag == "Player")
                {
                    SFXObjects.Instance.PlaySound(transform.position, SoundType.Hurt);
                    SFXObjects.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.PlayerBlood);
                    collision.GetComponent<PlayerController>().Damage(5);
                    Debug.Log("dmg player");
                }
                else if (collision.tag == "Scene")
                {
                    SFXObjects.Instance.PlaySound(transform.position, SoundType.Hit);
                    SFXObjects.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.WallEnemy);
                    TilemapManager.Instance.GetTilemap(0).DamageTile(Vector3Int.CeilToInt(transform.position) - Vector3.one, 0.35f);
                }
                Destroy(gameObject);
            }
        }
    }
}

public enum BulletType
{
    Player = 0,
    Enemy = 1
};
