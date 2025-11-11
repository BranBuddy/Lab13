using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class myData
{
   public string main = "";

}




public class WeatherManager : MonoBehaviour {

   [SerializeField] private string longitude;
   [SerializeField] private string latitude;

   internal string xmlApi = "";

   void Start()
      {
      xmlApi = $"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid=8e2f99a8a79594b1f7428a7310771697";

      StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
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
            callback(request.downloadHandler.text);
            string jsonString = request.downloadHandler.text;
            Debug.Log("Success!");
            myData data = JsonUtility.FromJson<myData>(jsonString);
            Debug.Log($"Weather Condition:{data.main}");

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

}