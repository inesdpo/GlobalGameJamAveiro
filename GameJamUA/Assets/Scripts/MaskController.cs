using UnityEngine;
using UnityEngine.UI;

public class MaskController : MonoBehaviour
{
    public Image maskOverlay;
    public KeyCode toggleKey = KeyCode.E;
    private bool maskOn = false;

    void Start()
    {
        if (maskOverlay != null)
            maskOverlay.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            maskOn = !maskOn;
            if (maskOverlay != null)
                maskOverlay.enabled = maskOn;
        }
    }
}
