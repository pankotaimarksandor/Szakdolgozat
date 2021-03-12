using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    GameObject chunkObject;

    Map map;
    ChunkCoord chunkCoord;
    Material[] materials = new Material[2];

    int vertexIndex = 0;
    int vertexIndexCollider = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> verticesCollider = new List<Vector3>();
    List<int> trianglesGround = new List<int>();
    List<int> trianglesWater = new List<int>();
    List<int> trianglesCollider = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public ChunkModification modification;
    public List<Vector3> enemySpawnPositions = new List<Vector3>();

    Vector3Int chunkPosition;

    int[,,] voxelMap = new int[ChunkData.chunkWidth, ChunkData.chunkHeight, ChunkData.chunkWidth];
    public int[,,] visiblityMap = new int[ChunkData.chunkWidth, ChunkData.chunkHeight, ChunkData.chunkWidth];

    bool isMapCreated = false;
    bool isMeshCreated = false;
    bool isStructureAdded = false;

    public Chunk(Map _map, ChunkCoord _chunkCoord)
    {
        map = _map;
        chunkCoord = _chunkCoord;

        chunkPosition = new Vector3Int(chunkCoord.x * ChunkData.chunkWidth, 0, chunkCoord.z * ChunkData.chunkWidth);
    }

    //Get the state of voxelmap
    public bool IsMapCreated
    {
        get
        {
            return isMapCreated;
        }
    }

    //Get the state of mesh
    public bool IsMeshCreated
    {
        get
        {
            return isMeshCreated;
        }

        set
        {
            isMeshCreated = value;

            if (!value)
            {
                GameObject.Destroy(chunkObject);
            }
        }
    }

    //Get and set structure added state
    public bool IsStructureAdded
    {
        get
        {
            return isStructureAdded;
        }

        set
        {
            isStructureAdded = value;
        }
    }

    //create GameObject
    public void CreateChunkObject()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        materials[0] = DataContainer.Instance.voxelGround;
        materials[1] = DataContainer.Instance.voxelWater;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(map.transform);
        chunkObject.transform.position = new Vector3(chunkCoord.x * ChunkData.chunkWidth, 0, chunkCoord.z * ChunkData.chunkWidth);
        chunkObject.name = "Chunk " + chunkCoord.x + ", " + chunkCoord.z;

        chunkObject.tag = "Ground";
    }

    //Create the voxelMap with modifications
    public void CreateVoxelMap()
    {
        for (int x = 0; x < ChunkData.chunkWidth; x++)
        {
            for (int y = 0; y < ChunkData.chunkHeight; y++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    voxelMap[x, y, z] = map.GetVoxelType(new Vector3Int(x, y, z) + chunkPosition);
                    visiblityMap[x, y, z] = 0;
                }
            }
        }

        if(modification != null)
        {
            ApplyModification();
        }

        isMapCreated = true;
    }

    //Apply modification
    void ApplyModification()
    {
        int x = modification.position.x - chunkPosition.x;
        int y = modification.position.y;
        int z = modification.position.z - chunkPosition.z;

        Vector3Int startPosition = new Vector3Int(x, y, z);

        int zone = modification.zone;
        int index = modification.index;

        for(int i = 0; i < DataContainer.Instance.structureZones[zone].structures[index].modifications.Count; i++)
        {
            Vector3Int position = DataContainer.Instance.structureZones[zone].structures[index].modifications[i].position + startPosition;
            int newType = DataContainer.Instance.structureZones[zone].structures[index].modifications[i].voxelType;

            voxelMap[position.x, position.y, position.z] = newType;
            visiblityMap[position.x, position.y, position.z] = 1;
        }
    }

    //Check the face to draw or not
    public bool CheckFaceDrawInChunk(Vector3Int position, bool isTransparent)
    {
        //if the voxel to check is not transparent
        if(!isTransparent)
        {
            if (position.y < 0)
            {
                //dont draw the face on bottom
                return false;
            }

            if (position.y > ChunkData.chunkHeight - 1)
            {
                //draw the face if it is on top
                return true;
            }

            if (position.x < 0 || position.x > ChunkData.chunkWidth - 1 || position.z < 0 || position.z > ChunkData.chunkWidth - 1)
            {
                //check the face draw in map and return with its value
                return map.CheckFaceDrawInMap(position + chunkPosition, isTransparent);
            }

            bool neighbourIsTransparent = DataContainer.Instance.voxelTypes[voxelMap[position.x, position.y, position.z]].isTransparent;

            if (neighbourIsTransparent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        if(isTransparent)
        {
            if (position.y < 0)
            {
                //dont draw the face on bottom
                return false;
            }

            if (position.y > ChunkData.chunkHeight - 1)
            {
                //draw the face if it is on top
                return true;
            }

            if (position.x < 0 || position.x > ChunkData.chunkWidth - 1 || position.z < 0 || position.z > ChunkData.chunkWidth - 1)
            {
                //check the face draw in map and return with its value
                return map.CheckFaceDrawInMap(position + chunkPosition, isTransparent);
            }

            bool neighbourIsVisible = DataContainer.Instance.voxelTypes[voxelMap[position.x, position.y, position.z]].isVisible;

            if (neighbourIsVisible)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    //Check voxel in chunk or map
    bool CheckVoxelInChunk(Vector3Int position)
    {
        if(position.x < 0 || position.x > ChunkData.chunkWidth - 1 || position.y < 0 || position.y > ChunkData.chunkHeight - 1 || position.z < 0 || position.z > ChunkData.chunkWidth - 1)
        {
            return false;
        }

        return true;
    }

    //Get voxel by given position
    public int GetVoxel(Vector3Int position)
    {
        int x = position.x - chunkPosition.x;
        int z = position.z - chunkPosition.z;

        return voxelMap[x, position.y, z];
    }

    //Set voxel in given position with new type
    public void SetVoxel(Vector3Int position, int newType)
    {
        int x = position.x - chunkPosition.x;
        int z = position.z - chunkPosition.z;

        voxelMap[x, position.y, z] = newType;
    }

    //Destruct voxel by position, change the type to air in voxelmap
    //Check all neighbour positions for other chunk update
    //start checking for floating voxels in chunk
    public void DestructVoxel(Vector3Int position)
    {
        int x = position.x - chunkPosition.x;
        int z = position.z - chunkPosition.z;
        
        visiblityMap[x, position.y, z] = 0;
        voxelMap[x, position.y, z] = 0;

        CheckNeighbourVoxels(x, position.y, z);

        CheckFloatingVoxels();

        UpdateMeshData();
        UpdateMesh();
    }

    //Check neighbour voxels, if outside, update the other chunks mesh, if water, add the position to the waterflow list
    void CheckNeighbourVoxels(int x, int y, int z)
    {
        Vector3Int destructedVoxel = new Vector3Int(x, y, z);
        bool isWater = false;

        for(int i = 0; i < 6; i++)
        {
            Vector3Int neighbourVoxel = destructedVoxel + ChunkData.voxelFaceChecks[i];

            if(!CheckVoxelInChunk(neighbourVoxel))
            {
                //check if the neighbour is water
                if(map.GetChunk(neighbourVoxel + chunkPosition).GetVoxel(neighbourVoxel + chunkPosition) == 1)
                {
                    isWater = true;
                }
                map.GetChunk(neighbourVoxel + chunkPosition).UpdateMeshData();
                map.GetChunk(neighbourVoxel + chunkPosition).UpdateMesh();
            }
            else
            {
                if(voxelMap[neighbourVoxel.x, neighbourVoxel.y, neighbourVoxel.z] == 1)
                {
                    isWater = true;
                }
            }
        }

        if(isWater)
        {
            map.waterToFlow.Add(destructedVoxel + chunkPosition);
        }
    }

    //Check for floating voxels
    void CheckFloatingVoxels()
    {
        List<Vector3Int> floatingVoxels = ChunkFloatingVoxels.CheckForFloatingVoxels(visiblityMap);
        if(floatingVoxels.Count > 0)
        {
            for(int i = 0; i < floatingVoxels.Count; i++)
            {
                visiblityMap[floatingVoxels[i].x, floatingVoxels[i].y, floatingVoxels[i].z] = 0;
                floatingVoxels[i] += chunkPosition;
            }

            map.GenerateVoxels(floatingVoxels, chunkCoord);
        }
    }

    //Update mesh data only if the voxel is not air
    public void UpdateMeshData()
    {
        for (int x = 0; x < ChunkData.chunkWidth; x++)
        {
            for (int y = 0; y < ChunkData.chunkHeight; y++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    if (voxelMap[x, y, z] != 0)
                    {
                        AddMeshDataToList(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    //Add mesh data to list based on neighbour voxels
    void AddMeshDataToList(Vector3Int position)
    {
        int currentVoxel = voxelMap[position.x, position.y, position.z];
        bool isTransparent = DataContainer.Instance.voxelTypes[currentVoxel].isTransparent;

        bool isDrawn = false;

        for (int i = 0; i < 6; i++)
        {
            Vector3Int neighbourPosition = position + ChunkData.voxelFaceChecks[i];

            if (!isTransparent && CheckFaceDrawInChunk(neighbourPosition, isTransparent))
            {
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 0]]);
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 1]]);
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 2]]);
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 3]]);

                trianglesGround.Add(vertexIndex);
                trianglesGround.Add(vertexIndex + 1);
                trianglesGround.Add(vertexIndex + 2);
                trianglesGround.Add(vertexIndex + 2);
                trianglesGround.Add(vertexIndex + 1);
                trianglesGround.Add(vertexIndex + 3);

                vertexIndex += 4;

                AddGroundTexture(DataContainer.Instance.voxelTypes[currentVoxel].textureIndex);

                verticesCollider.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 0]]);
                verticesCollider.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 1]]);
                verticesCollider.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 2]]);
                verticesCollider.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 3]]);

                trianglesCollider.Add(vertexIndexCollider);
                trianglesCollider.Add(vertexIndexCollider + 1);
                trianglesCollider.Add(vertexIndexCollider + 2);
                trianglesCollider.Add(vertexIndexCollider + 2);
                trianglesCollider.Add(vertexIndexCollider + 1);
                trianglesCollider.Add(vertexIndexCollider + 3);

                vertexIndexCollider += 4;
                isDrawn = true;
            }

            if (isTransparent && CheckFaceDrawInChunk(neighbourPosition, isTransparent))
            {
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 0]]);
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 1]]);
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 2]]);
                vertices.Add(position + ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 3]]);

                trianglesWater.Add(vertexIndex);
                trianglesWater.Add(vertexIndex + 1);
                trianglesWater.Add(vertexIndex + 2);
                trianglesWater.Add(vertexIndex + 2);
                trianglesWater.Add(vertexIndex + 1);
                trianglesWater.Add(vertexIndex + 3);

                vertexIndex += 4;

                AddWaterTexture(position.x, position.z);
                isDrawn = true;
            }
        }

        //if one of the faces is drawn, it is a visible voxel, so flag it in visiblity map
        if(isDrawn == true)
        {
            visiblityMap[position.x, position.y, position.z] = 1;
        }
    }

    //Add uv coords to ground type voxels
    void AddGroundTexture(int textureID)
    {
        int row = textureID / DataContainer.Instance.textureAtlasSize;
        int rowInvert = (DataContainer.Instance.textureAtlasSize - 1) - row;
        int col = textureID % DataContainer.Instance.textureAtlasSize;

        float textureOffset = 1f / DataContainer.Instance.textureAtlasSize;

        float x = col * textureOffset;
        float y = rowInvert * textureOffset;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + textureOffset));
        uvs.Add(new Vector2(x + textureOffset, y));
        uvs.Add(new Vector2(x + textureOffset, y + textureOffset));
    }

    //Add uv coords to water type voxels
    void AddWaterTexture(int x, int z)
    {
        float textureOffset = 1f / map.mapWaterSize;

        float u = (x + chunkPosition.x + DataContainer.Instance.mapBorderWidth) * textureOffset;
        float v = (z + chunkPosition.z + DataContainer.Instance.mapBorderWidth) * textureOffset;

        uvs.Add(new Vector2(u, v));
        uvs.Add(new Vector2(u, v + textureOffset));
        uvs.Add(new Vector2(u + textureOffset, v));
        uvs.Add(new Vector2(u + textureOffset, v + textureOffset));
    }

    //Create new mesh from lists
    public void UpdateMesh()
    {
        Mesh mesh = new Mesh();
        Mesh colliderMesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = 2;
        mesh.SetTriangles(trianglesGround.ToArray(), 0);
        mesh.SetTriangles(trianglesWater.ToArray(), 1);
        mesh.uv = uvs.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        colliderMesh.vertices = verticesCollider.ToArray();
        colliderMesh.triangles = trianglesCollider.ToArray();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = colliderMesh;

        ClearMeshData();
    }

    //Clear lists to avoid duplicates in the memory and unexpected results for updating data
    void ClearMeshData()
    {
        vertices.Clear();
        verticesCollider.Clear();
        trianglesGround.Clear();
        trianglesWater.Clear();
        trianglesCollider.Clear();
        uvs.Clear();
        vertexIndex = 0;
        vertexIndexCollider = 0;
    }
}
