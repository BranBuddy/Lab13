/**
   Written by Brandon Wahl

   This script grabs weather data from OpenWeather API based on latitude and longitude coordinates. In the JSON file provided, the script grabs data from 
   a variety of fields including weather conditions, sunrise/sunset times, and city name. Based on this data, the script updates the skybox material to reflect
   current weather and time of day in the specified city.
**/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[Serializable]
public class WeatherCondition
{
   public string main; // Main weather condition
   public string description; // Description of the weather condition
}

[Serializable]
public class SysInfo
{
   public long sunrise; // Will gather sunrise time in UTC seconds since epoch
   public long sunset; // Will gather sunset time in UTC seconds since epoch
}

[Serializable]
public class WeatherResponse
{
   public WeatherCondition[] weather; // Will gather weather conditions
   public SysInfo sys; // Will gather iinformation about sunrise/sunset
   public string name; // city name
   public long dt; // As defined on OpenWeatherMap: Time of data calculation (seconds since epoch)
   public int timezone; // As defined on OpenWeatherMap: Shift in seconds from UTC
}

public class WeatherManager : MonoBehaviour {

   // UI Elements
   [SerializeField] private TMP_Text cityText;
   [SerializeField] private TMP_Text cityWeather;

   //City Coordinates inputable via inspector, however they will change depending on which city button is clicked
   [SerializeField] internal string longitude;
   [SerializeField] internal string latitude;

   private string defaultLongitude = "-82.7193";
   private string defaultLatitude = "28.2442";

   //City Time Conditions
   [SerializeField] private Material skyboxSunrise;
   [SerializeField] private Material skyboxNight;
   [SerializeField] private Material skyboxSunset;

   //Weather Conditions
   [SerializeField] private Material skyboxClear;
   [SerializeField] private Material skyboxClouds;
   [SerializeField] private Material skyboxRain;
   [SerializeField] private Material skyboxDrizzle;
   [SerializeField] private Material skyboxThunderstorm;
   [SerializeField] private Material skyboxSnow;
   [SerializeField] private Material skyboxDefault;

   // Name of city
   public string cityName;

   //sunset and sunrise time in UTC seconds since epoch
   public DateTime sunriseTime;
   public DateTime sunsetTime;

   //use bottom two int vars to get just the hour as a int
   private int sunriseHour;
   private int sunsetHour;

   //Final city time hour only
   public int cityTimeOnly;

   // Gets current time in city
   internal DateTime cityTime;

   internal string xmlApi = "";

   void Start()
   {

      //Sets default coordinates if none are set
      latitude = defaultLatitude;
      longitude = defaultLongitude;
      
      //Gets current time
      cityTime = DateTime.Now;
      cityTimeOnly = cityTime.Hour;

      Debug.Log(cityTimeOnly);

      RepeatRequest();
   }

   public void RepeatRequest()
   {
      xmlApi = $"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid=36535064a28f2734ddc49d533c626f2e";

      StartCoroutine(GetWeatherXML(OnXMLDataLoaded));

      Debug.Log(cityTimeOnly);
   }


