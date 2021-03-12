using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    GameObject voxelObject;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    BoxCollider boxCollider;
    Rigidbody rigidbody;

    Vector3Int position;
    int voxelType;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public Voxel(Vector3Int _position, int _voxelType)
    {
        position = _position;
        voxelType = _voxelType;

        CreateVoxel();
    }

    void CreateVoxel()
    {
        voxelObject = new GameObject();

        meshFilter = voxelObject.AddComponent<MeshFilter>();
        meshRenderer = voxelObject.AddComponent<MeshRenderer>();
        boxCollider = voxelObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0.5f, 0.5f, 0.5f);
        rigidbody = voxelObject.AddComponent<Rigidbody>();

        meshRenderer.material = DataContainer.Instance.voxelGround;

        voxelObject.transform.position = position;
        voxelObject.name = "Voxel ";

        DrawVoxel();
    }

    void DrawVoxel()
    {
        for(int i = 0; i < 6; i++)
        {
            vertices.Add(ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 0]]);
            vertices.Add(ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 1]]);
            vertices.Add(ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 2]]);
            vertices.Add(ChunkData.voxelVertices[ChunkData.voxelTriangles[i, 3]]);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 3);

            vertexIndex += 4;

            AddTexture(DataContainer.Instance.voxelTypes[voxelType].textureIndex);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.Optimize();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
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

    public void DestroyVoxel()
    {
        GameObject.Destroy(voxelObject);
    }
}
