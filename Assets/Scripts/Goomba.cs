using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Goomba : MonoBehaviour
{
    public EnemyMovement mover;      // optional: link your enemy patrol script
    public Animator animator;        // optional: set a "Squish" trigger
    public AudioSource audioSource;  // optional
    public AudioClip squishSfx;      // optional
    public int pointsOnStomp = 100;

    private bool _dead = false;
    private Rigidbody2D _rb;
    private Collider2D _col;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

    public void Stomp()
    {
        if (_dead) return;
        _dead = true;

        if (mover) mover.enabled = false;              // stop patrolling
        if (animator) animator.SetTrigger("Flatten");   // play Flatten anim
        if (audioSource && squishSfx) audioSource.PlayOneShot(squishSfx);

        if (_rb) _rb.linearVelocity = Vector2.zero;    // stop movement
        if (_col) _col.enabled = false;               // no more hits

        Destroy(gameObject, 0.25f);                   // small delay to show feedback
    }
}
