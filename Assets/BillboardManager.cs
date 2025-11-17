/*
    Written by: Brandon Wahl

    This script will downloaded an image from a provided URL and display it on a billboard in the scene.
    After downloading, the image will be cached to avoid redundant network requests.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class BillboardManager : MonoBehaviour
{
    //Default image URL in case none is provided
    private const string defaultWebImage = "https://images.rawpixel.com/image_800/cHJpdmF0ZS9sci9pbWFnZXMvd2Vic2l0ZS8yMDIyLTA1L2ZsNDgyNDQxMTEwOS1pbWFnZS1rcHFrNXh0eC5qcGc.jpg";
    [SerializeField] private string webImage;

    //Once the texture is downloaded, it will be cached here
    private Texture2D cachedTexture = null;

    //Reference to the plane where the billboard image will be displayed
    [SerializeField] private GameObject billboardPlane;

    void Start()
    {
        //Debugging checks
        if(billboardPlane == null)
        {
            Debug.LogError("BillboardManager: Billboard Plane is not assigned in the Inspector.");
            return;
        }

        if(string.IsNullOrEmpty(webImage))
        {
            Debug.LogWarning("Setting webImage to default URL as it was not assigned in the Inspector.");
            webImage = defaultWebImage;
        }

        StartCoroutine(GetWebImage(OnImageDownloaded));
    }

    // Downloads the image from the url provided and caches it
    public IEnumerator DownloadImage(Action<Texture2D> callback) { 

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(webImage);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            cachedTexture = DownloadHandlerTexture.GetContent(request);
            callback(cachedTexture);
        }
        else
        {
            Debug.LogError($"Failed to download image: {request.error}");
        }
    }

    // Retrieves the image, using the cached version if available. If it isn't it'll run DownloadImage
    public IEnumerator GetWebImage(Action<Texture2D> callback)
    {
        if (cachedTexture != null)
        {
            Debug.Log("Using cached image");
            callback(cachedTexture);
            yield break;
        }

        yield return DownloadImage(callback);
    }

    // Sets the downloaded texture to the billboard plane's material
    public void OnImageDownloaded(Texture2D texture)
    {
        billboardPlane.GetComponent<Renderer>().material.mainTexture = texture;
    }

}
