using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<PlanetController> planets = new List<PlanetController>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (planets.Count > 0)
        {
            SwitchControlTo(planets[0]);
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

    public void DestroyPlanet(PlanetController planet)
    {
        if (planets.Contains(planet))
        {
            planets.Remove(planet);
            Destroy(planet.gameObject);
        }

        if (planets.Count == 0)
        {
            Debug.Log("Game Over!");
            // Add UI or restart here
        }
    }
}
