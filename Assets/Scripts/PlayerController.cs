using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Settings")][Space]
    [SerializeField][Tooltip("Movement speed multipler")] private float movementSpeedMultipler = 2f;
    [SerializeField][Tooltip("Number of jumps")] private int maxJumps = 2;
    [SerializeField][Tooltip("Bullet velocity multipler")] private float bulletSpeed = 5;
    [SerializeField][Tooltip("Time between shots")] private float timeBetweenShoots = 0.3f;
    [SerializeField][Tooltip("Duration of the stunning effect")] private float stunDuration = 1.3f;
    [SerializeField][Tooltip("Can a player shoot when he's stunned")] private bool canShootWhileStunned;
    [SerializeField][Tooltip("Duration of the dash")] private float dashDuration = 2f;
    [SerializeField][Tooltip("The price of using the dash")] private int dashCost = 15;

    [Space][Header("Game Objects")][Space]
    [SerializeField][Tooltip("Bullet GameObject")] private GameObject bulletPrefab;
    [SerializeField][Tooltip("The object on whose position the bullets will be created")] private Transform bulletSpawnPos;
    [SerializeField][Tooltip("The object around which the weapon will rotate")] private GameObject weaponPivot;

    [Space][Header("UI")][Space]
    [SerializeField][Tooltip("Reload progress image with type \"Filled\"")] private Image reloadProgress;
    [SerializeField][Tooltip("Healthbar image with type \"Filled\"")] private Image healthBar;
    [SerializeField][Tooltip("Remaining jumps image with type \"Filled\"")] private Image jumpsLeftBar;
    [SerializeField][Tooltip("Cursor image")] private GameObject crosshair;
    [SerializeField][Tooltip("Text in which the number of points will be displayed")] private Image pointsBar;
    [SerializeField][Tooltip("Text in which the number of coins will be displayed")] private Text coinsText;

    private CharacterController2D controller;
    private EffectHandler effectHandler;
    private TilemapManager tileManager;

    private float movementForce;
    private float gravityScale;
    private Rigidbody2D mRigidbody2D;
    private bool jump;

    protected bool canShoot;
    protected bool shootingBlockade;
    protected float timeLeft;

    protected float Health = 1f;
    protected float Points;
    protected int Coins = 5;

    protected bool isDashing;
    protected bool isInvulnerable;
    protected bool isControllable = true;

    private Camera cam;
    private Vector3 lookPos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mRigidbody2D = GetComponent<Rigidbody2D>();
        controller = GetComponent<CharacterController2D>();
        cam = Camera.main;
        effectHandler = EffectHandler.Instance;
        tileManager = TilemapManager.Instance;
        timeLeft = timeBetweenShoots;

        gravityScale = mRigidbody2D.gravityScale;

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
        Aim();
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (isDashing)
            mRigidbody2D.velocity = weaponPivot.transform.right * 25;
        else if(isControllable)
            controller.Move(movementForce, false, jump);
        jump = false;
    }

    #region PRIVATE FUNCTIONS
    private void HandleInput()
    {
        crosshair.transform.position = Input.mousePosition;
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
        if(Health <= 0)
        {
            // TODO: DEATH
            Health = 0;
        }
        healthBar.fillAmount = Health;
    }

    private void Aim()
    {
        lookPos = cam.ScreenToWorldPoint(Input.mousePosition);

        lookPos = lookPos - transform.position;
        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;

        weaponPivot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
        bullet.transform.rotation = weaponPivot.transform.rotation;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
        effectHandler.PlaySound(transform.position, SoundType.Shoot);
        canShoot = false;
    }

    private void Dash()
    {
        if (Coins >= dashCost)
        {
            AddCoins(-dashCost);
            StartCoroutine(IEDash(dashDuration));
            mRigidbody2D.AddForce(weaponPivot.transform.right * 20, ForceMode2D.Impulse);
        }
    }

    private void Stun()
    {
        Damage(10);
        effectHandler.InstantiateParticle(transform.position, ParticleType.Stun, transform, stunDuration);
        effectHandler.PlaySound(transform.position, SoundType.Stun);
    }

    private void LvlUP()
    {
        Heal(100);
        effectHandler.InstantiateParticle(transform.position, ParticleType.LvlUp, transform);
        effectHandler.PlaySound(transform.position, SoundType.LvlUp);
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if(Vector2.Distance(enemy.transform.position, transform.position) < 10)
            {
                var enemyEvent = enemy.GetComponent<EnemyEventHandler>();
                enemyEvent.Damage(100);
            }
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
