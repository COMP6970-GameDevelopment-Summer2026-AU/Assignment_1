// Defines all obstacle variants used by FallingObject and Spawner.
public enum ObstacleType
{
    Standard, // white  - steady fall
    Fast,     // orange - 2x speed, 0.6x size
    Zigzag,   // cyan   - sine-wave horizontal drift
    Heavy,    // purple - 1.5x size, slow
    Tiny,     // yellow - 0.4x size, fast
}
