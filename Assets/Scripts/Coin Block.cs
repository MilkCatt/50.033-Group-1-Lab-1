using System.Collections;
using UnityEngine;

public class CoinBox : MonoBehaviour
{
   public Rigidbody2D boxRb;        // this box's RB2D (Dynamic)
   public SpringJoint2D spring;      // SpringJoint2D on the box
   public Animator animator;         // has bool "Used"
   public AudioSource audioSource;   // on the box
   public AudioClip coinSfx;         // coin sound
   public GameObject coinPrefab;     // just needs a SpriteRenderer (no script)

   public float bumpImpulse = 3.0f;  // kick the spring when hit

   public float coinSpawnOffsetY = 0.6f;
   public float coinRiseHeight = 1.2f;
   public float coinRiseTime = 0.22f;
   public float coinFallTime = 0.22f;

   public bool used = false;

   void Reset()
   {
      boxRb = GetComponent<Rigidbody2D>();
      spring = GetComponent<SpringJoint2D>();
      animator = GetComponent<Animator>();
      audioSource = GetComponent<AudioSource>();
   }

   void OnCollisionEnter2D(Collision2D col)
   {
      if (used) return;
      if (!col.collider.CompareTag("Player")) return;
      if (HitFromBelow(col)) return;

      StartCoroutine(HandleHitSequence());
   }

   private bool HitFromBelow(Collision2D col)
   {
      // "From below" → contact normal points downward on THIS box (y < -0.5)
      foreach (var c in col.contacts)
         if (c.normal.y < -0.5f) return true;
      return false;
   }

   private IEnumerator HandleHitSequence()
   {
      used = true;

      // 1) Visual: mark as used (stop blinking via Animator)
      if (animator) animator.SetBool("Used", true);

      // 2) Kick the spring once (bounce)
      if (boxRb)
      {
         boxRb.linearVelocity = Vector2.zero;
         boxRb.angularVelocity = 0f;
         boxRb.AddForce(Vector2.up * bumpImpulse, ForceMode2D.Impulse);
      }

      // 3) Coin: spawn, rise, SFX at peak, fall, then destroy
      if (audioSource && coinSfx) audioSource.PlayOneShot(coinSfx);

      if (coinPrefab)
      {
         Vector3 startPos = transform.position + Vector3.up * coinSpawnOffsetY;
         GameObject coin = Instantiate(coinPrefab, startPos, Quaternion.identity);
         Transform ct = coin.transform;

         // rise
         Vector3 peak = startPos + Vector3.up * coinRiseHeight;
         float t = 0f;
         float rt = Mathf.Max(0.0001f, coinRiseTime);
         while (t < 1f)
         {
            ct.position = Vector3.Lerp(startPos, peak, t);
            t += Time.deltaTime / rt;
            yield return null;
         }
         ct.position = peak;

         // fall
         t = 0f;
         float ft = Mathf.Max(0.0001f, coinFallTime);
         while (t < 1f)
         {
            ct.position = Vector3.Lerp(peak, startPos, t);
            t += Time.deltaTime / ft;
            yield return null;
         }

         Destroy(coin);
      }

      // 4) No more bouncing: disable spring and lock the body
      if (spring) spring.enabled = false;
      if (boxRb)
      {
         boxRb.linearVelocity = Vector2.zero;
         boxRb.bodyType = RigidbodyType2D.Static; // lock forever
      }
   }
}
