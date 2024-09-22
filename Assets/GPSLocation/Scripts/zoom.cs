using System.Globalization;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonNameHandler : MonoBehaviour
{
    // 여러 버튼을 판별하기 위한 배열
    public Button[] buttons;

    public TextMeshProUGUI zoom_txt;

    public Map map;

    void Start()
    {
        // 각 버튼에 클릭 이벤트를 연결
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnButtonClick(btn));
        }
    }

    // 버튼이 클릭될 때 호출되는 함수
    void OnButtonClick(Button clickedButton)
    {
        int paresedZoom;

        if (clickedButton.gameObject.name == "current_loc")
        {
            if (map.map_doesnt_move == true)
            {
                map.map_doesnt_move = false;
            }
            else
            {
                map.map_doesnt_move = true;
                map.get_started();
            }

        }

        else if (!string.IsNullOrEmpty(zoom_txt.text))
        {
            // 공백 제거 후 입력값을 디버깅용으로 출력
            string zoomText = zoom_txt.text.Trim();


            // 숫자로 변환 가능한지 확인
            if (int.TryParse(zoomText, NumberStyles.Integer, CultureInfo.InvariantCulture, out paresedZoom))
            {

                if (clickedButton.gameObject.name == "zoomin")
                {
                    if(paresedZoom < 21)
                        paresedZoom += 1;
                    zoom_txt.text = paresedZoom.ToString();
                }
                else if (clickedButton.gameObject.name == "zoomout")
                {
                    if (paresedZoom > 15)
                        paresedZoom -= 1;
                    zoom_txt.text = paresedZoom.ToString();
                }

                else
                {
                    Debug.Log("알 수 없는 버튼 클릭됨");
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
}