   private IEnumerator CallAPI(string url, Action<string> callback)
   {


      using (UnityWebRequest request = UnityWebRequest.Get(url))
      {
         yield return request.SendWebRequest();
         if (request.result == UnityWebRequest.Result.ConnectionError)
         {
            Debug.LogError($"network problem: {request.error}");
         }
         else if (request.result == UnityWebRequest.Result.ProtocolError)
         {
            Debug.LogError($"response error: {request.responseCode}");
         }
         else
         {
            string jsonString = request.downloadHandler.text;
            callback(jsonString);

            Debug.Log("Success! JSON received.");
            try
            {
               //Parses data
               WeatherResponse data = JsonUtility.FromJson<WeatherResponse>(jsonString);
               if (data != null)
               {
                  //Gets city name
                  cityName = data.name;
                  cityText.text = cityName;

                  if (data.sys != null)
                  {
                     try
                     {
                           // OpenWeather returns unix epoch seconds, so to get local sunrise/sunset times
                           // we need to apply the timezone offset if available.
                        if (data.timezone != 0)
                        {
                           var tz = TimeSpan.FromSeconds(data.timezone);
                           sunriseTime = DateTimeOffset.FromUnixTimeSeconds(data.sys.sunrise).ToOffset(tz).DateTime;
                           sunsetTime  = DateTimeOffset.FromUnixTimeSeconds(data.sys.sunset).ToOffset(tz).DateTime;
                        }
                        else
                        {
                           // If there is no timezone info, fall back to local machine time
                           sunriseTime = DateTimeOffset.FromUnixTimeSeconds(data.sys.sunrise).ToLocalTime().DateTime;
                           sunsetTime  = DateTimeOffset.FromUnixTimeSeconds(data.sys.sunset).ToLocalTime().DateTime;
                        }

                        // Assigns time above into simple ints so we can compare hours
                        sunriseHour = sunriseTime.Hour;
                        sunsetHour  = sunsetTime.Hour;

                        Debug.Log($"Sunrise: {sunriseHour} | Sunset: {sunsetHour}");
                     }
                     catch (Exception ex)
                     {
                        //If there is any errors converting time, it throws a warning
                        Debug.LogWarning($"Failed converting sunrise/sunset: {ex.Message}");
                     }
                  }

                     // compute the city's local hour using dt (current timestamp) and timezone offset in seconds
                     try
                     {
                        if (data.timezone != 0)
                        {
                           DateTimeOffset cityOffset;
                           if (data.dt != 0)
                           {
                              cityOffset = DateTimeOffset.FromUnixTimeSeconds(data.dt).ToOffset(TimeSpan.FromSeconds(data.timezone));
                           }
                           else
                           {
                              // as a fail safe, use current UTC time and apply timezone offset
                              cityOffset = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromSeconds(data.timezone));
                           }


                           cityTimeOnly = cityOffset.Hour; // Sets cityTimeOnly to be the hour of the city's local time
                           Debug.Log($"City local hour: {cityTimeOnly}");
                        }
                     }
                     catch (Exception ex)
                     {
                        Debug.LogWarning($"Failed computing city local time: {ex.Message}");
                     }

                  if (data.weather != null && data.weather.Length > 0)
                  {
                     var first = data.weather[0];
                     cityWeather.text = first.main;

                     Debug.Log($"City: {data.name} - Weather Condition: {first.main} - {first.description}");
                     // set the skybox based on the main weather value
                     ApplySkybox(first.main);
                  }
                  else
                  {
                     Debug.Log("Weather array not present or empty in JSON.");
                  }
               }

            }
            catch (Exception ex)
            {
               //If the parsing fails, it throws an error
               Debug.LogError($"Failed parsing JSON with JsonUtility: {ex.Message}");
            }

         }
      }
   }
   public IEnumerator GetWeatherXML(Action<string> callback)
   {
      return CallAPI(xmlApi, callback);
   }

   public void OnXMLDataLoaded(string data)
   {
      Debug.Log(data);
   }

   // Apply a skybox material based on the OpenWeather "main" value (e.g. "Clear", "Clouds", "Rain")
   private void ApplySkybox(string main)
   {
      if (string.IsNullOrEmpty(main)) return;

      string key = main.Trim().ToLowerInvariant();
      Material chosen = null;

      // Checks if the desired hour is between the start and end of the night cycle. 
      // For example if sunset is 8pm and sunrise is 6am the next day, night would fall between 8pm-6am
      bool HourBetween(int hour, int startOfNight, int endOfNight)
      {
         if (startOfNight <= endOfNight)
            return hour >= startOfNight && hour < endOfNight;
         
         return hour >= startOfNight || hour < endOfNight;
      }

      // The keys below are based on OpenWeather "main" values that would override time-based skyboxes like sunrise/sunset
      if (key == "rain") chosen = skyboxRain;
      else if (key == "drizzle") chosen = skyboxDrizzle;
      else if (key == "thunderstorm") chosen = skyboxThunderstorm;
      else if (key == "snow") chosen = skyboxSnow;
      else if (key == "clear" || key == "clouds")
      {
         //Assigns new variable to the current city time
         int now = cityTimeOnly;

         // If sunrise/sunset not available, use local machine time
         bool haveSunTimes = sunriseHour != 0 || sunsetHour != 0;

         if (!haveSunTimes)
         {
            // if there is no data for sunrise/sunset, use system local hour
            now = DateTime.Now.Hour;
         }

         // Sunrise moment (exact hour), Sunset moment (exact hour)
         if (now == sunriseHour)
         {
            chosen = skyboxSunrise;
         }
         else if (now == sunsetHour)
         {
            chosen = skyboxSunset;
         }
         else if (HourBetween(now, sunriseHour, sunsetHour))
         {
            // Daytime
            chosen = (key == "clouds") ? skyboxClouds : skyboxClear;
         }
         else
         {
            // Night
            chosen = skyboxNight;
         }
      }
      else
      {
         chosen = skyboxDefault;
      }

      //Below will update the skybox if a material was chosen
      if (chosen != null)
      {
         RenderSettings.skybox = chosen;
#if UNITY_EDITOR
         UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
#endif
         DynamicGI.UpdateEnvironment();
      }
   }

}