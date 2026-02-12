using UnityEngine;
using UnityEngine.UI;

public class CrucibleManager : MonoBehaviour
{
    public static CrucibleManager Instance;

    public Slider crucibleSlider;

    private int crucibleCount = 0;
    private int maxCrucibles = 5;

    void Awake()
    {
        Instance = this;
    }

    public void AddCrucible()
    {
        if (crucibleCount < maxCrucibles)
        {
            crucibleCount++;
            crucibleSlider.value = crucibleCount;
        }
    }
}

