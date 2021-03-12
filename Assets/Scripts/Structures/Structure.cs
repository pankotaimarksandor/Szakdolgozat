using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StructureZone
{
    public string name;
    public Structure[] structures;
}

[System.Serializable]
public class Structure
{
    public string name;
    public TextAsset textAsset;
    public List<VoxelModification> modifications;
}

public class VoxelModification
{
    public Vector3Int position;
    public int voxelType;
}
