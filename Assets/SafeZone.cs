using UnityEngine;

// SafeZone — gradually shrinking safe area feature.
// Finds Player automatically via tag — no manual wiring needed.
//
// Unity setup (one time only):
//   1. Hierarchy → right-click → 2D Object → Sprite → Square
//   2. Rename it SafeZone
//   3. Position Y = -2.5, Scale X = 19, Y = 0.3
//   4. SpriteRenderer color = green
//   5. Add Component → SafeZone
[RequireComponent(typeof(SpriteRenderer))]
public class SafeZone : MonoBehaviour
{
    [Header("Shrink settings")]
    public float startHalfWidth = 9.5f;
    public float minHalfWidth   = 1.5f;
    public float shrinkRate     = 0.15f;

    // Warning zone — how close to edge before flashing red
    const float WARNING_THRESHOLD = 1.5f;
    const float HIT_COOLDOWN      = 2.1f; // slightly longer than invincibility duration

    float          currentHalfWidth;
    SpriteRenderer sr;
    GameManager    gm;
    Transform      player;
    Color          originalColor;

    bool  outsideBoundary  = false;
    float hitCooldownTimer = 0f;
    float survivalTime     = 0f;
    GUIStyle barStyle;
    bool     barStyleReady = false;

    public float HalfWidth => currentHalfWidth;

    void Start()
    {
        currentHalfWidth = startHalfWidth;
        sr            = GetComponent<SpriteRenderer>();
        gm            = FindAnyObjectByType<GameManager>();
        originalColor = sr.color;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        else Debug.LogWarning("SafeZone: No GameObject tagged 'Player' found.");

        // Move SafeZone and Player 20% up from their current positions.
        // Camera orthographic size = 5, total height = 10 units, 20% = 2 units up.
        float offset = Camera.main.orthographicSize * 2f * 0.05f; // net 5% up (was 20%, now 15% down = 5% up)

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + offset,
            transform.position.z
        );

        if (player != null)
        {
            player.position = new Vector3(
                player.position.x,
                player.position.y + offset,
                player.position.z
            );
        }
    }

    void Update()
    {
        if (gm == null || gm.IsGameOver) return;

        // Shrink over time
        currentHalfWidth = Mathf.Max(minHalfWidth,
                                     currentHalfWidth - shrinkRate * Time.deltaTime);

        // Resize the green bar visually
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(currentHalfWidth * 2f, s.y, s.z);

        // Cooldown timer
        if (hitCooldownTimer > 0f)
            hitCooldownTimer -= Time.deltaTime;

        // Track survival time
        survivalTime += Time.deltaTime;

        if (player == null) return;

        float px       = player.position.x;
        float distEdge = currentHalfWidth - Mathf.Abs(px); // positive = inside, negative = outside

        // ── Warning: flash bar red when close to edge ──────────────────────
        if (distEdge < WARNING_THRESHOLD && distEdge >= 0f)
        {
            // Flash between red and original color
            float flash = Mathf.PingPong(Time.time * 6f, 1f);
            sr.color = Color.Lerp(Color.red, originalColor, flash);
        }
        else
        {
            sr.color = originalColor;
        }

        // ── Boundary hit: only trigger once per crossing ───────────────────
        bool isOutside = px < -currentHalfWidth || px > currentHalfWidth;

        if (isOutside && !outsideBoundary && hitCooldownTimer <= 0f)
        {
            outsideBoundary = true;
            hitCooldownTimer = HIT_COOLDOWN;
            gm.TakeHit();
        }
        else if (!isOutside)
        {
            outsideBoundary = false; // reset when back inside
        }
    }

    void OnGUI()
    {
        if (!barStyleReady)
        {
            barStyle = new GUIStyle();
            barStyle.fontSize          = 16;
            barStyle.fontStyle         = FontStyle.Bold;
            barStyle.normal.textColor  = Color.white;
            barStyle.alignment         = TextAnchor.MiddleCenter;
            barStyleReady = true;
        }

        // Convert the bar world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        float barScreenY  = Screen.height - screenPos.y;
        float barScreenW  = currentHalfWidth / 9.5f * Screen.width; // approximate screen width of bar
        float barScreenX  = (Screen.width - barScreenW) / 2f;

        // Survival message inside the bar
        int    secs    = Mathf.FloorToInt(survivalTime);
        float  pct     = Mathf.InverseLerp(9.5f, 1.5f, currentHalfWidth) * 100f;
        string msg     = "Survived: " + secs + "s  |  Safe Zone: " + Mathf.FloorToInt(100f - pct) + "% remaining";

        GUI.Label(new Rect(barScreenX, barScreenY - 12, barScreenW, 24), msg, barStyle);
    }
}