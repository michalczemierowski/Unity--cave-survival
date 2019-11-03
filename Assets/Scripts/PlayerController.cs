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
    [SerializeField] [Tooltip("Bullet velocity multipler")] private float bulletSpeed = 5;
    [SerializeField] [Tooltip("Bullet damage")] private int bulletDamage = 5;
    [SerializeField] [Tooltip("Time between shots")] private float timeBetweenShoots = 0.3f;
    [SerializeField] [Tooltip("Duration of the stunning effect")] private float stunDuration = 1.3f;
    [SerializeField] [Tooltip("Can a player shoot when he's stunned")] private bool canShootWhileStunned;
    [SerializeField] [Tooltip("Duration of the dash")] private float dashDuration = 2f;
    [SerializeField] [Tooltip("The price of using the dash")] private int dashCost = 15;

    [Space] [Header("Game Objects")] [Space]
    [SerializeField] [Tooltip("Bullet GameObject")] private GameObject bulletPrefab;
    [SerializeField] [Tooltip("The object on whose position the bullets will be created")] private Transform bulletSpawnPos;
    [SerializeField] [Tooltip("The object around which the weapon will rotate")] private GameObject weaponPivot;

    [Space] [Header("UI")] [Space]
    [SerializeField] [Tooltip("Reload progress image with type \"Filled\"")] private Image reloadProgress;
    [SerializeField] [Tooltip("Healthbar image with type \"Filled\"")] private Image healthBar;
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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mRigidbody2D = GetComponent<Rigidbody2D>();
        mAnimator = GetComponent<Animator>();
        controller = GetComponent<CharacterController2D>();
        cam = Camera.main;
        effectHandler = EffectHandler.Instance;
        tileManager = TilemapManager.Instance;
        spawner = EnemySpawner.Instance;
        statsManager = StatsManager.Instance;
        timeLeft = timeBetweenShoots;

        gravityScale = mRigidbody2D.gravityScale;
        mAnimator.SetFloat("shootSpeed", timeBetweenShoots * 5);
        Cursor.visible = false;

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
        if (collision.tag == "EnemyWeapon")
        {
            if (isInvulnerable)
            {
                //When player is dashing
                collision.GetComponentInParent<EnemyEventHandler>().Damage(100);
            }
            else
            {
                //When enemy hits player
                if (isControllable)
                {
                    Stun();
                    Damage(collision.GetComponentInParent<EnemyEventHandler>().GetDamage());
                }
                var velocity = transform.up * 5;
                collision.GetComponentInParent<Rigidbody2D>().AddForce(-velocity, ForceMode2D.Impulse);
                mRigidbody2D.AddForce(velocity, ForceMode2D.Impulse);
                StartCoroutine(DisableControls(stunDuration, canShootWhileStunned));
                controller.setJumps(maxJumps);
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
            reloadProgress.fillAmount = timeLeft / timeBetweenShoots;
            if (timeLeft <= 0)
            {
                canShoot = true;
                timeLeft = timeBetweenShoots;
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

        //STATS
        statsManager.ReceiveDamage(value);
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
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
        bullet.GetComponent<BulletEventHandler>().damage = bulletDamage;
        bullet.transform.rotation = weaponPivot.transform.rotation;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
        effectHandler.PlaySound(transform.position, SoundType.Shoot);
        canShoot = false;

        //STATS
        statsManager.Shoot();
    }

    private void Dash()
    {
        if (Coins >= dashCost)
        {
            AddCoins(-dashCost);
            StartCoroutine(IEDash(dashDuration));
            mRigidbody2D.AddForce(weaponPivot.transform.right * 20, ForceMode2D.Impulse);

            //STATS
            statsManager.Dash();
        }
    }

    private void Stun()
    {
        effectHandler.InstantiateParticle(transform.position, ParticleType.Stun, transform, stunDuration);
        effectHandler.PlaySound(transform.position, SoundType.Stun);
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

        PrintStats();
    }

    private void PrintStats()
    {
        // TODO: SHOW STATS
        Stats stats = statsManager.GetStats();
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
                enemy.Damage(100);
        }
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
