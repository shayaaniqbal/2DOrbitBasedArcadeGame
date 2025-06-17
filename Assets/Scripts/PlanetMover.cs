using DG.Tweening;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class PlanetMover : MonoBehaviour
{
    public DOTweenPath path; // Assign via Inspector or dynamically
    private Tween moveTween;
    private bool isShot = false;



    void Start()
    {
        // Start path movement manually
        moveTween = transform.DOPath(path.wps.ToArray(), path.duration, PathType.CatmullRom)
            .SetEase(path.easeType)
            .SetLoops(path.loops)
            .SetOptions(true) // Close path = true
            .SetAutoKill(false)
            .Pause(); // start paused

        moveTween.Play(); // Play manually when ready
    }

    void LateUpdate()
    {
        // Keep the sprite fully flat (2D orientation)
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }


}
