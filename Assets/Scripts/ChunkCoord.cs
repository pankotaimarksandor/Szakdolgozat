using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord otherChunkCoord)
    {
        if (otherChunkCoord == null)
        {
            return false;
        }
        else if (otherChunkCoord.x == x && otherChunkCoord.z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Exist(List<ChunkCoord> chunkCoordList)
    {
        if(chunkCoordList.Count == 0)
        {
            return false;
        }
        else
        {
            for(int i = 0; i < chunkCoordList.Count; i++)
            {
                if(chunkCoordList[i].x == x && chunkCoordList[i].z == z)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
