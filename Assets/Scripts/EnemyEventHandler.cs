using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEventHandler : MonoBehaviour
{
    [SerializeField][Tooltip("Maximum health ")]
    int maxHp = 100;

    public GameObject hpBar;

    [SerializeField][Tooltip("Bullet velocity multipler")]
    float bulletSpeed = 5;

    [SerializeField][Tooltip("Movement speed multipler")]
    float movementSpeed = 3;

    [SerializeField][Range(0,3)][Tooltip("Airborne speed multiplier")]
    float inAirSpeedMultipler = 2;

    [SerializeField][Range(0, 1)][Tooltip("Chance to dash instead of a shot")]
    float dashChance = 0.25f;

    [SerializeField][Tooltip("Bullet GameObject")]
    GameObject bulletPrefab;

    [SerializeField][Tooltip("The object on whose position the bullets will be created")]
    Transform bulletSpawnPos;

    [SerializeField][Tooltip("The object around which the weapon will rotate")]
    GameObject weaponPivot;

    [SerializeField][Tooltip("The distance in which enemies will begin to attack")]
    float triggerDistance = 16;

    private SFXObjects sfx;
    private PlayerController controller;
    private Vector3 hpBarScale;
    private GameObject player;
    private Rigidbody2D mRigidbody2D;

    private bool isTriggered;

    protected float timeToShoot;
    protected int currentHp;

    private void Start()
    {
        currentHp = maxHp;
        hpBarScale = hpBar.transform.localScale;
        player = SFXObjects.Instance.player;
        mRigidbody2D = GetComponent<Rigidbody2D>();
        sfx = SFXObjects.Instance;
        controller = PlayerController.Instance;
    }

    private void Update()
    {
        if (!isTriggered)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < triggerDistance)
            {
                isTriggered = true;
                timeToShoot = Random.value * 5;
            }
        }
        else
        {
            Aim();
            if (timeToShoot <= 0)
                Shoot();
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
            RaycastHit2D r = Physics2D.Raycast(transform.position, Vector2.right, 1);
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
            RaycastHit2D l = Physics2D.Raycast(transform.position, Vector2.left, 1);
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
        {
            Destroy(gameObject);
            controller.AddPoints(10);
            controller.AddCoins(Random.Range(2, 5));
            sfx.InstantiateParticle(transform.position, ParticleType.Death);
            
            // Spawn coins
            //int i = Random.Range(2, 5);
            //while (i > 0)
            //{
            //    sfx.InstantiateCoin(transform.position + (Vector3.one * Random.value));
            //    i--;
            //}
        }

        hpBar.transform.localScale = new Vector3(hpBarScale.x * ((float)currentHp / (float)maxHp), hpBarScale.y, hpBarScale.z);
    }

    private void Shoot()
    {
        if (Random.value > dashChance)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.rotation = weaponPivot.transform.rotation;
            bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
            sfx.PlaySound(transform.position, SoundType.Shoot);
        }
        else
        {
            Dash();
        }

        timeToShoot = Random.value * 3;
    }

    private void Dash()
    {
        RaycastHit2D shoot;
        shoot = Physics2D.Raycast(transform.position, weaponPivot.transform.right, 10);
        if (shoot.collider == null || shoot.collider.tag == "Player")
            mRigidbody2D.AddForce(weaponPivot.transform.right * 15, ForceMode2D.Impulse);
        else
            Shoot();
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
