using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureGenerator : MonoBehaviour
{
    DataContainer data;

    float[,] noiseMap;
    float[,] gradientMap;
    float[,] coloredMap;

    public GameObject menuTexture;

    void Start()
    {
        data = DataContainer.Instance;
        noiseMap = new float[data.textureSize, data.textureSize];
        gradientMap = new float[data.textureSize, data.textureSize];
        coloredMap = new float[data.textureSize, data.textureSize];

        GenerateOffsets();
        GenerateNoiseMap();
        GenerateGradientMap();
        GenerateColoredMap();
        UpdateTexture();
    }

    //generate random offsets
    public void GenerateOffsets()
    {
        for(int i = 0; i < 4; i++)
        {
            data.noiseOffsets[i] = Random.Range(-10000, 10000);
        }
    }

    //generate noise map
    void GenerateNoiseMap()
    {
        noiseMap = Noise.GenerateNoiseMap(data.textureSize, data.noiseOctaves, data.noiseScale, data.noisePersistance, data.noiseLacunarity, data.noiseOffsets);
    }

    //Generate gradient map
    void GenerateGradientMap()
    {
        gradientMap = Gradient.GenerateGradientMap(data.textureSize, data.gradientCurve);
    }

    //Generate the colored map
    void GenerateColoredMap()
    {
        for(int x = 0; x < data.textureSize; x++)
        {
            for(int z = 0; z < data.textureSize; z++)
            {
                if(data.useCurve)
                {
                    coloredMap[x, z] = data.heightCurve.Evaluate(Mathf.Clamp01(noiseMap[x, z] - gradientMap[x, z]));
                }
                else
                {
                    coloredMap[x, z] = Mathf.Clamp01(noiseMap[x, z] - gradientMap[x, z]);
                }
            }
        }
    }

    //Generate texture for UI
    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(data.textureSize, data.textureSize);

        for (int x = 0; x < data.textureSize; x++)
        {
            for(int z = 0; z < data.textureSize; z++)
            {
                Color color = GetColor(x, z);
                texture.SetPixel(x, z, color);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        return texture;
    }

    //Get the color by selected texture
    Color GetColor(int x, int z)
    {
        Color color = new Color();

        switch(data.selectedTexture)
        {
            case 0:

                if (coloredMap[x, z] == 0)
                {
                    color = new Color(0.06275f, 0.16078f, 0.32157f);
                }
                else if (coloredMap[x, z] < data.waterHeight)
                {
                    color = new Color(0.11373f, 0.29804f, 0.56471f);
                }
                else
                {
                    if(data.surfaces[data.selectedSurface].surfaceVoxels.Length == 0)
                    {
                        color = Color.white;
                    }
                    else
                    {
                        for (int i = 0; i < data.surfaces[data.selectedSurface].surfaceVoxels.Length; i++)
                        {
                            if (coloredMap[x, z] <= data.surfaces[data.selectedSurface].surfaceVoxels[i].height)
                            {
                                color = data.surfaces[data.selectedSurface].surfaceVoxels[i].color;
                                break;
                            }
                        }
                        
                    }
                }

                break;

            case 1:

                color = new Color(noiseMap[x, z], noiseMap[x, z], noiseMap[x, z]);
                break;

            case 2:

                color = new Color(gradientMap[x, z], gradientMap[x, z], gradientMap[x, z]);
                break;

            case 3:

                color = new Color(coloredMap[x, z], coloredMap[x, z], coloredMap[x, z]);
                break;
        }

        return color;
    }

    //Update the texture
    public void UpdateTexture()
    {
        Texture2D texture = GenerateTexture();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, data.textureSize, data.textureSize), Vector2.zero, 100f);
        menuTexture.GetComponent<Image>().sprite = sprite;
    }

    //Update the noise map
    public void UpdateNoiseMap()
    {
        GenerateNoiseMap();
        GenerateColoredMap();
        UpdateTexture();
    }

    //Update gradient map
    public void UpdateGradientMap()
    {
        GenerateGradientMap();
        GenerateColoredMap();
        UpdateTexture();
    }
}
