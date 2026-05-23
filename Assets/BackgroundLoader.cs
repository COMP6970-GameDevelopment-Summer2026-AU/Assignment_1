using UnityEngine;

// BackgroundLoader — automatically loads and displays the background image.
// No manual wiring needed. Just:
//   1. Create a folder: Assets/Resources/
//   2. Put your image in it named exactly: background.png
// That's it — this script does the rest.
public class BackgroundLoader : MonoBehaviour
{
    void Start()
    {
        // Load the image from Assets/Resources/background.png
        Texture2D tex = Resources.Load<Texture2D>("background");

        if (tex == null)
        {
            Debug.LogWarning("BackgroundLoader: 'background.png' not found in Assets/Resources/");
            return;
        }

        // Create a new GameObject for the background
        GameObject bg = new GameObject("Background");

        // Add SpriteRenderer
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();

        // Convert Texture2D to Sprite
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), // pivot center
            100f
        );

        sr.sprite         = sprite;
        sr.sortingOrder   = -10; // behind everything

        // Scale to fill the camera view
        Camera cam     = Camera.main;
        float camH     = cam.orthographicSize * 2f;
        float camW     = camH * cam.aspect;

        float spriteH  = sr.bounds.size.y;
        float spriteW  = sr.bounds.size.x;

        bg.transform.localScale = new Vector3(
            camW / spriteW,
            camH / spriteH,
            1f
        );

        // Center it
        bg.transform.position = new Vector3(0f, 0f, 1f);

        // Set camera background to black so no blue edges show
        cam.backgroundColor = Color.black;
    }
}
