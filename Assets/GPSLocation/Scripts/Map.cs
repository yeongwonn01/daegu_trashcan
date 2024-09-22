//MIT License
//Copyright (c) 2023 DA LAB (https://www.youtube.com/@DA-LAB)
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using TMPro;
using System.Globalization;
using System.Runtime.ExceptionServices;
using UnityEngine.EventSystems;
public class GPS_arr
{
    public float latitude;
    public float longitude;

    // 생성자
    public GPS_arr(float lat, float lon)
    {
        latitude = lat;
        longitude = lon;
    }
    public float lat()
    {
        return latitude;
    }
    public float lon()
    {
        return longitude;
    }
}


public class Map : MonoBehaviour
{
    public string apiKey;
    public float lat = -33.85660618894087f;
    public float lon = 151.21500701957325f;

    public float middle_lat;
    public float middle_lon;

    public int zoom = 12;
    public enum resolution { low = 1, high = 2 };
    public resolution mapResolution = resolution.high;
    public enum type { roadmap, satellite, gybrid, terrain };
    public type mapType = type.roadmap;
    private string url = "";
    private int mapWidth = 640;
    private int mapHeight = 640;
    private bool mapIsLoading = false; //not used. Can be used to know that the map is loading 
    private Rect rect;

    private string apiKeyLast;
    private float latLast = -33.85660618894087f;
    private float lonLast = 151.21500701957325f;
    private int zoomLast = 12;
    private resolution mapResolutionLast = resolution.low;
    private type mapTypeLast = type.roadmap;
    private bool updateMap = true;


    public TextMeshProUGUI lat_txt;
    public TextMeshProUGUI lon_txt;
    public TextMeshProUGUI zoom_txt;

    private string iconUrl = "https://i.imgur.com/vOx3xD7.png";
    private string player_iconUrl = "https://i.imgur.com/UyUyBv9.png";
    public GPS_arr[] locations;

    public TextAsset csvFile;
    string[] data;

