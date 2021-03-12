using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int size, int octaves, float scale, float persistance, float lacunarity, int[] offsets)
    {
        float[,] noiseMap = new float[size, size];
        float halfSize = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseValue = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = ((float)x - halfSize) / scale * frequency + offsets[i];
                    float sampleZ = ((float)z - halfSize) / scale * frequency + offsets[i];

                    float sample = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;

                    noiseValue += sample * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseValue = Mathf.InverseLerp(-1f, 1f, noiseValue);
                noiseMap[x, z] = noiseValue;
            }
        }

        return noiseMap;
    }
}
