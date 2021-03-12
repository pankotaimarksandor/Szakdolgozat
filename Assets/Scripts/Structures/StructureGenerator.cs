using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    DataContainer data;

    void Start()
    {
        data = DataContainer.Instance;

        if(!data.structuresCreated)
        {
            GenerateStructures();
            data.structuresCreated = true;
        }
    }

    //Generating strucutres from text file with position and voxel type
    void GenerateStructures()
    {
        for (int i = 0; i < data.structureZones.Length; i++)
        {
            for (int j = 0; j < data.structureZones[i].structures.Length; j++)
            {
                List<VoxelModification> modificationList = new List<VoxelModification>();

                string text = data.structureZones[i].structures[j].textAsset.text;
                List<string> lines = new List<string>();
                lines.AddRange(text.Split("\n"[0]));

                for (int l = 0; l < lines.Count; l++)
                {
                    string[] split = lines[l].Split();

                    VoxelModification newModification = new VoxelModification
                    {
                        position = new Vector3Int(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[0])),
                        voxelType = int.Parse(split[3])
                    };

                    modificationList.Add(newModification);
                }

                data.structureZones[i].structures[j].modifications = modificationList;
            }
        }
    }
}
