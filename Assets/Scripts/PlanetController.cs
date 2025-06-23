using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlanetController : MonoBehaviour
{
    public DOTweenPath pathTween; // Optional path (orbit)
    public bool isPlayerControlled = false;
    public float shootForce = 10f;
    public LineRenderer aimLine;

    private Rigidbody2D rb;
    public bool isLaunched = false;
    private Tween orbitTween;

    // 👇 New Fields for Sprite Change
    private SpriteRenderer spriteRenderer;
    public Sprite hitSprite;


    public Transform targetPoint; // 👈 Assign this in Inspector


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        StartOrbitPath();
    }

    void Update()
    {
        if (isPlayerControlled && !isLaunched)
        {
            Aim();

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 shootDirection = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
                Shoot(shootDirection);
            }
        }
    }

    void LateUpdate()
    {
        // Lock rotation so sprite doesn't tilt
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Aim()
    {
        if (targetPoint == null) return;

        Vector2 dir = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;

        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, (Vector2)transform.position + dir * 5f);
        }
    }

    void Shoot(Vector2 _)
    {
        // Kill orbit tween if active
        if (orbitTween != null && orbitTween.IsActive())
        {
            orbitTween.Kill();
            orbitTween = null;
        }

        if (aimLine != null && aimLine.positionCount >= 2)
        {
            Vector2 start = aimLine.GetPosition(0);
            Vector2 end = aimLine.GetPosition(1);
            Vector2 shootDir = (end - start).normalized;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = shootDir * shootForce;

            isLaunched = true;
            isPlayerControlled = false;
            aimLine.enabled = false;
        }
        else
        {
            Debug.LogWarning("Aim line not set up correctly.");
        }
    }

    void StartOrbitPath()
    {
        if (pathTween != null && !isLaunched)
        {
            orbitTween = transform.DOPath(pathTween.wps.ToArray(), pathTween.duration, PathType.CatmullRom)
           .SetEase(pathTween.easeType)
           .SetLoops(pathTween.loops)
           .SetOptions(true) // Close path = true
           .SetAutoKill(false)
           .Pause(); // start paused

            orbitTween.Play(); // Play manually when ready
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        spriteRenderer.sprite = hitSprite;

        // If it hits a death zone, trigger game over
        if (collider.gameObject.CompareTag("DeathZone"))
        {
            SoundManager.Instance.PlayHitSFX();
            Level.Instance.DestroyPlanet(this);
            UIManager.Instance.ShowGameOver();
            Debug.Log("Game Over - Planet hit the DeathZone");
            return;
        }

        // Normal planet-planet collision logic
        if (!isLaunched) return;

        PlanetController hitPlanet = collider.gameObject.GetComponent<PlanetController>();
        if (hitPlanet != null && hitPlanet != this && !hitPlanet.isLaunched)
        {
            Level.Instance.HandlePlanetHit(this, hitPlanet);
        }
    }

}
