using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Settings")]
    [Space]
    [SerializeField][Tooltip("Movement speed multipler")]
    float movementSpeedMultipler = 2f;
    [SerializeField][Tooltip("Bullet velocity multipler")]
    float bulletSpeed = 5;
    [SerializeField][Tooltip("Time between shots")]
    float timeBetweenShoots = 0.3f;
    [Header("Game Objects")]
    [Space]
    [SerializeField][Tooltip("Bullet GameObject")]
    GameObject bulletPrefab;
    [SerializeField][Tooltip("The object on whose position the bullets will be created")]
    Transform bulletSpawnPos;
    [SerializeField][Tooltip("The object around which the weapon will rotate")]
    GameObject weaponPivot;

    [Space]
    [Header("UI")]
    [Space]
    [SerializeField][Tooltip("Reload progress image with type \"Filled\"")]
    Image reloadProgress;
    [SerializeField][Tooltip("Healthbar image with type \"Filled\"")]
    Image healthBar;
    [SerializeField][Tooltip("Cursor image")]
    GameObject crosshair;
    [SerializeField][Tooltip("Text in which the number of points will be displayed")]
    Image pointsBar;
    [SerializeField][Tooltip("Text in which the number of coins will be displayed")]
    Text coinsText;

    CharacterController2D controller;
    SFXObjects sfx;
    TilemapManager tileManager;


    private float movementForce;
    private float gravityScale;
    private Rigidbody2D mRigidbody2D;
    private bool jump;

    protected bool canShoot;
    protected float timeLeft;

    protected float Health = 1f;
    protected float Points;
    protected int Coins = 5;

    protected bool isDashing;
    protected bool isInvulnerable;
    protected bool isControllable = true;

    private Camera cam;
    private Vector3 lookPos;

    private Vector3[] localScale = new Vector3[] { Vector3.one, new Vector3(-1, 1, 1) };

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mRigidbody2D = GetComponent<Rigidbody2D>();
        controller = GetComponent<CharacterController2D>();
        cam = Camera.main;
        sfx = SFXObjects.Instance;
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
        if (collision.tag == "Enemy")
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
                    Damage(10);
                var velocity = transform.up * 5;
                collision.GetComponentInParent<Rigidbody2D>().AddForce(-velocity, ForceMode2D.Impulse);
                mRigidbody2D.AddForce(velocity, ForceMode2D.Impulse);
                StartCoroutine(DisableControls(1));
                controller.setJumps(2);
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
        if (!canShoot)
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
                sfx.InstantiateCoin(new Vector3(lookPos.x, lookPos.y, 0) + transform.position);
        }
    }

    public void Damage(int value)
    {
        if (isInvulnerable)
            return;
        Health -= (float)value / 100f;
        if(Health <= 0)
        {
            //TODO
        }
        healthBar.fillAmount = Health;
    }

    private void Aim()
    {
        lookPos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (lookPos.x > transform.position.x)
            weaponPivot.transform.localScale = localScale[0];
        else
            weaponPivot.transform.localScale = localScale[1];

        lookPos = lookPos - transform.position;
        float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;

        weaponPivot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.identity);
        bullet.transform.rotation = weaponPivot.transform.rotation;
        bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right * bulletSpeed, ForceMode2D.Impulse);
        sfx.PlaySound(transform.position, SoundType.Shoot);
        canShoot = false;
    }

    private void Dash()
    {
        StartCoroutine(IEDash());
        mRigidbody2D.AddForce(weaponPivot.transform.right * 20, ForceMode2D.Impulse);
    }

    private IEnumerator IEDash()
    {
        isDashing = true;
        mRigidbody2D.gravityScale = 0;
        isInvulnerable = true;
        yield return new WaitForSeconds(1.5f);
        isInvulnerable = false;
        mRigidbody2D.gravityScale = gravityScale;
        isDashing = false;
        controller.setJumps(2);
    }
    private IEnumerator DisableControls(float time)
    {
        isControllable = false;
        yield return new WaitForSeconds(time);
        isControllable = true;
    }
    #endregion

    #region PUBLIC FUNCTIONS
    public void Heal(int value)
    {
        Health += (float)value / 100f;
        if (Health > 1f) Health = 1f;
    }

    public void AddPoints(int value)
    {
        Points += (float)value / 100f;
        if (Points >= 1)
        {
            Points = 0;
            Health = 1;
            healthBar.fillAmount = Health;
            sfx.InstantiateParticle(transform.position, ParticleType.LvlUp, transform);
        }
        pointsBar.fillAmount = Points;
    }

    public void AddCoins(int value)
    {
        Coins += value;
        coinsText.text = Coins.ToString();
    }
    #endregion
}
