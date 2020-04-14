using UnityEngine;

public class BulletEventHandler : MonoBehaviour
{
    public BulletType type;
    public int damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (type == BulletType.Player)
        {
            if (collision.tag != "Player")
            {
                if (collision.tag == "Enemy")
                {
                    EnemyEventHandler enemy = collision.GetComponentInParent<EnemyEventHandler>();
                    EffectHandler.Instance.PlaySound(transform.position, SoundType.Hurt);
                    EffectHandler.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.EnemyBlood, color:enemy.mainColor);
                    enemy.Damage(damage);
                    Destroy(gameObject);
                }
                else if (collision.tag == "Scene")
                {
                    EffectHandler.Instance.PlaySound(transform.position, SoundType.Hit);
                    EffectHandler.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.WallPlayer);
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
                    EffectHandler.Instance.PlaySound(transform.position, SoundType.Hurt);
                    EffectHandler.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.PlayerBlood);
                    collision.GetComponent<PlayerController>().Damage(damage);
                    Destroy(gameObject);
                }
                else if (collision.tag == "Scene")
                {
                    EffectHandler.Instance.PlaySound(transform.position, SoundType.Hit);
                    EffectHandler.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.WallEnemy);
                    //TilemapManager.Instance.GetTilemap(0).DamageTile(Vector3Int.CeilToInt(transform.position) - Vector3.one, 0.35f);
                    Destroy(gameObject);
                }
            }
        }
    }
}

public enum BulletType
{
    Player = 0,
    Enemy = 1
};
