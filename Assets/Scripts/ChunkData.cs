using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkData
{
	//Chunk size variables
	public static readonly int chunkWidth = 16;
	public static readonly int chunkHeight = 128;

	//Voxel vertices
	public static readonly Vector3Int[] voxelVertices =
	{
		new Vector3Int(0, 0, 0), //index 0
		new Vector3Int(1, 0, 0), //index 1
		new Vector3Int(1, 1, 0), //index 2
		new Vector3Int(0, 1, 0), //index 3
		new Vector3Int(0, 0, 1), //index 4
		new Vector3Int(1, 0, 1), //index 5
		new Vector3Int(1, 1, 1), //index 6
		new Vector3Int(0, 1, 1), //index 7
    };

	//Voxel triangles
	public static readonly int[,] voxelTriangles =
	{
		{0, 3, 1, 2}, //front
		{5, 6, 4, 7}, //back
		{3, 7, 2, 6}, //top
		{1, 5, 0, 4}, //bottom
		{4, 7, 0, 3}, //left
		{1, 2, 5, 6}, //right
    };

	//Voxel faces
	public static readonly Vector3Int[] voxelFaceChecks =
	{
		new Vector3Int(0, 0, -1), //front
		new Vector3Int(0, 0, 1),  //back
		new Vector3Int(0, 1, 0),  //top
		new Vector3Int(0, -1, 0), //bottom
		new Vector3Int(-1, 0, 0), //left
		new Vector3Int(1, 0, 0),  //right
    };

	//Voxel all neighbour position
	public static readonly Vector3Int[] voxelDiagonalCheck =
	{
		new Vector3Int(0, 0, -1), //front
		new Vector3Int(0, 0, 1),  //back
		new Vector3Int(0, 1, 0),  //top
		new Vector3Int(0, -1, 0), //bottom
		new Vector3Int(-1, 0, 0), //left
		new Vector3Int(1, 0, 0),  //right

		//up //8
		new Vector3Int(-1, 1, 1),
		new Vector3Int(0, 1, 1),
		new Vector3Int(1, 1, 1),
		new Vector3Int(-1, 1, 0),
		new Vector3Int(1, 1, 0),
		new Vector3Int(1, 1, -1),
		new Vector3Int(0, 1, -1),
		new Vector3Int(-1, 1, -1),

		//near //4
		new Vector3Int(1, 0, 1),
		new Vector3Int(-1, 0, 1),
		new Vector3Int(1, 0, -1),
		new Vector3Int(-1, 0, -1),

		//bottom //8
		new Vector3Int(-1, -1, 1),
		new Vector3Int(0, -1, 1),
		new Vector3Int(1, -1, 1),
		new Vector3Int(-1, -1, 0),
		new Vector3Int(1, -1, 0),
		new Vector3Int(1, -1, -1),
		new Vector3Int(0, -1, -1),
		new Vector3Int(-1, -1, -1),
	};
}
