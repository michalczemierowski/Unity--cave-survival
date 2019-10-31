using System.Collections;
using UnityEngine;

public class SFXObjects : MonoBehaviour
{
    public static SFXObjects Instance;

    public AudioSource[] SFX;
    public GameObject[] VFX;

    [Space]
    public GameObject Coin;

    [SerializeField]
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

    public void InstantiateParticleWithRotation(Vector3 position, ParticleType type)
    {
        int intType = (int)type;
        GameObject particle = Instantiate(VFX[intType], position, Quaternion.identity);

        Vector3 lookPos = player.transform.position;
        lookPos = lookPos - position;
        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;

        particle.transform.rotation = Quaternion.AngleAxis(angle - 45, Vector3.forward);
        particle.transform.position = particle.transform.position + particle.transform.right;
        particle.GetComponent<ParticleSystem>().Play();

        StartCoroutine(destroyParticleTimer(particle, 5));
    }

    public void InstantiateParticle(Vector3 position, ParticleType type, Transform parent = null)
    {
        int intType = (int)type;
        GameObject particle = parent == null ? Instantiate(VFX[intType], position, Quaternion.identity) : Instantiate(VFX[intType], position, Quaternion.identity, parent);

        particle.transform.position = particle.transform.position;
        particle.GetComponent<ParticleSystem>().Play();

        StartCoroutine(destroyParticleTimer(particle, 5));
    }

    public void InstantiateCoin(Vector3 position)
    {
        Instantiate(Coin, position, Quaternion.identity);
    }

    private IEnumerator destroyParticleTimer(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }
}

public enum SoundType
{
    Hit = 0,
    Hurt = 1,
    Jump = 2,
    Shoot = 3,
    Coin = 4
}

public enum ParticleType
{
    PlayerBlood = 0,
    EnemyBlood = 1,
    WallPlayer = 2,
    WallEnemy = 3,
    Death = 4,
    Coin = 5,
    LvlUp = 6
}
