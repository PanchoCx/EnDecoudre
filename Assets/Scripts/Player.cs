using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


public enum States { OnFoot, Flying, Waiting, Dashing }

public class Player : MonoBehaviour
{
    public int hp;
    [SerializeField] public int maxHP;

    public Power currentPower;

    [HideInInspector]
    public List<PowerEnum> availablePowers;

    private System.Random rnd = new System.Random();

    public bool isDead = false;

    [SerializeField] int indexOfPrefab;
    GameObject deathBubble;
    GameObject throwBubble;
    Transform _groudCircle;
    private int indexOfFace;
    public static event Action<int> ThePlayerSpawns;
    public States playerState;
    private CircleCollider2D detection;
    public PlayerMovement playerMovement;
    public Transform playerTransform;
    Animator anim;

    private bool isInvincible = false;
    [SerializeField]
    private float invincibilityDurationSeconds;
    [SerializeField]
    private float invincibilityDeltaTime;
    UImanager _uimanager;
   // private Rigidbody2D rb;

    bool canFire;

    private float duration = 2f;

    public int currentFace;

    private SpriteRenderer spriteRenderer;

    [SerializeField]
    public List<PowerData> listPowerPrefabs;

    void Awake()
    {
        _uimanager = GameObject.FindGameObjectWithTag("UImanager").GetComponent<UImanager>();
        playerState = States.OnFoot;
        anim = this.GetComponent<Animator>();
        detection = gameObject.GetComponent<CircleCollider2D>();
        playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerTransform = gameObject.GetComponent<Transform>();
       // rb = gameObject.GetComponent<Rigidbody2D>();
        deathBubble = transform.GetChild(1).gameObject;
        deathBubble.SetActive(false);
        throwBubble = transform.GetChild(2).gameObject;
        throwBubble.SetActive(false);

        _groudCircle = transform.GetChild(3);

        hp = maxHP;
        canFire = true;
        availablePowers = new List<PowerEnum>() { PowerEnum.Nova, PowerEnum.Shotgun, PowerEnum.Boomerang, PowerEnum.Dash, PowerEnum.Sword, PowerEnum.Machinegun };

        spriteRenderer = GetComponent<SpriteRenderer>();
    }



    void Start()
    {
        Roll();
        _uimanager.SwapSkill(currentPower, this);
        _uimanager.UpdateShotCount(currentPower, this);
        playerTransform.position = new Vector2(indexOfPrefab, 0);
        SoundAssets.instance.PlaySpawnSound();
        ThePlayerSpawns?.Invoke(indexOfPrefab);
    }

    public int getIndexOfPrefab()
    {
        return indexOfPrefab;
    }

    public void Yeet(InputAction.CallbackContext context) //Se mettre en position d'attente du Yeet
    {
       if (playerState == States.OnFoot)
        {
            playerState = States.Waiting;
            anim.SetBool("onWait", true);
            throwBubble.SetActive(true);
            playerMovement.setSpeed(0);
            StartCoroutine(ReviveCoroutine());
        }
        if (context.canceled)
        {
            playerState = States.OnFoot;
            anim.SetBool("onWait", false);
            throwBubble.SetActive(false);
            if (!isDead) {
                playerMovement.setSpeed(playerMovement.maxSpeed); 
            }
        }
    }

    private IEnumerator ReviveCoroutine()
    {
        Collider2D[] colliders = new Collider2D[10];
        var tempNumber = this.GetComponent<BoxCollider2D>().OverlapCollider(new ContactFilter2D(), colliders);
        Collider2D temp = colliders.FirstOrDefault(x => x!=null && x.CompareTag("Player"));
        
        if ( tempNumber > 0 && temp != null && temp.gameObject.GetComponent<Player>() is Player otherPlayer)
        {
            if (otherPlayer.isDead)
            {
                otherPlayer.Revive();
            }
        }
        yield return new WaitForSeconds(0.3f);
    }

