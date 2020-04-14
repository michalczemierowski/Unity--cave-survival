using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Settings")] [Space]
    [SerializeField] [Tooltip("Movement speed multipler")] private float movementSpeedMultipler = 2f;
    [SerializeField] [Tooltip("Number of jumps")] private int maxJumps = 2;
    [Space(10)]
    public Weapon weapon;
    [SerializeField] private LayerMask shootingRayLayerMask;
    [Space(10)]
    [SerializeField] [Tooltip("Duration of the stunning effect")] private float stunDuration = 1.3f;
    [SerializeField] [Tooltip("Can a player shoot when he's stunned")] private bool canShootWhileStunned;
    [SerializeField] [Tooltip("Duration of the dash")] private float dashDuration = 2f;
    [SerializeField] [Tooltip("The price of using the dash")] private int dashCost = 15;

    [Space] [Header("Game Objects")] [Space]
    [SerializeField] [Tooltip("Bullet GameObject")] private GameObject bulletPrefab;
    [SerializeField] [Tooltip("The object on whose position the bullets will be created")] private Transform bulletSpawnPos;
    [SerializeField] [Tooltip("The object around which the weapon will rotate")] private GameObject weaponPivot;
    [SerializeField] [Tooltip("Cinemachine virtual camera")] CameraShake cmVirtualCamShake;

    [Space] [Header("UI")] [Space]
    [SerializeField] [Tooltip("Reload progress image with type \"Filled\"")] private Image reloadProgress;
    [SerializeField] [Tooltip("Healthbar image with type \"Filled\"")] private Image healthBar;
    [SerializeField] [Tooltip("Healthbar flash color")] private Color heathBarFlashColor = Color.red;
    [SerializeField] [Tooltip("Remaining jumps image with type \"Filled\"")] private Image jumpsLeftBar;
    [SerializeField] [Tooltip("Cursor image")] private GameObject crosshair;
    [SerializeField] [Tooltip("Text in which the number of points will be displayed")] private Image pointsBar;
    [SerializeField] [Tooltip("Text in which the number of coins will be displayed")] private Text coinsText;
    [SerializeField] [Tooltip("Canvas which will be displayer on death")] private GameObject DeathCanvas;

    private CharacterController2D controller;
    private EffectHandler effectHandler;
    private StatsManager statsManager;
    private TilemapManager tileManager;
    private EnemySpawner spawner;
    private Rigidbody2D mRigidbody2D;
    private Animator mAnimator;
    private LineRenderer lineRenderer;

    private float movementForce;
    private float gravityScale;
    private bool jump;

    protected bool canShoot;
    protected bool shootingBlockade;
    protected float timeLeft;

    protected float Health = 1f;
    protected float Points;
    protected int Coins = 5;
    protected int Level;

    protected bool isDashing;
    protected bool isInvulnerable;
    protected bool isControllable = true;
    protected bool Dead;

    private Camera cam;
    private Vector3 lookPos;
    private Color startHealthBarColor;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mRigidbody2D = GetComponent<Rigidbody2D>();
        mAnimator = GetComponent<Animator>();
        controller = GetComponent<CharacterController2D>();
        lineRenderer = GetComponentInChildren<LineRenderer>();

        cam = Camera.main;
        effectHandler = EffectHandler.Instance;
        tileManager = TilemapManager.Instance;
        spawner = EnemySpawner.Instance;
        statsManager = StatsManager.Instance;

        timeLeft = weapon.timeBetweenShoots;
        gravityScale = mRigidbody2D.gravityScale;
        mAnimator.SetFloat("shootSpeed", weapon.timeBetweenShoots * 5);

        Cursor.visible = false;

        startHealthBarColor = healthBar.color;
        healthBar.fillAmount = Health;
        coinsText.text = Coins.ToString();
        pointsBar.fillAmount = Points;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Coin pickup
        /*
        if(collision.tag == "Coin")
        {
            AddCoins(1);
            GameObject collider = collision.gameObject;
            sfx.InstantiateParticle(collider.transform.position, ParticleType.Coin);
            sfx.PlaySound(collider.transform.position, SoundType.Coin);
            Destroy(collider.gameObject);
        }
        */
        if (collision.tag == "Enemy")
        {
            EnemyEventHandler enemy = collision.GetComponentInParent<EnemyEventHandler>();
            if (isInvulnerable)
            {
                //When player is dashing
                enemy.Kill();
            }
            else
            {
                if (!enemy.canDamageMeele)
                    return;

                //When enemy hits player
                if (isControllable)
                {
                    Stun();
                    Damage(enemy.GetDamage());
                }
                var velocity = transform.up * 5;
                collision.GetComponentInParent<Rigidbody2D>().AddForce(-velocity, ForceMode2D.Impulse);
                mRigidbody2D.AddForce(velocity, ForceMode2D.Impulse);
            }
        }
    }

    private void Update()
    {
        if (!Dead)
        {
            Aim();
            HandleInput();
        }
        else if(Time.timeScale > 0)
        {
            Time.timeScale -= Time.deltaTime/2;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
            mRigidbody2D.velocity = weaponPivot.transform.right * 25;
        else if (isControllable)
            controller.Move(movementForce, false, jump);

        controller.DetectFacing();
        jump = false;
    }

    #region PRIVATE FUNCTIONS
    private void HandleInput()
    {
        movementForce = Input.GetAxisRaw("Horizontal") * movementSpeedMultipler;
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
        if (Input.GetButton("Fire1") && canShoot)
        {
            Shoot();
        }
        if (!canShoot && !shootingBlockade)
        {
            timeLeft -= Time.deltaTime;
            reloadProgress.fillAmount = timeLeft / weapon.timeBetweenShoots;
            if (timeLeft <= 0)
            {
                canShoot = true;
                timeLeft = weapon.timeBetweenShoots;
                reloadProgress.fillAmount = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
        }
        if (Input.GetButtonDown("Fire2") && Coins > 0 && !isDashing)
        {
            Dash();
            //if (tileManager.GetTilemap(0).SetTile(lookPos, TileType.White))
            //{
            //    Coins--;
            //    coinsText.text = Coins.ToString();
            //}
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (tileManager.GetTilemap(0).RemoveTile(lookPos))
                effectHandler.InstantiateCoin(new Vector3(lookPos.x, lookPos.y, 0) + transform.position);
        }
    }

    public void Damage(int value)
    {
        if (isInvulnerable)
            return;
        Health -= (float)value / 100f;
        if (Health <= 0)
            Death();

        healthBar.fillAmount = Health;
        StartCoroutine(HealthBarFlash());

        if (isControllable)
            cmVirtualCamShake.Shake(0.2f, shakeAmplitude: 2.5f);

        //STATS
        statsManager?.ReceiveDamage(value);
    }

    private IEnumerator HealthBarFlash()
    {
        healthBar.color = heathBarFlashColor;
        healthBar.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        yield return new WaitForSeconds(0.15f);
        healthBar.transform.localScale = Vector3.one;
        healthBar.color = startHealthBarColor;
    }

    private void Aim()
    {
        lookPos = cam.ScreenToWorldPoint(Input.mousePosition);
        crosshair.transform.position = new Vector3(lookPos.x, lookPos.y, 0);

        lookPos = lookPos - transform.position;
        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;

        weaponPivot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Shoot()
    {
        controller.Shoot();

        if (weapon.weaponType == WeaponType.PROJECTILE)
        {
            ShootProjectile();
        }
        else if (weapon.weaponType == WeaponType.RAYCAST)
        {
            ShootRay();
        }
        else if (weapon.weaponType == WeaponType.EXPLOSIVE)
        {
            ShootExplosive();
        }

        effectHandler.PlaySound(transform.position, SoundType.Shoot);
        canShoot = false;

        //STATS
        statsManager?.Shoot();
    }

    private void ShootProjectile()
    {
        GameObject bullet = Instantiate(weapon.bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
        bullet.GetComponent<BulletEventHandler>().damage = weapon.damage;
        bullet.transform.rotation = weaponPivot.transform.rotation;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * weapon.bulletSpeed, ForceMode2D.Impulse);
    }

    private void ShootExplosive()
    {
        GameObject bullet = Instantiate(weapon.bulletPrefab, bulletSpawnPos.position, Quaternion.identity);

        ExplosiveBullet explosiveBullet = bullet.GetComponent<ExplosiveBullet>();
        explosiveBullet.damage = weapon.damage;
        explosiveBullet.radius = weapon.radius;

        bullet.transform.rotation = weaponPivot.transform.rotation;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * weapon.bulletSpeed, ForceMode2D.Impulse);
    }

    private void ShootRay()
    {
        Vector3 direction = (cam.ScreenToWorldPoint(Input.mousePosition) - bulletSpawnPos.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPos.position, direction, Mathf.Infinity, shootingRayLayerMask);
        if(hit.collider != null)
        {
            if (hit.collider.tag == "Enemy")
            {
                EnemyEventHandler enemy = hit.collider.GetComponentInParent<EnemyEventHandler>();
                enemy.Damage(weapon.damage);

                EffectHandler.Instance.PlaySound(hit.point, SoundType.Hurt);
                EffectHandler.Instance.InstantiateParticleWithRotation(hit.point, ParticleType.EnemyBlood, color: enemy.mainColor, moveRight: false);
            }
            else
            {
                EffectHandler.Instance.PlaySound(hit.point, SoundType.Hit);
                EffectHandler.Instance.InstantiateParticleWithRotation(hit.point, ParticleType.WallPlayer, moveRight: false);
            }

            StartCoroutine(DrawLine(hit.point));
        }
    }

    private IEnumerator DrawLine(Vector2 position)
    {
        lineRenderer.SetPositions(new Vector3[] { bulletSpawnPos.position, position });
        yield return null;
        yield return null;
        lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
    }

    private void Dash()
    {
        if (Coins >= dashCost)
        {
            AddCoins(-dashCost);
            StartCoroutine(IEDash(dashDuration));
            mRigidbody2D.AddForce(weaponPivot.transform.right * 20, ForceMode2D.Impulse);

            //STATS
            statsManager?.Dash();
        }
    }

    private void Stun()
    {
        StartCoroutine(DisableControls(stunDuration, canShootWhileStunned));
        controller.setJumps(maxJumps);

        effectHandler.InstantiateParticle(transform.position, ParticleType.Stun, transform, stunDuration);
        effectHandler.PlaySound(transform.position, SoundType.Stun);

        cmVirtualCamShake.Shake();
    }

    private void Death()
    {
        Instantiate(DeathCanvas, Vector3.zero, Quaternion.identity);
        effectHandler.InstantiateParticle(transform.position, ParticleType.PlayerDeath);

        foreach (EnemyEventHandler enemy in spawner.GetEnemies())
        {
            if (Vector2.Distance(enemy.transform.position, transform.position) < 10)
                enemy.Damage(100);
        }
        isInvulnerable = true;
        isControllable = false;
        Dead = true;

        crosshair.transform.parent.gameObject.SetActive(false);
        Cursor.visible = true;

        //STATS
        statsManager?.Death();
        PrintStats();
    }

    private void PrintStats()
    {
        // TODO: SHOW STATS
        Stats stats = statsManager?.GetStats();
        print(stats.causedDamage + " CAUSED DAMAGE");
        print(stats.receivedDamage + " RECEIVED DAMAGE");
        print(stats.shotsFired + " SHOTS FIRED");
        print(stats.usedDashes + " USED DASHES");
        print(stats.enemiesKilled + " ENEMIES KILLED");
    }

    private void LvlUP()
    {
        Heal(100);
        effectHandler.InstantiateParticle(transform.position, ParticleType.LvlUp, transform);
        effectHandler.PlaySound(transform.position, SoundType.LvlUp);

        Level++;
        spawner.SetLevel(Level);

        foreach (EnemyEventHandler enemy in spawner.GetEnemies())
        {
            if(Vector2.Distance(enemy.transform.position, transform.position) < 10)
                enemy.Kill();
        }

        //STATS
        statsManager?.LvlUP();
    }

    private IEnumerator IEDash(float time)
    {
        isDashing = true;
        mRigidbody2D.gravityScale = 0;
        isInvulnerable = true;
        yield return new WaitForSeconds(time);
        isInvulnerable = false;
        mRigidbody2D.gravityScale = gravityScale;
        isDashing = false;
        controller.setJumps(maxJumps);
    }
    private IEnumerator DisableControls(float time, bool canShootWhileStunned = false)
    {
        isControllable = false;
        if (!canShootWhileStunned) shootingBlockade = true;
        yield return new WaitForSeconds(time);
        if (!canShootWhileStunned) shootingBlockade = false;
        isControllable = true;
    }
    #endregion

    #region PUBLIC FUNCTIONS
    public void Heal(int value)
    {
        Health += (float)value / 100f;
        if (Health > 1f) Health = 1f;
        healthBar.fillAmount = Health;
    }

    public void AddPoints(int value)
    {
        Points += (float)value / 100f;
        if (Points >= 1)
        {
            Points = 0;
            LvlUP();
        }
        pointsBar.fillAmount = Points;
    }

    public void AddCoins(int value)
    {
        Coins += value;
        coinsText.text = Coins.ToString();
    }

    public void ResetJumps()
    {
        controller.setJumps(maxJumps);
        jumpsLeftBar.fillAmount = 1;
    }

    public void setJumps(int value)
    {
        jumpsLeftBar.fillAmount = (float)value / maxJumps;
    }
    #endregion
}
