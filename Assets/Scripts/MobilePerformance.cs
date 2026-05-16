using UnityEngine;

public class MobilePerformance : MonoBehaviour
{
    void Awake()
    {

        QualitySettings.vSyncCount = 1;

        Application.targetFrameRate = 60;

    }
}