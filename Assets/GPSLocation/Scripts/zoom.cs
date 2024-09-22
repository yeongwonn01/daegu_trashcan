using System.Globalization;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonNameHandler : MonoBehaviour
{
    // ���� ��ư�� �Ǻ��ϱ� ���� �迭
    public Button[] buttons;

    public TextMeshProUGUI zoom_txt;

    public Map map;

    void Start()
    {
        // �� ��ư�� Ŭ�� �̺�Ʈ�� ����
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => OnButtonClick(btn));
        }
    }

    // ��ư�� Ŭ���� �� ȣ��Ǵ� �Լ�
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
            // ���� ���� �� �Է°��� ���������� ���
            string zoomText = zoom_txt.text.Trim();


            // ���ڷ� ��ȯ �������� Ȯ��
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
                    Debug.Log("�� �� ���� ��ư Ŭ����");
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
