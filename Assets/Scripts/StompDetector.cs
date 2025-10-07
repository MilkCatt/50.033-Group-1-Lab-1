using UnityEngine;

public class StompDetector : MonoBehaviour
{
    public MarioMovement player;         // drag your player here
    public JumpOverGoomba scoreManager;   // drag the score/scoreText host here (same GO that holds it)
    public float bounceImpulse = 10f;     // tweak to taste

    private Rigidbody2D _playerRb;

    void Awake()
    {
        _playerRb = player.GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        // Only stomp if player is moving downward (prevents abuse while rising)
        if (_playerRb.linearVelocity.y > 0f) return;

        var goomba = other.GetComponent<Goomba>();
        if (goomba == null) return;

        // 1) Kill the goomba
        goomba.Stomp();

        // 2) Bounce the player
        var v = _playerRb.linearVelocity;
        v.y = 0f;
        _playerRb.linearVelocity = v;
        _playerRb.AddForce(Vector2.up * bounceImpulse, ForceMode2D.Impulse);

        // 3) Award points on kill (replaces your “jump over = score”)
        if (scoreManager) scoreManager.AddScore(goomba.pointsOnStomp);

        // 4) Give a very brief grace window so landing frames don’t kill the player
        player.SetJustStompedGrace();
    }
}
