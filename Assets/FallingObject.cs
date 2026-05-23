using UnityEngine;

// Extended from original FallingObject.
// Adds: multiple obstacle types, different speeds, random sizes.
// Zigzag uses Rigidbody2D.MovePosition so physics collisions still fire correctly.
// Spawner calls SetType() after Instantiate; Start() applies the settings.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class FallingObject : MonoBehaviour
{
    // Set by Spawner before Start() runs.
    [HideInInspector] public ObstacleType obstacleType = ObstacleType.Standard;

    const float BASE_SPEED   = 5f;
    const float DESTROY_Y    = -6f;
    const float ZIGZAG_AMP   = 2.5f;
    const float ZIGZAG_FREQ  = 2.0f;

    float      fallSpeed;
    float      startX;
    float      spawnTime;
    SpriteRenderer sr;
    Rigidbody2D    rb;
    GameManager    gm;

    // ── Per-type obstacle counters ────────────────────────────────────────
    static int countStandard = 0;
    static int countFast     = 0;
    static int countZigzag   = 0;
    static int countHeavy    = 0;
    static int countTiny     = 0;
    int        obstacleNumber;


    GUIStyle    numStyle;
    bool        numStyleReady = false;

    void Start()
    {
        sr        = GetComponent<SpriteRenderer>();
        rb        = GetComponent<Rigidbody2D>();
        gm        = FindAnyObjectByType<GameManager>();
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        startX    = transform.position.x;
        spawnTime = Time.time;

        // Assign per-type number to this obstacle
        switch (obstacleType)
        {
            case ObstacleType.Standard: obstacleNumber = ++countStandard; break;
            case ObstacleType.Fast:     obstacleNumber = ++countFast;     break;
            case ObstacleType.Zigzag:   obstacleNumber = ++countZigzag;   break;
            case ObstacleType.Heavy:    obstacleNumber = ++countHeavy;    break;
            case ObstacleType.Tiny:     obstacleNumber = ++countTiny;     break;
            default:                    obstacleNumber = 1;               break;
        }

        switch (obstacleType)
        {
            case ObstacleType.Standard:
                fallSpeed = BASE_SPEED;
                sr.color  = new Color(1f, 0.6f, 0f);  // bright orange
                break;
            case ObstacleType.Fast:
                fallSpeed             = BASE_SPEED * 2f;
                transform.localScale *= 0.6f;
                sr.color              = new Color(0.9f, 0.2f, 0f);  // dark red-orange
                break;
            case ObstacleType.Zigzag:
                fallSpeed = BASE_SPEED * 0.85f;
                sr.color  = new Color(0.2f, 0.8f, 1f);             // cyan
                break;
            case ObstacleType.Heavy:
                fallSpeed             = BASE_SPEED * 0.55f;
                transform.localScale *= 1.5f;
                sr.color              = new Color(0.5f, 0.1f, 0.9f); // purple
                break;
            case ObstacleType.Tiny:
                fallSpeed             = BASE_SPEED * 1.4f;
                transform.localScale *= 0.4f;
                sr.color              = new Color(1f, 0.9f, 0f);    // yellow
                break;
        }
    }

    public void SetType(ObstacleType type) { obstacleType = type; }

    void FixedUpdate()
    {
        // Use MovePosition so the physics engine keeps collision detection active.
        float newY = transform.position.y - fallSpeed * Time.fixedDeltaTime;
        float newX = transform.position.x;

        if (obstacleType == ObstacleType.Zigzag)
        {
            float elapsed = Time.time - spawnTime;
            newX = startX + Mathf.Sin(elapsed * ZIGZAG_FREQ * Mathf.PI * 2f) * ZIGZAG_AMP;
        }

        rb.MovePosition(new Vector2(newX, newY));

        if (transform.position.y < DESTROY_Y)
            Destroy(gameObject);
    }

    public static void ResetAllCounters()
    {
        countStandard = 0;
        countFast     = 0;
        countZigzag   = 0;
        countHeavy    = 0;
        countTiny     = 0;
    }

    void OnGUI()
    {
        if (!numStyleReady)
        {
            numStyle = new GUIStyle();
            numStyle.fontSize          = 13;
            numStyle.fontStyle         = FontStyle.Bold;
            numStyle.normal.textColor  = Color.black;
            numStyle.alignment         = TextAnchor.MiddleCenter;
            numStyleReady = true;
        }

        if (Camera.main == null) return;

        // Convert world position to screen GUI position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.z < 0) return; // behind camera

        float guiX = screenPos.x;
        float guiY = Screen.height - screenPos.y;

        // Type prefix for the label
        string prefix = obstacleType switch
        {
            ObstacleType.Standard => "W",
            ObstacleType.Fast     => "F",
            ObstacleType.Zigzag   => "Z",
            ObstacleType.Heavy    => "H",
            ObstacleType.Tiny     => "T",
            _                     => "?"
        };

        // Draw obstacle number centered on the object
        GUI.Label(new Rect(guiX - 25f, guiY - 12f, 50f, 24f),
                  prefix + "#" + obstacleNumber, numStyle);
    }
}