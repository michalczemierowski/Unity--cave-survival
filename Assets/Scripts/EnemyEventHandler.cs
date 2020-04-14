using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyEventHandler : MonoBehaviour
{
    [Header("Settings")][Space]
    [SerializeField][Tooltip("Maximum health ")] private int maxHp = 100;
    [Tooltip("Particles color ")] public Color mainColor;
    [Tooltip("Can enemy damage player on touch")] public bool canDamageMeele = true;
    [SerializeField][Tooltip("Bullet velocity multipler")] private float bulletSpeed = 5;
    [SerializeField][Tooltip("Bullet/Dash damage")] private int damage = 5;
    [SerializeField][Tooltip("Movement speed multipler")] private float movementSpeed = 3;
    [SerializeField][Range(0,3)][Tooltip("Airborne speed multiplier")] private float inAirSpeedMultipler = 2;
    [SerializeField][Range(0, 1)][Tooltip("Chance to dash instead of a shot")] private float dashChance = 0.25f;
    [SerializeField][Tooltip("Time multiplier between shots")] private float timeMultipler = 3f;
    [SerializeField][Tooltip("Attack regardless of distance")] private bool triggerOnInstantiate = false;
    [SerializeField][Tooltip("The distance in which enemies will begin to attack")] private float triggerDistance = 16;

    [Space][Header("Game Objects")][Space]
    [SerializeField][Tooltip("Health indicator sprite")] private GameObject hpBar;
    [SerializeField][Tooltip("Bullet GameObject")] private GameObject bulletPrefab;
    [SerializeField][Tooltip("The object on whose position the bullets will be created")] private Transform bulletSpawnPos;
    [SerializeField][Tooltip("The object around which the weapon will rotate")] private GameObject weaponPivot;

    [Header("Events")]
    [Space]
    public UnityEvent OnShootEvent;
    public UnityEvent OnDashEvent;

    private EffectHandler effectHandler;
    private PlayerController controller;
    private StatsManager statsManager;
    private EnemySpawner spawner;

    private Vector3 hpBarScale;
    private GameObject player;
    private Rigidbody2D mRigidbody2D;
    private Vector3 feetPos;

    private LayerMask moveLayerMask;

    private bool isTriggered;

    protected float timeToShoot;
    protected int currentHp;

    public void setTimeMultipler(float value)
    {
        timeMultipler = value;
    }

    private void Start()
    {
        if (OnShootEvent == null)
            OnShootEvent = new UnityEvent();
        if (OnDashEvent == null)
            OnDashEvent = new UnityEvent();

        moveLayerMask = LayerMask.GetMask("Player", "Scene");

        hpBar.GetComponent<SpriteRenderer>().color = mainColor;
        currentHp = maxHp;
        hpBarScale = hpBar.transform.localScale;
        player = EffectHandler.Instance.player;
        mRigidbody2D = GetComponent<Rigidbody2D>();
        effectHandler = EffectHandler.Instance;
        controller = PlayerController.Instance;
        spawner = EnemySpawner.Instance;
        statsManager = StatsManager.Instance;

        if (triggerOnInstantiate)
        {
            isTriggered = true;
            timeToShoot = Random.value * timeMultipler;
        }
        feetPos = new Vector3(0, transform.localScale.y * 0.5f, 0);
    }

    private void Update()
    {
        if (!isTriggered)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < triggerDistance)
            {
                isTriggered = true;
                timeToShoot = Random.value * timeMultipler;
            }
        }
        else
        {
            Aim();
            if (timeToShoot <= 0)
            {
                if (Random.value > dashChance)
                    Shoot();
                else Dash();
            }
            else
                timeToShoot -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (!isTriggered)
            return;

        Vector2 difference = player.transform.position - transform.position;
        float step = movementSpeed * Time.deltaTime;
        if (mRigidbody2D.velocity.y < 0)
            step *= inAirSpeedMultipler;

        if (difference.x > 0)
        {
            RaycastHit2D r = Physics2D.Raycast(transform.position - feetPos, Vector2.right, transform.localScale.x, moveLayerMask);
            if (r.collider == null)
            {
                transform.position = new Vector2(transform.position.x + step, transform.position.y);
            }
            else if (r.collider.tag == "Scene")
            {
                mRigidbody2D.AddForce(Vector2.up, ForceMode2D.Impulse);
            }
        }
        else
        {
            RaycastHit2D l = Physics2D.Raycast(transform.position - feetPos, Vector2.left, transform.localScale.x, moveLayerMask);
            if (l.collider == null)
            {
                transform.position = new Vector2(transform.position.x - step, transform.position.y);
            }
            else if (l.collider.tag == "Scene")
            {
                mRigidbody2D.AddForce(Vector2.up, ForceMode2D.Impulse);
            }
        }

    }

    public void Damage(int dmg)
    {
        currentHp -= dmg;
        if (currentHp <= 0)
            Death();

        hpBar.transform.localScale = new Vector3(hpBarScale.x * ((float)currentHp / (float)maxHp), hpBarScale.y, hpBarScale.z);

        //STATS
        statsManager?.CauseDamage(dmg);
    }

    public int GetDamage()
    {
        return damage;
    }

    public void Kill()
    {
        Death();
    }

    private void Death()
    {
        spawner.KilledEnemy(this);
        Destroy(gameObject);
        controller.AddPoints(10);
        controller.AddCoins(Random.Range(2, 5));
        effectHandler.InstantiateParticle(transform.position, ParticleType.EnemyDeath, destroyTime:5);
        effectHandler.PlaySound(transform.position, SoundType.EnemyDeath);

        //STATS
        statsManager?.KillEnemy();
    }

    private void Shoot()
    {
        RaycastHit2D shoot;
        shoot = Physics2D.Raycast(transform.position, weaponPivot.transform.right, 10);
        if (shoot.collider != null && shoot.collider.tag == "Player")
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<BulletEventHandler>().damage = damage;
            bullet.transform.rotation = weaponPivot.transform.rotation;
            bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
            effectHandler.PlaySound(transform.position, SoundType.Shoot);
            OnShootEvent.Invoke();
        }

        timeToShoot = Random.value * timeMultipler;
    }

    private void Dash()
    {
        RaycastHit2D shoot;
        shoot = Physics2D.Raycast(transform.position, weaponPivot.transform.right, 10);
        if (shoot.collider != null && shoot.collider.tag == "Player")
        {
            mRigidbody2D.AddForce(weaponPivot.transform.right * 15, ForceMode2D.Impulse);
            OnDashEvent.Invoke();
        }
        timeToShoot = Random.value * timeMultipler;
    }

    private void Aim()
    {
        Vector3 lookPos = player.transform.position - transform.position;
        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;
        weaponPivot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (player.transform.position.x > transform.position.x)
            weaponPivot.transform.localScale = Vector3.one;
        else
            weaponPivot.transform.localScale = new Vector3(1, -1, 1);
    }
}