    private Vector2 lastPointerPosition;
    private bool isDragging = false;
    private double mapMoveSensitivity = 0.000001;
    private float moveThreshold = 500f;
    public bool map_doesnt_move = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetGoogleMap());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
        data = csvFile.text.Split(new char[] { '\n' });  // 줄 단위로 데이터를 나누기
        locations = new GPS_arr[400];

        int i = 0;
        foreach (string line in data)
        {
            // 쉼표를 기준으로 각 열 데이터를 나누기
            string[] row = line.Split(new char[] { ',' });
            if (row[0] != "")
            {
                locations[i] = new GPS_arr(float.Parse(row[0]), float.Parse(row[1]));
                i++;
            }

        }
        middle_lat = lat;
        middle_lon = lon;
        
    }

    // Update is called once per frame
    void Update()
    {
        float parsedLat, parsedLon;
        int paresedZoom;

        if (!map_doesnt_move)
        {
            HandleTouchInput();
        }


        // 입력 문자열이 유효한지 확인 (빈 문자열 또는 잘못된 입력 처리)
        if (!string.IsNullOrEmpty(lat_txt.text) && !string.IsNullOrEmpty(lon_txt.text) &&!string.IsNullOrEmpty(zoom_txt.text))
        {
            // 공백 제거 후 입력값을 디버깅용으로 출력
            string latText = lat_txt.text.Trim();
            string lonText = lon_txt.text.Trim();
            string zoomText = zoom_txt.text.Trim();


            // 숫자로 변환 가능한지 확인
            if (float.TryParse(latText, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedLat) &&
                float.TryParse(lonText, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedLon) &&
                int.TryParse(zoomText, NumberStyles.Integer, CultureInfo.InvariantCulture, out paresedZoom))
            {
                lat = parsedLat;
                lon = parsedLon;
                zoom = paresedZoom;
                if (map_doesnt_move)
                {
                    middle_lat = lat;
                    middle_lon = lon;  
                }

                // 지도 업데이트 로직
                if (map_doesnt_move && updateMap && (apiKeyLast != apiKey || !Mathf.Approximately(latLast, lat) || !Mathf.Approximately(lonLast, lon) || zoomLast != zoom || mapResolutionLast != mapResolution || mapTypeLast != mapType))
                {
                    rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
                    mapWidth = (int)Math.Round(rect.width);
                    mapHeight = (int)Math.Round(rect.height);
                    StartCoroutine(GetGoogleMap());
                    updateMap = false;
                }
                else if(updateMap && !map_doesnt_move && zoomLast != zoom)
                {
                    rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
                    mapWidth = (int)Math.Round(rect.width);
                    mapHeight = (int)Math.Round(rect.height);
                    StartCoroutine(GetGoogleMap());
                    updateMap = false;
                }
            }
            else
            {
                Debug.LogError("Invalid input for latitude or longitude. Input is not a valid float number.");
            }
        }
        else
        {
            Debug.LogError("Latitude or longitude text is empty.");
        }
    }

    private void HandleTouchInput()
    {
        // 터치 입력 처리
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastPointerPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector2 touchDelta = touch.position - lastPointerPosition;

                // 터치 이동이 임계값 이상일 때만 지도 이동
                if (touchDelta.magnitude > moveThreshold)
                {
                    lastPointerPosition = touch.position;

                    double deltaLat = -touchDelta.y * mapMoveSensitivity * (20 / zoom) * (20 / zoom);
                    double deltaLon = -touchDelta.x * mapMoveSensitivity * (20 / zoom) * (20 / zoom);
                    middle_lat += (float)deltaLat;
                    middle_lon += (float)deltaLon;

                    StartCoroutine(GetGoogleMap());
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        // 마우스 입력 처리
        if (Input.GetMouseButtonDown(0))
        {
            lastPointerPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - lastPointerPosition;

            // 마우스 이동이 임계값 이상일 때만 지도 이동
            if (mouseDelta.magnitude > moveThreshold)
            {
                lastPointerPosition = Input.mousePosition;
                double deltaLat = -mouseDelta.y * mapMoveSensitivity * (20 / zoom);
                double deltaLon = -mouseDelta.x * mapMoveSensitivity * (20 / zoom);
                middle_lat += (float)deltaLat;
                middle_lon += (float)deltaLon;

                StartCoroutine(GetGoogleMap());
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
    public void get_started()
    {
        if (map_doesnt_move)
        {
            middle_lat = lat;
            middle_lon = lon;
        }
        StartCoroutine(GetGoogleMap());
    }

    IEnumerator GetGoogleMap()
    {
        string encodedIconUrl = Uri.EscapeDataString(iconUrl);
        string encodedPlayerIconUrl = Uri.EscapeDataString(player_iconUrl);

        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + middle_lat + "," + middle_lon + "&zoom=" + zoom + "&size=" + mapWidth + "x" + mapHeight + "&scale=" + mapResolution + "&maptype=" + mapType;
        if(locations.Length != 0)
           url += $"&markers=icon:{encodedIconUrl}";

        for (int i=0;i< locations.Length; i++)
        {
            if (locations[i] != null)
            {
                if (locations[i].lat() >= middle_lat - 0.01f && locations[i].lat() <= middle_lat + 0.01f && locations[i].lon() >= middle_lon - 0.01f && locations[i].lon() <= middle_lon + 0.01f)
                {
                    url += $"|{locations[i].lat()},{locations[i].lon()}";
                }
            }
        }
        url += $"&markers=icon:{encodedPlayerIconUrl}|{lat},{lon}"+"&key=" + apiKey;
        Debug.LogError(url);

        mapIsLoading = true;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("WWW ERROR: " + www.error);
        }
        else
        {
            mapIsLoading = false;
            gameObject.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            apiKeyLast = apiKey;
            latLast = lat;
            lonLast = lon;   
            zoomLast = zoom;
            mapResolutionLast = mapResolution;
            mapTypeLast = mapType;
            updateMap = true;
        }
    }

}