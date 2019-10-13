using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoogleStaticMap : MonoBehaviour
{
    public RawImage rawImage;
    [Range(0f, 1f)]
    public float transparency = 1f;
    public float mapCenterLatitude = 37.5665350f;
    public float mapCenterLongtitude = 126.9779690f;
    [Range(1, 20)]
    public int mapZoom = 14;
    public int mapWidth = 640;
    public int mapHeight = 640;
    public enum MapType
    {
        roadmap, satellite, terrain, hybrid,
    }
    public MapType mapType = MapType.roadmap;
    [Range(1, 4)]
    public int scale = 1;
    public float markerLatitude = 37.5665350f;
    public float markerLongtitude = 126.9779690f;
    public enum MarkerSize
    {
        tiny, mid, small,
    }
    public MarkerSize markerSize = MarkerSize.mid;
    public enum MarkerColor
    {
        black, brown, green, purple, yellow, blue, gray, orange, red, white,
    }
    public MarkerColor markerColor = MarkerColor.blue;
    public char label = 'C';
    public string apiKey;

    private string url;
    private Color rawImageColor = Color.white;

    IEnumerator Map()
    {
        rawImageColor.a = transparency;
        rawImage.color = rawImageColor;

        label = Char.ToUpper(label);

        url = "https://maps.googleapis.com/maps/api/staticmap"
            + "?center=" + mapCenterLatitude + "," + mapCenterLongtitude
            + "&zoom=" + mapZoom
            + "&size=" + mapWidth + "x" + mapHeight
            + "&scale=" + scale
            + "&maptype=" + mapType
            + "&markers=size:" + markerSize + "%7Ccolor:" + markerColor + "%7Clabel:" + label + "%7C" + markerLatitude + "," + markerLongtitude
            + "&key=" + apiKey;
        WWW www = new WWW(url);
        yield return www;
        rawImage.texture = www.texture;
        rawImage.SetNativeSize();
    }

    public void RefreshMap()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Map());
        }
    }

    private void Reset()
    {
        rawImage = gameObject.GetComponentInChildren<RawImage>();
        RefreshMap();
    }

    private void Start()
    {
        Screen.SetResolution(720, 1280, true);

        Reset();
        StartCoroutine("cor_GetGPS");

        zoomPlusBtn.onClick.AddListener(() =>
        {
            if (mapZoom < 19)
                mapZoom++;
            RefreshMap();
        });
        zoomMinusBtn.onClick.AddListener(() =>
        {
            if (mapZoom > 0)
                mapZoom--;
            RefreshMap();
        });
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshMap();
    }
#endif

    /// <summary>
    /// GPS
    /// </summary>
    LocationInfo currentGPSPosition;
    [Space(10f)]
    public Button zoomPlusBtn;
    public Button zoomMinusBtn;
    public Text text;
    IEnumerator cor_GetGPS()
    {
        // First, check if user has location service enabled
        //if (Input.location.isEnabledByUser == false)
        //{
        //    text.text = "Input.location.isEnabledByUser == false";
        //    yield break;
        //}

        // Start service before querying location
        Input.location.Start(0.5f);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            text.text = "Timed out";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            text.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            InvokeRepeating("UpdateGPSPotion", 0.5f, 1f);
        }

        // Stop service if there is no need to query location updates continuously
        //Input.location.Stop();
    }

    void UpdateGPSPotion()
    {
        text.text = "Good!";
        currentGPSPosition = Input.location.lastData;

        // 위도 => currentGPSPosition.latitude
        // 경도 => currentGPSPosition.longitude
        mapCenterLatitude = currentGPSPosition.latitude;
        mapCenterLongtitude = currentGPSPosition.longitude;
        markerLatitude = currentGPSPosition.latitude;
        markerLongtitude = currentGPSPosition.longitude;

        RefreshMap();

    }
}
