using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUI : MonoBehaviour
{
    public GameObject crosshair;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI fpsText;
    public GameObject pausePanel;

    bool cursorVisiblity = false;
    float frameRate;
    float timer;

    void Start()
    {
        Cursor.visible = cursorVisiblity;
    }

    void Update()
    {
        crosshair.transform.position = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(cursorVisiblity == false)
            {
                cursorVisiblity = true;
                Cursor.visible = true;
                crosshair.SetActive(false);
                pausePanel.SetActive(true);

                DataContainer.Instance.isPlaying = false;
            }
            else
            {
                cursorVisiblity = false;
                Cursor.visible = false;
                crosshair.SetActive(true);
                pausePanel.SetActive(false);

                DataContainer.Instance.isPlaying = true;
            }
        }

        infoText.text = "Your HP: " + DataContainer.Instance.playerHealth;
        fpsText.text = "FPS: " + frameRate;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    public void SetCursor(bool value)
    {
        Cursor.visible = value;
        cursorVisiblity = value;
        crosshair.SetActive(!value);

        DataContainer.Instance.isPlaying = !value;
    }

    public void SetPlaying(bool value)
    {
        DataContainer.Instance.isPlaying = value;
    }
}
