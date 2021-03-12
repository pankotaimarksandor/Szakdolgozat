using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Gradient
{
    public static float[,] GenerateGradientMap(int size, AnimationCurve gradientCurve)
    {
        float[,] gradientMap = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float gradientX = (float)x / (float)size * 2 - 1;
                float gradientZ = (float)z / (float)size * 2 - 1;

                float gradient = Mathf.Max(Mathf.Abs(gradientX), Mathf.Abs(gradientZ));
                float gradientValue = gradientCurve.Evaluate(gradient);
                
                gradientMap[x, z] = gradientValue;
            }
        }

        return gradientMap;
    }
}
