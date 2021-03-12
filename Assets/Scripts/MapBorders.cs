using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBorders
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    GameObject borderObject;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> bottomTriangles = new List<int>();
    List<int> waterTriangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    int vertexIndexCollider = 0;
    List<Vector3> verticesCollider = new List<Vector3>();
    List<int> trianglesCollider = new List<int>();

    Material[] materials = new Material[2];

    int mapSize;
    int borderSize;
    int waterLevel;

    //Constructor to initialize the border
    public MapBorders(int _mapSize, int _borderSize, int _waterLevel)
    {
        mapSize = _mapSize;
        borderSize = _borderSize;
        waterLevel = _waterLevel;

        borderObject = new GameObject();
        meshFilter = borderObject.AddComponent<MeshFilter>();
        meshRenderer = borderObject.AddComponent<MeshRenderer>();
        meshCollider = borderObject.AddComponent<MeshCollider>();

        materials[0] = DataContainer.Instance.voxelGround;
        materials[1] = DataContainer.Instance.voxelWater;
        meshRenderer.materials = materials;

        borderObject.transform.position = Vector3.zero;
        borderObject.name = "Border";

        CreateMeshData();
    }

    void CreateMeshData()
    {
        waterLevel += 1;
        int bottomTextureIndex = DataContainer.Instance.surfaces[DataContainer.Instance.selectedSurface].surfaceVoxels[0].voxelTypeIndexes[0];
        int mapWaterSize = (DataContainer.Instance.chunks * ChunkData.chunkWidth) + (2 * borderSize);
        float textureOffsetWater = 1f / mapWaterSize;

        //BOTTOM
        vertices.Add(new Vector3(-borderSize, 0, -borderSize));
        vertices.Add(new Vector3(-borderSize, 0, mapSize + borderSize));
        vertices.Add(new Vector3(mapSize + borderSize, 0, mapSize + borderSize));
        vertices.Add(new Vector3(mapSize + borderSize, 0, -borderSize));

        verticesCollider.Add(new Vector3(-borderSize, 0, -borderSize));
        verticesCollider.Add(new Vector3(-borderSize, 0, mapSize + borderSize));
        verticesCollider.Add(new Vector3(mapSize + borderSize, 0, mapSize + borderSize));
        verticesCollider.Add(new Vector3(mapSize + borderSize, 0, -borderSize));

        AddTriangles(0);
        AddTriangles(2);
        AddTexture(bottomTextureIndex);

        #region EAST_Water
        vertices.Add(new Vector3(-borderSize, waterLevel, -borderSize));
        vertices.Add(new Vector3(-borderSize, waterLevel, mapSize + borderSize));
        vertices.Add(new Vector3(0, waterLevel, mapSize));
        vertices.Add(new Vector3(0, waterLevel, 0));
        
        AddTriangles(1);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(borderSize * textureOffsetWater, (mapSize + borderSize) * textureOffsetWater));
        uvs.Add(new Vector2(borderSize * textureOffsetWater, borderSize * textureOffsetWater));
        #endregion

        #region NORTH_Water
        vertices.Add(new Vector3(-borderSize, waterLevel, mapSize + borderSize));
        vertices.Add(new Vector3(mapSize + borderSize, waterLevel, mapSize + borderSize));
        vertices.Add(new Vector3(mapSize, waterLevel, mapSize));
        vertices.Add(new Vector3(0, waterLevel, mapSize));

        AddTriangles(1);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2((mapSize + borderSize) * textureOffsetWater, (mapSize + borderSize) * textureOffsetWater));
        uvs.Add(new Vector2(borderSize * textureOffsetWater, (mapSize + borderSize) * textureOffsetWater));
        #endregion

        #region WEST_Water
        vertices.Add(new Vector3(mapSize + borderSize, waterLevel, mapSize + borderSize));
        vertices.Add(new Vector3(mapSize + borderSize, waterLevel, -borderSize));
        vertices.Add(new Vector3(mapSize, waterLevel, 0));
        vertices.Add(new Vector3(mapSize, waterLevel, mapSize));

        AddTriangles(1);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2((mapSize + borderSize) * textureOffsetWater, borderSize * textureOffsetWater));
        uvs.Add(new Vector2((mapSize + borderSize) * textureOffsetWater, (mapSize + borderSize) * textureOffsetWater));
        #endregion

        #region SOUTH_Water
        vertices.Add(new Vector3(mapSize + borderSize, waterLevel, -borderSize));
        vertices.Add(new Vector3(-borderSize, waterLevel, -borderSize));
        vertices.Add(new Vector3(0, waterLevel, 0));
        vertices.Add(new Vector3(mapSize, waterLevel, 0));

        AddTriangles(1);

        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(borderSize * textureOffsetWater, borderSize * textureOffsetWater));
        uvs.Add(new Vector2((mapSize + borderSize) * textureOffsetWater, borderSize * textureOffsetWater));
        #endregion

        //EAST COLLIDER
        verticesCollider.Add(new Vector3(0, 0, -borderSize));
        verticesCollider.Add(new Vector3(0, borderSize, -borderSize));
        verticesCollider.Add(new Vector3(0, borderSize, mapSize + borderSize));
        verticesCollider.Add(new Vector3(0, 0, mapSize + borderSize));

        AddTriangles(2);

        //NORTH COLLIDER
        verticesCollider.Add(new Vector3(-borderSize, 0, mapSize));
        verticesCollider.Add(new Vector3(-borderSize, borderSize, mapSize));
        verticesCollider.Add(new Vector3(mapSize + borderSize, borderSize, mapSize));
        verticesCollider.Add(new Vector3(mapSize + borderSize, 0, mapSize));

        AddTriangles(2);

        //WEST COLLIDER
        verticesCollider.Add(new Vector3(mapSize, 0, mapSize + borderSize));
        verticesCollider.Add(new Vector3(mapSize, borderSize, mapSize + borderSize));
        verticesCollider.Add(new Vector3(mapSize, borderSize, -borderSize));
        verticesCollider.Add(new Vector3(mapSize, 0, -borderSize));

        AddTriangles(2);

        //SOUTH COLLIDER
        verticesCollider.Add(new Vector3(mapSize + borderSize, 0, 0));
        verticesCollider.Add(new Vector3(mapSize + borderSize, borderSize, 0));
        verticesCollider.Add(new Vector3(-borderSize, borderSize, 0));
        verticesCollider.Add(new Vector3(-borderSize, 0, 0));

        AddTriangles(2);

        GenerateMesh();
    }

    void AddTriangles(int listIndex)
    {
        switch(listIndex)
        {
            case 0:
                bottomTriangles.Add(vertexIndex);
                bottomTriangles.Add(vertexIndex + 1);
                bottomTriangles.Add(vertexIndex + 2);
                bottomTriangles.Add(vertexIndex + 2);
                bottomTriangles.Add(vertexIndex + 3);
                bottomTriangles.Add(vertexIndex + 0);
                vertexIndex += 4;
                break;
            case 1:
                waterTriangles.Add(vertexIndex);
                waterTriangles.Add(vertexIndex + 1);
                waterTriangles.Add(vertexIndex + 2);
                waterTriangles.Add(vertexIndex + 2);
                waterTriangles.Add(vertexIndex + 3);
                waterTriangles.Add(vertexIndex + 0);
                vertexIndex += 4;
                break;
            case 2:
                trianglesCollider.Add(vertexIndexCollider);
                trianglesCollider.Add(vertexIndexCollider + 1);
                trianglesCollider.Add(vertexIndexCollider + 2);
                trianglesCollider.Add(vertexIndexCollider + 2);
                trianglesCollider.Add(vertexIndexCollider + 3);
                trianglesCollider.Add(vertexIndexCollider + 0);
                vertexIndexCollider += 4;
                break;
        }
    }

    void AddTexture(int textureID)
    {
        int row = textureID / DataContainer.Instance.textureAtlasSize;
        int rowInvert = (DataContainer.Instance.textureAtlasSize - 1) - row;
        int col = textureID % DataContainer.Instance.textureAtlasSize;

        float textureOffset = 1f / DataContainer.Instance.textureAtlasSize;

        float u = col * textureOffset;
        float v = rowInvert * textureOffset;

        uvs.Add(new Vector2(u, v));
        uvs.Add(new Vector2(u, v + textureOffset));
        uvs.Add(new Vector2(u + textureOffset, v));
        uvs.Add(new Vector2(u + textureOffset, v + textureOffset));
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(bottomTriangles.ToArray(), 0);
        mesh.SetTriangles(waterTriangles.ToArray(), 1);
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        Mesh colliderMesh = new Mesh();

        colliderMesh.vertices = verticesCollider.ToArray();
        colliderMesh.triangles = trianglesCollider.ToArray();

        meshCollider.sharedMesh = colliderMesh;

        ClearData();
    }

    void ClearData()
    {
        vertexIndex = 0;
        vertices.Clear();
        bottomTriangles.Clear();
        waterTriangles.Clear();
        uvs.Clear();

        vertexIndexCollider = 0;
        verticesCollider.Clear();
        trianglesCollider.Clear();
    }
}
