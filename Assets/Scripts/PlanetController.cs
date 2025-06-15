using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlanetController : MonoBehaviour
{
    public bool isOrbiting = true;
    public Transform orbitCenter;
    public float orbitSpeed = 30f;
    public bool isPlayerControlled = false;
    public float shootForce = 10f;
    public LineRenderer aimLine;

    private Rigidbody2D rb;
    private bool isLaunched = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    void Update()
    {
        if (isPlayerControlled && !isLaunched)
        {
            Aim();

            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
    }

    void FixedUpdate()
    {
        if (isOrbiting && !isLaunched)
        {
            OrbitAroundCenter();
        }
    }

    void OrbitAroundCenter()
    {
        if (orbitCenter != null)
        {
            transform.RotateAround(orbitCenter.position, Vector3.forward, orbitSpeed * Time.fixedDeltaTime);
        }
    }

    void Aim()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - transform.position).normalized;

        if (aimLine != null)
        {
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, (Vector2)transform.position + dir * 5f);
        }
    }

    void Shoot()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - transform.position).normalized;
        rb.isKinematic = false;
        rb.AddForce(dir * shootForce, ForceMode2D.Impulse);
        isLaunched = true;
        isPlayerControlled = false;
        if (aimLine != null) aimLine.enabled = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isLaunched)
        {
            PlanetController hitPlanet = collision.gameObject.GetComponent<PlanetController>();
            if (hitPlanet != null && !hitPlanet.isLaunched)
            {
                GameManager.Instance.SwitchControlTo(hitPlanet);
            }

            GameManager.Instance.DestroyPlanet(this);
        }
    }
}
