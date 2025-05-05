using UnityEngine;

public class FixedAspectRatio : MonoBehaviour
{
    // Set your desired aspect ratio (e.g., 16:9)
    public float targetAspect = 16f / 9f;
    public GameObject background;


    void Start()
    {
        Camera camera = GetComponent<Camera>();


        // Determine current window aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;

        

        // Calculate scale height
        float scaleHeight = windowAspect / targetAspect;

        // Apply letterbox/pillarbox
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }

        background.transform.localScale = new Vector3(Screen.width, Screen.height, 1f);
    }
}