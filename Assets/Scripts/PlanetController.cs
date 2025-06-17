using UnityEngine;
using DG.Tweening; // Needed for tween control

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlanetController : MonoBehaviour
{
    public DOTweenPath pathTween; // Assign in Inspector
    public bool isPlayerControlled = false;
    public float shootForce = 10f;
    public LineRenderer aimLine;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Tween orbitTween;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Start DOTween path if assigned
        if (pathTween != null)
        {
            orbitTween = transform.DOPath(pathTween.wps.ToArray(), pathTween.duration, PathType.CatmullRom)
                .SetEase(pathTween.easeType)
                .SetLoops(pathTween.loops)
                .SetUpdate(UpdateType.Fixed)
                .SetTarget(gameObject);
        }
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
        // Kill path tween
        if (orbitTween != null && orbitTween.IsActive()) orbitTween.Kill();

        // Launch with velocity
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = direction.normalized * shootForce;

        // Update state
        isLaunched = true;
        isPlayerControlled = false;
        if (aimLine != null) aimLine.enabled = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched) return;

        PlanetController hitPlanet = collision.gameObject.GetComponent<PlanetController>();
        if (hitPlanet != null && hitPlanet != this && !hitPlanet.isLaunched)
        {
            GameManager.Instance.SwitchControlTo(hitPlanet);
            GameManager.Instance.DestroyPlanet(this);
        }
    }
}
