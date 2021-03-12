using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour
{
    DataContainer data;

    [Header("Texture generator")]
    public TextureGenerator textureGenerator;

    [Header("Noise octaves")]
    public int octavesValue;
    public TextMeshProUGUI octavesText;
    public Button octavesButtonPlus;
    public Button octavesButtonMinus;
    public int octavesMax = 4;
    public int octavesMin = 1;

    [Header("Noise scale")]
    public float scaleValue;
    public TextMeshProUGUI scaleText;
    public Button scaleButtonPlus;
    public Button scaleButtonMinus;
    public float scaleMax = 100f;
    public float scaleMin = 10f;

    [Header("Noise persistance")]
    public float persistanceValue;
    public TextMeshProUGUI persistanceText;
    public Button persistanceButtonPlus;
    public Button persistanceButtonMinus;
    public float persistanceMax = 0.7f;
    public float persistanceMin = 0.3f;

    [Header("Noise lacunarity")]
    public float lacunarityValue;
    public TextMeshProUGUI lacunarityText;
    public Button lacunarityButtonPlus;
    public Button lacunarityButtonMinus;
    public float lacunarityMax = 3f;
    public float lacunarityMin = 1f;

    [Header("Offsets")]
    public TextMeshProUGUI offsetText;

    [Header("Dropdowns")]
    public TMP_Dropdown surfaceDropdown;
    public TMP_Dropdown textureDropdown;
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        data = DataContainer.Instance;
        data.isPlaying = true;
        SetValues();
    }

    void SetValues()
    {
        octavesValue = data.noiseOctaves;
        scaleValue = data.noiseScale;
        persistanceValue = data.noisePersistance;
        lacunarityValue = data.noiseLacunarity;
    }

    public void SetText()
    {
        octavesText.text = octavesValue.ToString();
        scaleText.text = scaleValue.ToString("F0");
        persistanceText.text = persistanceValue.ToString("F2");
        lacunarityText.text = lacunarityValue.ToString("F2");

        string text = "";
        foreach (int offset in data.noiseOffsets)
        {
            text += offset.ToString() + "\n";
        }
        offsetText.text = text;

        textureDropdown.value = data.selectedTexture;
        surfaceDropdown.value = data.selectedSurface;
        difficultyDropdown.value = data.difficulty;
    }

    public void SetOctaves(int value)
    {
        octavesValue += value;

        if(octavesValue >= octavesMax)
        {
            octavesValue = octavesMax;
            octavesButtonPlus.interactable = false;
        }
        else if(octavesValue <= octavesMin)
        {
            octavesValue = octavesMin;
            octavesButtonMinus.interactable = false;
        }
        else
        {
            octavesButtonPlus.interactable = true;
            octavesButtonMinus.interactable = true;
        }

        octavesText.text = octavesValue.ToString();
        data.noiseOctaves = octavesValue;
        textureGenerator.UpdateNoiseMap();
    }

    public void SetScale(float value)
    {
        scaleValue += value;

        if (scaleValue >= scaleMax)
        {
            scaleValue = scaleMax;
            scaleButtonPlus.interactable = false;
        }
        else if (scaleValue <= scaleMin)
        {
            scaleValue = scaleMin;
            scaleButtonMinus.interactable = false;
        }
        else
        {
            scaleButtonPlus.interactable = true;
            scaleButtonMinus.interactable = true;
        }

        scaleText.text = scaleValue.ToString("F0");
        data.noiseScale = scaleValue;
        textureGenerator.UpdateNoiseMap();
    }

    public void SetPersistance(float value)
    {
        persistanceValue += value;

        if (persistanceValue >= persistanceMax)
        {
            persistanceValue = persistanceMax;
            persistanceButtonPlus.interactable = false;
        }
        else if (persistanceValue <= persistanceMin)
        {
            persistanceValue = persistanceMin;
            persistanceButtonPlus.interactable = false;
        }
        else
        {
            scaleButtonPlus.interactable = true;
            persistanceButtonMinus.interactable = true;
        }

        persistanceText.text = persistanceValue.ToString("F2");
        data.noisePersistance = persistanceValue;
        textureGenerator.UpdateNoiseMap();
    }

    public void SetLacunarity(float value)
    {
        lacunarityValue += value;

        if (lacunarityValue >= lacunarityMax)
        {
            lacunarityValue = lacunarityMax;
            lacunarityButtonPlus.interactable = false;
        }
        else if (lacunarityValue <= lacunarityMin)
        {
            lacunarityValue = lacunarityMin;
            lacunarityButtonMinus.interactable = false;
        }
        else
        {
            lacunarityButtonPlus.interactable = true;
            lacunarityButtonMinus.interactable = true;
        }

        lacunarityText.text = lacunarityValue.ToString("F2");
        data.noiseLacunarity = lacunarityValue;
        textureGenerator.UpdateNoiseMap();
    }

    public void GenerateOffsets()
    {
        textureGenerator.GenerateOffsets();

        string text = "";
        foreach (int offset in data.noiseOffsets)
        {
            text += offset.ToString() + "\n";
        }
        offsetText.text = text;
        textureGenerator.UpdateNoiseMap();
    }

    public void SetSurface(int value)
    {
        switch (value)
        {
            case 0:
                data.selectedSurface = 0;
                textureGenerator.UpdateTexture();
                break;
            case 1:
                data.selectedSurface = 1;
                textureGenerator.UpdateTexture();
                break;
            case 2:
                data.selectedSurface = 2;
                textureGenerator.UpdateTexture();
                break;
        }
    }

    public void SetTexture(int value)
    {
        switch (value)
        {
            case 0:
                data.selectedTexture = 0;
                textureGenerator.UpdateTexture();
                break;
            case 1:
                data.selectedTexture = 1;
                textureGenerator.UpdateTexture();
                break;
            case 2:
                data.selectedTexture = 2;
                textureGenerator.UpdateTexture();
                break;
            case 3:
                data.selectedTexture = 3;
                textureGenerator.UpdateTexture();
                break;
        }
    }

    public void SetDifficulty(int value)
    {
        switch (value)
        {
            case 0:
                data.difficulty = 0;
                break;
            case 1:
                data.difficulty = 1;
                break;
            case 2:
                data.difficulty = 2;
                break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
