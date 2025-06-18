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
    private bool isLaunched = false;
    private Tween orbitTween;

    // 👇 New Fields for Sprite Change
    private SpriteRenderer spriteRenderer;
    public Sprite hitSprite;

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
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - transform.position).normalized;

        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, (Vector2)transform.position + dir * 5f);
        }
    }

    void Shoot(Vector2 direction)
    {
        // Kill orbit tween if active
        if (orbitTween != null && orbitTween.IsActive())
        {
            orbitTween.Kill();
            orbitTween = null;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = direction.normalized * shootForce;

        isLaunched = true;
        isPlayerControlled = false;
        if (aimLine != null) aimLine.enabled = false;
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

    void OnCollisionEnter2D(Collision2D collision)
    {

        spriteRenderer.sprite = hitSprite;

        // If it hits a death zone, trigger game over
        if (collision.gameObject.CompareTag("DeathZone"))
        {
            Level.Instance.DestroyPlanet(this);
            UIManager.Instance.ShowGameOver();
            Debug.Log("Game Over - Planet hit the DeathZone");
            return;
        }

        // Normal planet-planet collision logic
        if (!isLaunched) return;

        PlanetController hitPlanet = collision.gameObject.GetComponent<PlanetController>();
        if (hitPlanet != null && hitPlanet != this && !hitPlanet.isLaunched)
        {
            
          
            Level.Instance.HandlePlanetHit(this, hitPlanet);

            
        }
    }
}
