using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MarioMovement : MonoBehaviour
{
    public float speed = 10;
    public float maxSpeed = 20;
    private Rigidbody2D marioBody;
    public float upSpeed = 10;
    private bool onGroundState = true;
    private SpriteRenderer marioSprite;
    private bool faceRightState = true;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreTextOver;
    public GameObject enemies;
    public JumpOverGoomba jumpOverGoomba;   // now just the score manager
    public GameObject gameOverCanvas;
    public GameObject uiCanvas;
    public Animator marioAnimator;
    public AudioSource marioAudio;
    public AudioClip marioDeath;
    public float deathImpulse = 25;

    // state
    [System.NonSerialized] public bool alive = true;
    public Transform gameCamera;

    // Ground, Enemies, Obstacles Layer
    int collisionLayerMask = (1 << 3) | (1 << 6) | (1 << 7);

    // --- NEW: brief grace after a successful stomp so side contacts won't kill immediately
    private bool justStomped = false;

    void Start()
    {
        // Set to be 30 FPS
        Application.targetFrameRate = 30;
        marioBody = GetComponent<Rigidbody2D>();
        marioSprite = GetComponent<SpriteRenderer>();
        marioAnimator.SetBool("onGround", onGroundState);
    }

    void Update()
    {
        // toggle facing state
        if ((Input.GetKeyDown("a") || Input.GetKeyDown(KeyCode.LeftArrow)) && faceRightState)
        {
            faceRightState = false;
            marioSprite.flipX = true;
            if (marioBody.linearVelocity.x > 0.1f)
                marioAnimator.SetTrigger("onSkid");
        }

        if ((Input.GetKeyDown("d") || Input.GetKeyDown(KeyCode.RightArrow)) && !faceRightState)
        {
            faceRightState = true;
            marioSprite.flipX = false;
            if (marioBody.linearVelocity.x < -0.1f)
                marioAnimator.SetTrigger("onSkid");
        }

        marioAnimator.SetFloat("xSpeed", Mathf.Abs(marioBody.linearVelocity.x));
    }

    void FixedUpdate()
    {
        if (!alive) return;

        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(moveHorizontal) > 0)
        {
            Vector2 movement = new Vector2(moveHorizontal, 0);
            if (marioBody.linearVelocity.magnitude < maxSpeed)
                marioBody.AddForce(movement * speed);
        }

        // stop on key up
        if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
        {
            marioBody.linearVelocity = Vector2.zero;
        }

        // jump
        if (Input.GetKeyDown("space") && onGroundState)
        {
            marioBody.AddForce(Vector2.up * upSpeed, ForceMode2D.Impulse);
            onGroundState = false;
            marioAnimator.SetBool("onGround", onGroundState);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Ground & generic landing check
        if (col.gameObject.CompareTag("Ground") && !onGroundState)
        {
            onGroundState = true;
            marioAnimator.SetBool("onGround", onGroundState);
        }

        if (((collisionLayerMask & (1 << col.transform.gameObject.layer)) > 0) & !onGroundState)
        {
            onGroundState = true;
            marioAnimator.SetBool("onGround", onGroundState);
        }

        // --- NEW: Side-collision with enemy = death (top contacts are handled by StompDetector)
        if (alive && col.collider.CompareTag("Enemy"))
        {
            if (justStomped) return; // ignore immediately after a stomp bounce

            bool landedOnTop = false;
            foreach (var c in col.contacts)
            {
                if (c.normal.y > 0.45f)
                {
                    landedOnTop = true;
                    break;
                }
            }

            // If not a top contact, treat as lethal side/bottom hit
            if (!landedOnTop)
            {
                DoDeathSequence();
            }
        }
    }

    // --- REMOVED: old trigger-based instant death with enemies ---
    // void OnTriggerEnter2D(Collider2D other) { ... }

    private void DoDeathSequence()
    {
        Debug.Log("Collided with goomba (side/bottom)!");
        scoreTextOver.text = scoreText.text;
        marioAnimator.Play("mario-die");
        marioAudio.PlayOneShot(marioDeath);
        alive = false;
    }

    public void SetJustStompedGrace()
    {
        if (gameObject.activeInHierarchy) StartCoroutine(_Grace());
    }

    private IEnumerator _Grace()
    {
        justStomped = true;
        yield return new WaitForSeconds(0.15f); // a few frames of safety
        justStomped = false;
    }

    public void RestartButtonCallback(int input)
    {
        Debug.Log("Restart!");
        // show ui
        uiCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        ResetGame();
        Time.timeScale = 1.0f;
    }

    private void ResetGame()
    {
        // reset position
        marioBody.transform.position = new Vector3(-5.00f, -1.0f, 0.0f);

        // reset sprite direction
        faceRightState = true;
        marioSprite.flipX = false;

        // reset score text (score value is reset in JumpOverGoomba below)
        scoreText.text = "Score: 0";

        // reset Goombas to start positions
        foreach (Transform eachChild in enemies.transform)
        {
            var em = eachChild.GetComponent<EnemyMovement>();
            if (em != null) eachChild.transform.localPosition = em.startPosition;
        }

        // reset score value
        if (jumpOverGoomba != null) jumpOverGoomba.score = 0;

        marioAnimator.SetTrigger("gameRestart");
        alive = true;

        // reset camera position
        gameCamera.position = new Vector3(0, 0, -10);
    }

    void PlayJumpSound()
    {
        marioAudio.PlayOneShot(marioAudio.clip);
    }

    void PlayDeathImpulse()
    {
        marioBody.AddForce(Vector2.up * deathImpulse, ForceMode2D.Impulse);
    }

    void GameOverScene()
    {
        Time.timeScale = 0.0f;
        uiCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
    }
}
