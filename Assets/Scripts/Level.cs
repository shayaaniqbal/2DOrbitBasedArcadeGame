using UnityEngine;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public static Level Instance;

    [Header("Game Mode")]
    public bool isFreeForAll = true; // Toggle in Inspector

    [Header("Planet Settings")]
    public List<PlanetController> planets = new List<PlanetController>();

    private int currentIndex = 0; // For ordered mode

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (planets.Count > 0)
        {
            currentIndex = 0;
            SwitchControlTo(planets[currentIndex]);
        }
    }

    public void SwitchControlTo(PlanetController newPlanet)
    {
        foreach (PlanetController planet in planets)
        {
            planet.isPlayerControlled = false;
            if (planet.aimLine != null) planet.aimLine.enabled = false;
        }

        newPlanet.isPlayerControlled = true;
        if (newPlanet.aimLine != null) newPlanet.aimLine.enabled = true;
    }

    public void HandlePlanetHit(PlanetController shooter, PlanetController target)
    {
        if (isFreeForAll)
        {
            // Free for all mode: allow any hit
            DestroyPlanet(shooter);
            SwitchControlTo(target);

            if (planets.Count == 1)
            {
                Debug.Log("Game Won!");
                // Optional: handle win UI
            }
        }
        else
        {
            // Ordered Mode: target must be next in list
            int targetIndex = planets.IndexOf(target);
            if (targetIndex == currentIndex + 1)
            {
                DestroyPlanet(shooter);
                currentIndex = targetIndex;
                SwitchControlTo(target);
            }
            else
            {
                Debug.Log("Game Over - Wrong target hit");
                DestroyPlanet(shooter);
                TriggerGameOver();
            }
        }
    }

    public void DestroyPlanet(PlanetController planet)
    {
        if (planets.Contains(planet))
        {
            planets.Remove(planet);
            Destroy(planet.gameObject);
        }

        if (planets.Count == 0)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        Debug.Log("Game Over!");
        // TODO: Add game over UI, restart, etc.
    }
}
