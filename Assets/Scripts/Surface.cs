using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Surface
{
    public string name;
    public SurfaceVoxel[] surfaceVoxels;
}

[System.Serializable]
public class SurfaceVoxel
{
    public string name;
    public float height;
    public Color color;
    public int[] voxelTypeIndexes;
}
