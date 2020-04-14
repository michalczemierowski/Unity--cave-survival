using UnityEngine;

//https://github.com/michalczemierowski
public class ExplosiveBullet : MonoBehaviour
{
    public int damage;
    public float radius;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" || collision.tag == "Scene")
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.tag == "Enemy" && collider.name != "weapon")
                {
                    EnemyEventHandler enemy = collider.GetComponentInParent<EnemyEventHandler>();
                    EffectHandler.Instance.PlaySound(transform.position, SoundType.Hurt);
                    EffectHandler.Instance.InstantiateParticleWithRotation(transform.position, ParticleType.EnemyBlood, color: enemy.mainColor);
                    enemy.Damage(damage);
                }
            }
            Destroy(gameObject);
        }
    }
}
