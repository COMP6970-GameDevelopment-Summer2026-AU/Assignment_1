using UnityEngine;

// Extended from original Spawner.
// Adds: difficulty ramp (interval shrinks with score),
//       weighted obstacle type selection.
public class Spawner : MonoBehaviour
{
    // ── Original field (same wiring) ──────────────────────────────────────
    public GameObject fallingObjectPrefab;

    // ── Timing ────────────────────────────────────────────────────────────
    float spawnInterval = 1f;   // matches original starting value
    float timer         = 0f;
    const float MIN_INTERVAL = 0.25f;
    const float DIFFICULTY   = 0.012f;

    GameManager gm;

    void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
        // Reset all obstacle counters every time the scene loads
        FallingObject.ResetAllCounters();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Shrink interval as score grows (difficulty ramp).
        spawnInterval = Mathf.Max(MIN_INTERVAL, 1f - gm.Score * DIFFICULTY);

        if (timer >= spawnInterval)
        {
            SpawnFallingObject();
            timer = 0f;
        }
    }

    void SpawnFallingObject()
    {
        float xPos     = Random.Range(-8f, 8f);
        Vector3 spawnPos = new Vector3(xPos, transform.position.y, 0f);
        GameObject obj   = Instantiate(fallingObjectPrefab, spawnPos, Quaternion.identity);

        FallingObject fo = obj.GetComponent<FallingObject>();
        if (fo != null) fo.SetType(PickType());
    }

    // Weighted pool — harder types unlock as score grows.
    ObstacleType PickType()
    {
        float s = gm.Score;
        var pool = new System.Collections.Generic.List<(ObstacleType t, float w)>
        {
            (ObstacleType.Standard, 1.0f)
        };
        if (s >= 10f) pool.Add((ObstacleType.Fast,   Mathf.Lerp(0f, 1.2f, (s-10f)/40f)));
        if (s >= 20f) pool.Add((ObstacleType.Zigzag, Mathf.Lerp(0f, 0.9f, (s-20f)/30f)));
        if (s >= 35f) pool.Add((ObstacleType.Heavy,  Mathf.Lerp(0f, 0.7f, (s-35f)/25f)));
        if (s >= 35f) pool.Add((ObstacleType.Tiny,   Mathf.Lerp(0f, 0.8f, (s-35f)/25f)));

        float total = 0f; foreach (var e in pool) total += e.w;
        float roll  = Random.Range(0f, total);
        float cum   = 0f;
        foreach (var e in pool) { cum += e.w; if (roll <= cum) return e.t; }
        return ObstacleType.Standard;
    }
}