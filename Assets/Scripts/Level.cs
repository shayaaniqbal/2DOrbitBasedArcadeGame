using UnityEngine;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public static Level Instance;

    [Header("Game Mode")]
    public bool isFreeForAll ; // Toggle in Inspector

    [Header("Planet Settings")]
    public List<PlanetController> planets = new List<PlanetController>();


    private int currentIndex = 0; // For ordered mode
    private List<PlanetController> originalOrder = new List<PlanetController>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        

        originalOrder = new List<PlanetController>(planets);

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

    public void HandlePlanetHit(PlanetController shooter, PlanetController target)
    {
        SoundManager.Instance.PlayHitSFX();

        if (isFreeForAll)
        {
            if (IsFinalPairFreeForAll(shooter, target))
            {
                DestroyPlanet(shooter);
                DestroyPlanet(target);
                UIManager.Instance.ShowWin();
                return;
            }

            // Allow hitting ANY other unlaunched planet (no order needed)
            if (target != shooter && !target.isLaunched && planets.Contains(target))
            {
                DestroyPlanet(shooter);
                SwitchControlTo(target);

                if (planets.Count == 1)
                {
                    UIManager.Instance.ShowWin();
                }
            }
            else
            {
                Debug.Log("Game Over - Invalid target in FreeForAll");
                UIManager.Instance.ShowGameOver();
                DestroyPlanet(shooter);
            }
        }
        else // Ordered Mode
        {
            int shooterIndex = originalOrder.IndexOf(shooter);
            int targetIndex = originalOrder.IndexOf(target);

            if (targetIndex == currentIndex + 1)
            {
                bool isFinalPair = IsFinalPairForArrangement(shooter, target);

                currentIndex = targetIndex;
                DestroyPlanet(shooter);

                if (isFinalPair)
                {
                    DestroyPlanet(target);
                    UIManager.Instance.ShowWin();
                    return;
                }

                SwitchControlTo(target);
            }
            else
            {
                Debug.Log("Game Over - Wrong target hit");
                DestroyPlanet(shooter);
                DestroyPlanet(target);
                UIManager.Instance.ShowGameOver();
            }
        }
    }



    private bool IsFinalPairForArrangement(PlanetController shooter, PlanetController target)
    {
        if (planets.Count != 2) return false;

        var lastTwo = originalOrder.GetRange(originalOrder.Count - 2, 2);

        return (lastTwo[0] == shooter && lastTwo[1] == target);
    }

    private bool IsFinalPairFreeForAll(PlanetController a, PlanetController b)
    {
        // Just check if only these two are left
        return planets.Count == 2 && planets.Contains(a) && planets.Contains(b);
    }


    public void DestroyPlanet(PlanetController planet)
    {
        if (planets.Contains(planet))
        {
            planets.Remove(planet);
            Destroy(planet.gameObject);
        }

       
    }

   
}
