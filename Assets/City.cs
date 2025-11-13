/**
    Written by Brandon Wahl
    
    This script will be applied to a button which represents a city. In inspector, insert the desired latitude and longitude coordinates.
    When the button is clicked, the script updates the WeatherManager's coordinates to those of the city.
**/

using System;
using UnityEngine;

public class City : MonoBehaviour
{
    //Input latitude and longitude for this city via Inspector
    [SerializeField] private string cityLongitude = null;
    [SerializeField] private string cityLatitude = null;

    [SerializeField] private WeatherManager weatherManager;

    //Basic debugging to ensure WeatherManager is assigned
    void Awake()
    {

        if (weatherManager == null)
            weatherManager = GetComponent<WeatherManager>();

        if (weatherManager == null)
            Debug.LogWarning("City: WeatherManager not found. Assign it in the Inspector or place WeatherManager on the same GameObject/parent/scene.", this);
    }

    public void changeValueOnClick()
    {
        //Debugging
        if (weatherManager == null)
        {
            Debug.LogError("City.changeValueOnClick: WeatherManager is null — cannot update coordinates.");
            return;
        }

        // If the string isnt null or empty, update the WeatherManager's coordinates
        if (!string.IsNullOrEmpty(this.cityLongitude))
            weatherManager.longitude = this.cityLongitude.Trim();
        if (!string.IsNullOrEmpty(this.cityLatitude))
            weatherManager.latitude = this.cityLatitude.Trim();

        //Request new weather data based on updated coordinates
        weatherManager.RepeatRequest();

    }
}
