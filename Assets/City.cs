using System;
using UnityEngine;

public class City : MonoBehaviour
{
    [SerializeField] private string cityLongitude = null;
    [SerializeField] private string cityLatitude = null;

    [SerializeField] private WeatherManager weatherManager;

    void Awake()
    {

        if (weatherManager == null)
            weatherManager = GetComponent<WeatherManager>();

        if (weatherManager == null)
            Debug.LogWarning("City: WeatherManager not found. Assign it in the Inspector or place WeatherManager on the same GameObject/parent/scene.", this);
    }

    public void changeValueOnClick()
    {
        if (weatherManager == null)
        {
            Debug.LogError("City.changeValueOnClick: WeatherManager is null — cannot update coordinates.");
            return;
        }

        // Only overwrite if this city has values (avoid setting null unintentionally)
        if (!string.IsNullOrEmpty(this.cityLongitude))
            weatherManager.longitude = this.cityLongitude.Trim();
        if (!string.IsNullOrEmpty(this.cityLatitude))
            weatherManager.latitude = this.cityLatitude.Trim();

        weatherManager.RepeatRequest();

    }
}
