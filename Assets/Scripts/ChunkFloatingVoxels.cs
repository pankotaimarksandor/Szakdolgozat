using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkFloatingVoxels
{
    public static List<Vector3Int> CheckForFloatingVoxels(int[,,] mapToCheck)
    {
        List<Vector3Int> floatingPositions = new List<Vector3Int>();
        List<Vector3Int> checkPositions = new List<Vector3Int>();
        int[,,] map = new int[ChunkData.chunkWidth, ChunkData.chunkHeight, ChunkData.chunkWidth];

        //FISRT PASS, copy the array to a new array to avoid unexpected results
        Vector3Int lowestVoxel = new Vector3Int(0, 0, 0);
        bool voxelFound = false;

        for (int x = 0; x < ChunkData.chunkWidth; x++)
        {
            for (int y = 0; y < ChunkData.chunkHeight; y++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    map[x, y, z] = mapToCheck[x, y, z];
                }
            }
        }

        //SECOND PASS, find the first visible voxel from bottom
        for (int y = 0; y < ChunkData.chunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.chunkWidth; x++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    if (map[x, y, z] != 0 && !voxelFound)
                    {
                        lowestVoxel = new Vector3Int(x, y, z);
                        voxelFound = true;
                        break;
                    }
                }
            }
        }

        checkPositions.Add(lowestVoxel);

        //THIRD PASS, search for all connected neighbours
        while (checkPositions.Count > 0)
        {
            map[checkPositions[0].x, checkPositions[0].y, checkPositions[0].z] = 0;

            //Check all neighbour of the current position
            for (int i = 0; i < 26; i++)
            {
                Vector3Int np = checkPositions[0] + ChunkData.voxelDiagonalCheck[i];

                if (np.x >= 0 && np.x < ChunkData.chunkWidth && np.y >= 0 && np.y < ChunkData.chunkHeight && np.z >= 0 && np.z < ChunkData.chunkWidth)
                {
                    if (map[np.x, np.y, np.z] != 0 && !checkPositions.Contains(np))
                    {
                        checkPositions.Add(np);
                    }
                }
            }

            checkPositions.RemoveAt(0);
        }

        //FOURTH PASS, check the new map, the remaining 1 means the voxel is not connected to the lowest visible voxel
        for (int x = 0; x < ChunkData.chunkWidth; x++)
        {
            for (int y = 0; y < ChunkData.chunkHeight; y++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    if (map[x, y, z] == 1)
                    {
                        floatingPositions.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        return floatingPositions;
    }
}