    private void Update()
    {
        //print(playerState);
    }
        //print(this.GetComponent<PlayerInput>().currentControlScheme);

    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));

    }

    private static Vector2 GroundCircleDUringParabola(Vector2 start, Vector2 end, float height, float t)
    {
        return Vector2.Lerp(start, end, t);
    }


    public void Roll()
    {
        currentFace = rnd.Next(0, availablePowers.Count); //Next(int x, int y) returns a value between x and y, upper bound excluded.
        //currentFace = (int)PowerEnum.Machinegun;

        Debug.LogFormat("Cx : {0} rolled {1}", this.gameObject.name, availablePowers[currentFace]);
        currentPower = Power.GetPower(this, availablePowers[currentFace], listPowerPrefabs);
        indexOfFace = currentFace +1;
    }


    public IEnumerator Fly(Vector2 start, Vector2 finish)
    {
        this.GetComponent<PlayerMovement>().SetOnFly(true);
        SoundAssets.instance.PlayYeetSound(getIndexOfPrefab());
        float animation = 0f;
        //this.GetComponent<BoxCollider2D>().isTrigger = true;
        anim.SetBool("onFly", true);
        // faut lancer ROLL pour que sa change la valeur de indexOfFace
        Roll();
        Debug.LogFormat("index of Face = {0}", indexOfFace);
        while (animation < duration)
        {
            animation += Time.deltaTime;
            //transform.position = Parabola(start, finish, duration, animation / duration);
            transform.position = Parabola(start, finish, duration, animation / duration);
            _groudCircle.position = GroundCircleDUringParabola(start, finish, duration, animation / duration);
            Collider2D[] colliders = new Collider2D[2];
            if(this.GetComponent<BoxCollider2D>().OverlapCollider(new ContactFilter2D(), colliders) > 0 && !colliders.First().gameObject.CompareTag("Player"))
            {
                break;
            }
            anim.SetInteger("indexOfFace", indexOfFace);
            //lancer l'aleatoire entre 1 et 6 avec powers ? en gros tenir a jour une valeur int faceValue pour que des l'atterissage on soit dans la bonne animation
            yield return null;
        }

        _groudCircle.position = gameObject.transform.position;

        this.GetComponent<PlayerMovement>().SetInput(0, 0);
        this.GetComponent<PlayerMovement>().SetOnFly(false);
        playerState = States.OnFoot;
        //this.GetComponent<BoxCollider2D>().isTrigger = false;
        _uimanager.SwapSkill(currentPower, this);
        anim.SetBool("onFly", false);
        yield return null;
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State == GameManager.GameState.InGame && !isDead)
        {
            if (context.performed)
            {
                if (currentPower != null && currentPower.currentCharges > 0 && canFire)
                {
                    StartCoroutine(CooldownAttack());
                    currentPower.currentCharges--;
                    currentPower.ActivateOnce(this);
                    _uimanager.UpdateShotCount(currentPower, this);
                }
            }
        }
    }

    private IEnumerator CooldownAttack()
    {
        canFire = false;
        yield return new WaitForSeconds(1);
        canFire = true;
        yield return null;
    }

    private void die()
    {
        GameManager.Instance.HandlePLayerDied();
        SoundAssets.instance.PlayPlayerDieSound(getIndexOfPrefab());
        deathBubble.SetActive(true);
        anim.SetBool("died", true);
        playerMovement.setSpeed(0);
        playerMovement.SetInput(0, 0);
        isDead = true;
        Debug.Log("{0} died");
    }

    public void Revive()
    {
        hp = maxHP;
        isDead = false;
        deathBubble.SetActive(false);
        playerMovement.setSpeed(playerMovement.maxSpeed);
        anim.SetBool("died", false);
        float[] direction = playerMovement.getDirection();
        StartCoroutine(Fly(new Vector2(playerTransform.position.x, playerTransform.position.y), new Vector2(playerTransform.position.x + direction[0] * 5, playerTransform.position.y + direction[1] * 2)));
        GameManager.Instance.HandlePlayerResurect();
    }

    public void takeDamage(int value)
    {
        if (isInvincible) return;

        if (hp > 0)
        {
            hp -= value;
            SoundAssets.instance.PlayTakeDamagePlayer(getIndexOfPrefab());
            Debug.LogFormat("{0} lost {1} Hp", gameObject.name, value);
            _uimanager.UpdatePlayerLife(this);
        }

        if ( hp <= 0 && !isDead)
        {
            hp = -1;
            die();
            _uimanager.UpdatePlayerLife(this);
            return;
        }

        StartCoroutine(BecomeTemporarilyInvincible());
        
    }

    private IEnumerator BecomeTemporarilyInvincible()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDurationSeconds; i += invincibilityDeltaTime)
        {
            // Alternate between 0 and 1 scale to simulate flashing
            if (spriteRenderer.enabled)
            {
                spriteRenderer.enabled = false;
            }
            else
            {
                spriteRenderer.enabled = true;
            }
            yield return new WaitForSeconds(invincibilityDeltaTime);
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
    }
}




