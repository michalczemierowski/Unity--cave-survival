using System.Collections;
using UnityEngine;

public class EffectHandler : MonoBehaviour
{
    public static EffectHandler Instance;

    public AudioSource[] SFX;
    public GameObject[] VFX;

    [Space]
    public GameObject Coin;
    public GameObject player;

    void Awake()
    {
        Instance = this;
    }

    public void PlaySound(Vector3 position, SoundType type)
    {
        int intType = (int)type;
        SFX[intType].gameObject.transform.position = position;
        SFX[intType].Play();
    }

    public void InstantiateParticleWithRotation(Vector3 position, ParticleType type, float destroyTime = 5f, Color color = new Color(), bool moveRight = true)
    {
        int intType = (int)type;
        GameObject particle = Instantiate(VFX[intType], position, Quaternion.identity);

        Vector3 lookPos = player.transform.position;
        lookPos = lookPos - position;
        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;

        particle.transform.rotation = Quaternion.AngleAxis(angle - 45, Vector3.forward);
        if(moveRight)
            particle.transform.position += particle.transform.right;
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (color != null)
        {
            var main = ps.main;
            main.startColor = color;
        }
        particle.GetComponent<ParticleSystem>().Play();

        Destroy(particle, destroyTime);
    }

    public void InstantiateParticle(Vector3 position, ParticleType type, Transform parent = null, float destroyTime = 1f)
    {
        int intType = (int)type;
        GameObject particle = parent == null ? Instantiate(VFX[intType], position, Quaternion.identity) : Instantiate(VFX[intType], position, Quaternion.identity, parent);

        particle.transform.position = particle.transform.position;
        particle.GetComponent<ParticleSystem>().Play();

        Destroy(particle, destroyTime);
    }

    public void InstantiateCoin(Vector3 position)
    {
        Instantiate(Coin, position, Quaternion.identity);
    }
}

public enum SoundType
{
    Hit = 0,
    Hurt = 1,
    Jump = 2,
    Shoot = 3,
    Coin = 4,
    Stun = 5,
    LvlUp = 6,
    EnemyDeath = 7
}

public enum ParticleType
{
    PlayerBlood = 0,
    EnemyBlood = 1,
    WallPlayer = 2,
    WallEnemy = 3,
    EnemyDeath = 4,
    Coin = 5,
    LvlUp = 6,
    Stun = 7,
    PlayerDeath = 8
}
