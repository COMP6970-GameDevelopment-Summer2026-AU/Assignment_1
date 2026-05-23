using UnityEngine;
using UnityEngine.InputSystem;

// Extended from original PlayerMovement.
// Adds: extra life system with invincibility flash,
//       dark orange player color, Player/JA label,
//       safe zone boundary clamping.
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    // ── Original fields ────────────────────────────────────────────────────
    float horizontalInput = 0f;
    float moveSpeed       = 5f;
    float xLimit          = 9f;

    // ── New: invincibility flash ───────────────────────────────────────────
    bool  isInvincible      = false;
    float invincibleTimer   = 0f;
    const float INVINCIBLE_DURATION = 2f;
    const float FLASH_INTERVAL      = 0.1f;
    float flashTimer = 0f;

    SpriteRenderer sr;
    GameManager    gm;
    SafeZone       safeZone; // found automatically, null-safe

    void Start()
    {
        sr       = GetComponent<SpriteRenderer>();
        gm       = FindAnyObjectByType<GameManager>();
        safeZone = FindAnyObjectByType<SafeZone>();

        // Dark orange player color.
        sr.color = new Color(0.75f, 0.25f, 0.05f);
    }

    void Update()
    {
        // ── Original movement ──────────────────────────────────────────────
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            horizontalInput = 1f;
        else
            horizontalInput = 0f;

        transform.position += Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;

        // Clamp to safe zone if present, otherwise use fixed xLimit.
        float limit = (safeZone != null) ? safeZone.HalfWidth : xLimit;
        float clampedX = Mathf.Clamp(transform.position.x, -limit, limit);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // ── Invincibility flash ────────────────────────────────────────────
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            flashTimer      -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                sr.enabled = !sr.enabled;
                flashTimer = FLASH_INTERVAL;
            }
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                sr.enabled   = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Obstacle")) return;
        if (isInvincible) return;

        gm.TakeHit();

        // Grant invincibility if still alive.
        if (!gm.IsGameOver)
        {
            isInvincible    = true;
            invincibleTimer = INVINCIBLE_DURATION;
            flashTimer      = FLASH_INTERVAL;
        }
    }

    // ── Draw Player/JA label via OnGUI — no wiring needed ─────────────────
    GUIStyle labelStyle;
    bool styleReady = false;

    void OnGUI()
    {
        if (!styleReady)
        {
            labelStyle = new GUIStyle();
            labelStyle.fontSize          = 11;
            labelStyle.fontStyle         = FontStyle.Bold;
            labelStyle.normal.textColor  = Color.white;
            labelStyle.alignment         = TextAnchor.MiddleCenter;
            styleReady = true;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        float guiX = screenPos.x;
        float guiY = Screen.height - screenPos.y;

        // Player/JA label centered on player box
        GUI.Label(new Rect(guiX - 40f, guiY - 10f, 80f, 20f), "Player/JA", labelStyle);

        // Invincibility tooltip shown above the player
        if (isInvincible)
        {
            // Tooltip background
            GUIStyle tooltipBg = new GUIStyle();
            tooltipBg.fontSize          = 12;
            tooltipBg.fontStyle         = FontStyle.Bold;
            tooltipBg.normal.textColor  = Color.black;
            tooltipBg.alignment         = TextAnchor.MiddleCenter;

            GUIStyle tooltipStyle = new GUIStyle();
            tooltipStyle.fontSize         = 12;
            tooltipStyle.fontStyle        = FontStyle.Bold;
            tooltipStyle.normal.textColor = new Color(1f, 0.9f, 0f); // yellow
            tooltipStyle.alignment        = TextAnchor.MiddleCenter;

            float timeLeft = Mathf.Ceil(invincibleTimer);
            string line1   = "INVINCIBLE  (" + timeLeft + "s)";
            string line2   = "Obstacles pass through!";

            float boxW = 180f;
            float boxH = 42f;
            float boxX = guiX - boxW / 2f;
            float boxY = guiY - 70f;

            // Dark background box
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(boxX - 4, boxY - 2, boxW + 8, boxH + 4), Texture2D.whiteTexture);

            // Yellow border
            GUI.color = new Color(1f, 0.9f, 0f, 0.9f);
            GUI.DrawTexture(new Rect(boxX - 4, boxY - 2, boxW + 8, 2),       Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(boxX - 4, boxY + boxH + 2, boxW + 8, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(boxX, boxY,      boxW, 22), line1, tooltipStyle);
            GUI.Label(new Rect(boxX, boxY + 21, boxW, 20), line2, tooltipStyle);
        }
    }
}