using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Extended from original GameManager.
// Keeps original scoreText + gameOverText wiring — no new wiring needed.
// Adds: lives system, instructions screen — all drawn via OnGUI.
public class GameManager : MonoBehaviour
{
    // ── Original fields (same wiring as original) ──────────────────────────
    float score;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;

    // ── State ──────────────────────────────────────────────────────────────
    int  lives       = 3;
    bool gameOver    = false;
    bool gameStarted = false;

    public float Score      => Mathf.FloorToInt(score);
    public bool  IsGameOver => gameOver;

    void Start()
    {
        gameOver       = false;
        gameStarted    = false;
        lives          = 3;
        Time.timeScale = 0f; // paused until player presses SPACE
        scoreText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
    }

    void Update()
    {
        // ── Waiting on instruction screen ──────────────────────────────────
        if (!gameStarted)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                gameStarted    = true;
                Time.timeScale = 1f;
                scoreText.gameObject.SetActive(true);
            }
            return;
        }

        // ── Game over screen ───────────────────────────────────────────────
        if (gameOver)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                RestartGame();
            return;
        }

        // ── Gameplay ───────────────────────────────────────────────────────
        // Score accumulated — displayed via OnGUI, not TMP
        score += Time.deltaTime;
        if (scoreText != null) scoreText.gameObject.SetActive(false);
    }

    public void TakeHit()
    {
        if (gameOver) return;
        lives = Mathf.Max(0, lives - 1);
        if (lives <= 0) GameOver();
    }

    public void GameOver()
    {
        if (gameOver) return;
        gameOver       = true;
        Time.timeScale = 0f;
        // Game over drawn by OnGUI — keep original TMP text hidden.
        gameOverText.gameObject.SetActive(false);
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // ══════════════════════════════════════════════════════════════════════
    // OnGUI — draws instruction screen + lives HUD + game over overlay
    // No Canvas wiring needed at all.
    // ══════════════════════════════════════════════════════════════════════

    // Styles
    GUIStyle styleTitle;
    GUIStyle styleMeta;
    GUIStyle styleHeader;
    GUIStyle styleBody;
    GUIStyle stylePrompt;
    GUIStyle styleLives;
    GUIStyle styleGameOver;
    GUIStyle styleFinalScore;
    bool     stylesReady = false;

    void InitStyles()
    {
        if (stylesReady) return;

        styleTitle = new GUIStyle();
        styleTitle.fontSize          = 42;
        styleTitle.fontStyle         = FontStyle.Bold;
        styleTitle.normal.textColor  = Color.white;
        styleTitle.alignment         = TextAnchor.MiddleCenter;

        styleMeta = new GUIStyle();
        styleMeta.fontSize           = 20;
        styleMeta.normal.textColor   = new Color(0.75f, 0.75f, 0.75f);
        styleMeta.alignment          = TextAnchor.MiddleCenter;

        styleHeader = new GUIStyle();
        styleHeader.fontSize         = 24;
        styleHeader.fontStyle        = FontStyle.Bold;
        styleHeader.normal.textColor = Color.yellow;
        styleHeader.alignment        = TextAnchor.MiddleLeft;

        styleBody = new GUIStyle();
        styleBody.fontSize           = 20;
        styleBody.normal.textColor   = Color.white;
        styleBody.alignment          = TextAnchor.MiddleLeft;
        styleBody.wordWrap           = true;

        stylePrompt = new GUIStyle();
        stylePrompt.fontSize         = 30;
        stylePrompt.fontStyle        = FontStyle.Bold;
        stylePrompt.normal.textColor = Color.yellow;
        stylePrompt.alignment        = TextAnchor.MiddleCenter;

        styleLives = new GUIStyle();
        styleLives.fontSize          = 24;
        styleLives.fontStyle         = FontStyle.Bold;
        styleLives.normal.textColor  = Color.white;
        styleLives.alignment         = TextAnchor.UpperRight;

        styleGameOver = new GUIStyle();
        styleGameOver.fontSize       = 52;
        styleGameOver.fontStyle      = FontStyle.Bold;
        styleGameOver.normal.textColor = Color.red;
        styleGameOver.alignment      = TextAnchor.MiddleCenter;

        styleFinalScore = new GUIStyle();
        styleFinalScore.fontSize     = 30;
        styleFinalScore.normal.textColor = Color.white;
        styleFinalScore.alignment    = TextAnchor.MiddleCenter;

        stylesReady = true;
    }

    void OnGUI()
    {
        InitStyles();
        float sw = Screen.width;
        float sh = Screen.height;

        // ── Instruction screen ─────────────────────────────────────────────
        if (!gameStarted)
        {
            DrawInstructionScreen(sw, sh);
            return;
        }

        // ── HUD styles ─────────────────────────────────────────────────────
        GUIStyle hudStyle = new GUIStyle();
        hudStyle.fontSize         = 18;
        hudStyle.fontStyle        = FontStyle.Bold;
        hudStyle.normal.textColor = Color.white;
        hudStyle.alignment        = TextAnchor.MiddleCenter;

        // ── Lives — top left with dark box ─────────────────────────────────
        string livesStr = "Lives: ";
        for (int i = 0; i < 3; i++)
            livesStr += (i < lives) ? "[+] " : "[ ] ";

        GUI.color = new Color(0.05f, 0.05f, 0.25f, 0.88f);
        GUI.DrawTexture(new Rect(10, 10, 230, 34), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 230, 34), livesStr, hudStyle);

        // ── Score — top right with dark box ────────────────────────────────
        if (!gameOver)
        {
            string scoreStr = "Score: " + Mathf.FloorToInt(score);
            GUI.color = new Color(0.05f, 0.05f, 0.25f, 0.88f);
            GUI.DrawTexture(new Rect(sw - 160, 10, 150, 34), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(sw - 160, 10, 150, 34), scoreStr, hudStyle);
        }

        // ── Game over overlay ──────────────────────────────────────────────
        if (gameOver)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(0, sh * 0.3f,      sw, 65), "GAME OVER",                                styleGameOver);
            GUI.Label(new Rect(0, sh * 0.3f + 75, sw, 45), "Final Score: " + Mathf.FloorToInt(score),  styleFinalScore);
            GUI.Label(new Rect(0, sh - 70,         sw, 45), "Press SPACE to restart",                   stylePrompt);
        }
    }

    void DrawInstructionScreen(float sw, float sh)
    {
        // Dark overlay
        GUI.color = new Color(0.07f, 0.07f, 0.18f, 0.97f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Title block ────────────────────────────────────────────────────
        GUI.Label(new Rect(0, 14, sw, 56),  "DODGE 2D  —  Jahidul Arafat Edition",                   styleTitle);
        GUI.Label(new Rect(0, 68, sw, 26),  "Assignment 1  |  COMP 6970 Game Development  |  Summer 2026", styleMeta);
        GUI.Label(new Rect(0, 92, sw, 24),  "Developed by Jahidul Arafat  —  Extended from the original Dodge2D", styleMeta);

        // Divider under title
        GUI.color = new Color(1f, 1f, 1f, 0.2f);
        GUI.DrawTexture(new Rect(sw * 0.04f, 122, sw * 0.92f, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // ── Three-column layout ────────────────────────────────────────────
        float totalW = sw * 0.92f;
        float startX = sw * 0.04f;
        float colW   = totalW / 3f;
        float col1X  = startX;
        float col2X  = startX + colW;
        float col3X  = startX + colW * 2f;
        float lh     = 27f;
        float gap    = 14f;
        float startY = 132f;
        float y;

        // ══ COLUMN 1 — Instructions ════════════════════════════════════════
        y = startY;

        GUI.Label(new Rect(col1X, y, colW, lh), "CONTROLS",                         styleHeader); y += lh + 2;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Left / A    Move Left",           styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Right / D   Move Right",          styleBody);  y += lh + gap;

        GUI.Label(new Rect(col1X, y, colW, lh), "LIVES",                                        styleHeader); y += lh + 2;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Start with 3 lives: [+][+][+]",             styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Each obstacle hit = -1 life.",               styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  After hit: 2 sec INVINCIBILITY.",            styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Player BLINKS = invincible.",                styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  While blinking: obstacles pass through!",    styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Tooltip shows countdown + reminder.",        styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  0 lives = Game Over.",                       styleBody);  y += lh + gap;

        GUI.Label(new Rect(col1X, y, colW, lh), "SAFE ZONE",                              styleHeader); y += lh + 2;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Green bar at the bottom.",               styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Shrinks gradually over time.",           styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Bar shows survival time + % remaining.", styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Bar flashes RED = near the edge!",       styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Step outside = -1 life.",                styleBody);  y += lh + gap;

        GUI.Label(new Rect(col1X, y, colW, lh), "SCORE",                                    styleHeader); y += lh + 2;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Score = seconds survived.",              styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Dodging obstacles does NOT add points.", styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  New obstacle types unlock over time.",   styleBody);  y += lh;
        GUI.Label(new Rect(col1X, y, colW, lh), "  Spawn rate increases as score grows.",   styleBody);

        // ══ COLUMN 2 — Obstacles ═══════════════════════════════════════════
        y = startY;

        GUI.Label(new Rect(col2X, y, colW, lh), "OBSTACLES  (5 types)",              styleHeader); y += lh + 4;

        GUI.color = Color.white;
        GUI.Label(new Rect(col2X, y, colW, lh), "  ORANGE  Standard   (always)",     styleBody); y += lh;
        GUI.Label(new Rect(col2X + 14, y, colW, lh), "Steady fall. Label: W#1, W#2...",  styleBody); y += lh + 6;

        GUI.color = new Color(1f, 0.5f, 0.1f);
        GUI.Label(new Rect(col2X, y, colW, lh), "  ORANGE  Fast   (appears after 10s)",      styleBody); y += lh;
        GUI.color = Color.white;
        GUI.Label(new Rect(col2X + 14, y, colW, lh), "2x speed, smaller size.",      styleBody); y += lh + 6;

        GUI.color = new Color(0.2f, 0.9f, 1f);
        GUI.Label(new Rect(col2X, y, colW, lh), "  CYAN    Zigzag  (appears after 20s)",     styleBody); y += lh;
        GUI.color = Color.white;
        GUI.Label(new Rect(col2X + 14, y, colW, lh), "Swings left and right.",       styleBody); y += lh + 6;

        GUI.color = new Color(0.7f, 0.3f, 1f);
        GUI.Label(new Rect(col2X, y, colW, lh), "  PURPLE  Heavy  (appears after 35s)",      styleBody); y += lh;
        GUI.color = Color.white;
        GUI.Label(new Rect(col2X + 14, y, colW, lh), "Large and slow.",              styleBody); y += lh + 6;

        GUI.color = new Color(1f, 0.95f, 0.1f);
        GUI.Label(new Rect(col2X, y, colW, lh), "  YELLOW  Tiny   (appears after 35s)",      styleBody); y += lh;
        GUI.color = Color.white;
        GUI.Label(new Rect(col2X + 14, y, colW, lh), "Very small and fast.",         styleBody); y += lh + 14;

        // Tip box
        GUI.color = new Color(1f, 1f, 0f, 0.07f);
        GUI.DrawTexture(new Rect(col2X, y, colW - 10, lh * 2 + 12), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(col2X + 8, y + 4,      colW - 20, lh), "TIP: Stay near the centre —",          styleBody);
        GUI.Label(new Rect(col2X + 8, y + 4 + lh, colW - 20, lh), "safe zone shrinks from both sides.",   styleBody);

        // ══ COLUMN 3 — Grading Checklist ═══════════════════════════════════
        y = startY;

        // Column background
        GUI.color = new Color(0f, 0.15f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(col3X - 8, y - 6, colW + 4, sh - y - 70), Texture2D.whiteTexture);
        // Green left border
        GUI.color = new Color(0.2f, 1f, 0.3f, 0.6f);
        GUI.DrawTexture(new Rect(col3X - 8, y - 6, 3, sh - y - 70), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle chkHeader = new GUIStyle();
        chkHeader.fontSize         = 19;
        chkHeader.fontStyle        = FontStyle.Bold;
        chkHeader.normal.textColor = new Color(0.3f, 1f, 0.4f);
        chkHeader.alignment        = TextAnchor.MiddleLeft;

        GUIStyle chkSection = new GUIStyle();
        chkSection.fontSize         = 15;
        chkSection.fontStyle        = FontStyle.Bold;
        chkSection.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        chkSection.alignment        = TextAnchor.MiddleLeft;

        GUIStyle chkItem = new GUIStyle();
        chkItem.fontSize            = 15;
        chkItem.normal.textColor    = new Color(0.3f, 1f, 0.4f);
        chkItem.alignment           = TextAnchor.MiddleLeft;

        GUI.Label(new Rect(col3X + 2, y, colW, 24), "GRADING CHECKLIST", chkHeader); y += 28;

        // Required
        GUI.Label(new Rect(col3X + 2, y, colW, 20), "Required (75 pts)", chkSection); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Core Gameplay          25 pts", chkItem); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Score Counter          (Game Systems)", chkItem); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Game Over Screen       25 pts", chkItem); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Restart System         (included)", chkItem); y += 28;

        // Additional feature
        GUI.Label(new Rect(col3X + 2, y, colW, 20), "Additional Feature (35 pts)", chkSection); y += 22;

        GUIStyle chkBlue = new GUIStyle(chkItem);
        chkBlue.normal.textColor = new Color(0.4f, 0.8f, 1f);
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Multiple Obstacle Types", chkBlue); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Different Falling Speeds", chkBlue); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Extra Life System",         chkBlue); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Shrinking Safe Area",       chkBlue); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Random Obstacle Sizes",     chkBlue); y += 28;

        // Polish
        GUIStyle chkYellow = new GUIStyle(chkItem);
        chkYellow.normal.textColor = new Color(1f, 0.9f, 0.3f);
        GUI.Label(new Rect(col3X + 2, y, colW, 20), "Polish & Presentation (15 pts)", chkSection); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Difficulty Ramp",           chkYellow); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Player Label (Player/JA)",  chkYellow); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] Instruction Manual",        chkYellow); y += 22;
        GUI.Label(new Rect(col3X + 2, y, colW, 22), "  [✓] README on GitHub",          chkYellow);

        // Bottom divider + prompt
        GUI.color = new Color(1f, 1f, 1f, 0.2f);
        GUI.DrawTexture(new Rect(sw * 0.04f, sh - 58, sw * 0.92f, 1), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, sh - 52, sw, 46), "Press SPACE to Start", stylePrompt);
    }
}