using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;     // Mario
    public Transform endLimit;   // level end anchor

    [Header("Horizontal Follow")]
    [Tooltip("Pixels (world units) to keep as initial X offset from player.")]
    public float xOffset = 0f;
    [Tooltip("Smooth time for horizontal damping.")]
    public float xSmoothTime = 0.08f;

    [Header("Vertical Follow")]
    [Tooltip("How far above the player the camera is allowed to float at most.")]
    public float maxRiseAboveGround = 3.0f;
    [Tooltip("Minimum ground Y for the camera (usually your floor).")]
    public float minY = 0.0f;
    [Tooltip("Vertical smoothing.")]
    public float ySmoothTime = 0.12f;

    private float _startX;    // smallest X of camera
    private float _endX;      // largest X of camera
    private float _viewportHalfWidth;

    private float _xVel;      // SmoothDamp velocity caches
    private float _yVel;

    private float _groundBaselineY; // snapshot of where camera sits when player is grounded

    void Start()
    {
        if (!player) { Debug.LogError("CameraController: Missing player."); enabled = false; return; }

        // compute viewport half width (orthographic)
        var cam = GetComponent<Camera>();
        float halfHeight = cam.orthographicSize;
        float halfWidth  = halfHeight * cam.aspect;
        _viewportHalfWidth = halfWidth;

        // establish horizontal clamp range
        _startX = transform.position.x; // assume scene starts framed correctly
        float endWorldX = endLimit ? endLimit.position.x : _startX;
        _endX = endWorldX - _viewportHalfWidth;

        // initial horizontal offset from player (optional)
        xOffset = Mathf.Approximately(xOffset, 0f) ? (transform.position.x - player.position.x) : xOffset;

        // baseline ground Y is current camera Y at start
        _groundBaselineY = Mathf.Max(minY, transform.position.y);
    }

    void LateUpdate()
    {
        if (!player) return;

        // --- Horizontal follow with clamp ---
        float targetX = player.position.x + xOffset;
        // clamp to [startX, endX]
        targetX = Mathf.Clamp(targetX, _startX, _endX);
        float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref _xVel, xSmoothTime);

        // --- Vertical follow (limited rise) ---
        // We allow the camera to rise up to maxRiseAboveGround from the *baseline*.
        float desiredY = _groundBaselineY;

        // If player is above baseline, float up towards a limited ceiling
        if (player.position.y > _groundBaselineY)
        {
            float capY = _groundBaselineY + maxRiseAboveGround;
            desiredY = Mathf.Min(player.position.y, capY);
        }

        // Always respect a minimum Y (e.g., ground/floor)
        desiredY = Mathf.Max(desiredY, minY);

        float newY = Mathf.SmoothDamp(transform.position.y, desiredY, ref _yVel, ySmoothTime);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    /// <summary>
    /// Call this from your player when he lands to "re-anchor" the baseline,
    /// so the camera doesn't keep floating high after big jumps.
    /// </summary>
    public void ReanchorGroundBaseline(float playerGroundY)
    {
        _groundBaselineY = Mathf.Max(minY, playerGroundY);
    }
}
