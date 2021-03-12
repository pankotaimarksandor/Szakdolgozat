using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    DataContainer data;

    [HideInInspector] public int mapSize;
    float mapScale;
    public int waterLevel;
    public int mapWaterSize;

    int[,] heightMap;
    Chunk[,] chunks;

    public GameObject player;
    public GameObject enemy;
    public Vector3 spawnPosition = Vector3.zero;

    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    List<ChunkCoord> chunksToCreateVoxelMap = new List<ChunkCoord>();
    bool isCreatingVoxelMap;
    List<ChunkCoord> chunksToCreateMesh = new List<ChunkCoord>();
    bool isCreatingMesh;
    public List<Vector3Int> waterToFlow = new List<Vector3Int>();
    bool isWaterFlowing;


    void Start()
    {
        data = DataContainer.Instance;

        mapSize = data.chunks * ChunkData.chunkWidth;
        mapWaterSize = mapSize + 512;
        mapScale = (mapSize / data.textureSize) * data.noiseScale;
        waterLevel = Mathf.FloorToInt(data.waterHeight * data.heightMultiplier);

        heightMap = new int[mapSize, mapSize];
        chunks = new Chunk[data.chunks, data.chunks];

        MapBorders mapBorders = new MapBorders(mapSize, data.mapBorderWidth, waterLevel);

        GenerateHeightMap();
        InitializeChunks();
        GenerateSpawnPositions();
        CheckViewDistance();
        CreateChunksOnLoad();
    }

    void Update()
    {
        playerChunkCoord = GetChunkCoord(player.transform.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if (chunksToCreateVoxelMap.Count > 0 && !isCreatingVoxelMap)
        {
            StartCoroutine(CreateMap());
        }

        if (chunksToCreateMesh.Count > 0 && !isCreatingMesh)
        {
            StartCoroutine(CreateMesh());
        }

        if (waterToFlow.Count > 0 && !isWaterFlowing)
        {
            StartCoroutine(WaterFlow());
        }
    }

    //Generate height map
    void GenerateHeightMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapSize, data.noiseOctaves, mapScale, data.noisePersistance, data.noiseLacunarity, data.noiseOffsets);
        float[,] gradientMap = Gradient.GenerateGradientMap(mapSize, data.gradientCurve);

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                if (data.useCurve)
                {
                    heightMap[x, z] = Mathf.FloorToInt(data.heightMultiplier * data.heightCurve.Evaluate(Mathf.Clamp01(noiseMap[x, z] - gradientMap[x, z])));
                }
                else
                {
                    heightMap[x, z] = Mathf.FloorToInt(data.heightMultiplier * Mathf.Clamp01(noiseMap[x, z] - gradientMap[x, z]));
                }
            }
        }
    }

    //Initialize all chunks in map
    void InitializeChunks()
    {
        for (int x = 0; x < data.chunks; x++)
        {
            for (int z = 0; z < data.chunks; z++)
            {
                chunks[x, z] = new Chunk(this, new ChunkCoord(x, z));
            }
        }
    }

    //Generate player, enemies and structures spawn points
    void GenerateSpawnPositions()
    {
        List<Vector2> points = Poisson.GeneratePoints(30, new Vector2(mapSize, mapSize), 15);
        List<Vector2> spawnPoints = new List<Vector2>();

        //check all points from poisson, save the point when it is bigger than the water level
        for (int i = 0; i < points.Count; i++)
        {
            if (heightMap[(int)points[i].x, (int)points[i].y] > waterLevel)
            {
                spawnPoints.Add(points[i]);
            }
        }

        int p = 1;

        //choose one random pont to spawn the player
        while (p > 0)
        {
            int index = Random.Range(0, spawnPoints.Count);
            Vector2 position = spawnPoints[index];

            int x = Mathf.FloorToInt(position.x);
            int z = Mathf.FloorToInt(position.y);

            spawnPosition = new Vector3(x, heightMap[x, z] + 1, z);
            spawnPoints.RemoveAt(index);
            p--;
        }

        //remaining points is for structures
        foreach (Vector2 point in spawnPoints)
        {
            ChunkCoord c = GetChunkCoord(new Vector3(point.x, 0, point.y));

            if (!chunks[c.x, c.z].IsStructureAdded)
            {
                int x = Mathf.FloorToInt(point.x);
                int z = Mathf.FloorToInt(point.y);

                int randomIndex = Random.Range(0, data.structureZones[data.selectedSurface].structures.Length);

                //calculate new position to skip structure overflow
                int rCoord = Random.Range(0, 6);
                int xCoord = c.x * ChunkData.chunkWidth + 5 + rCoord;
                int zCoord = c.z * ChunkData.chunkWidth + 5 + rCoord;
                Vector3Int newPosition = new Vector3Int(xCoord, heightMap[xCoord, zCoord], zCoord);

                ChunkModification modification = new ChunkModification
                {
                    position = newPosition,
                    zone = data.selectedSurface,
                    index = randomIndex
                };

                chunks[c.x, c.z].modification = modification;
                chunks[c.x, c.z].IsStructureAdded = true;
            }
        }

        playerLastChunkCoord = GetChunkCoord(spawnPosition);
        playerChunkCoord = GetChunkCoord(spawnPosition);
        player.transform.position = spawnPosition;
    }

    //Check if the spawn position is air
    public bool ValidateSpawnPont(Vector3 position)
    {
        Vector3Int pos = Vector3Int.FloorToInt(position);

        if (GetChunk(pos).GetVoxel(new Vector3Int(pos.x, heightMap[pos.x, pos.z] + 1, pos.z)) == 0)
        {
            return true;
        }
        else return false;
    }

    //Get spawn position for enemy by heightmap
    public Vector3Int GetSpawnPosition(Vector3 position)
    {
        Vector3Int spawnPosition = new Vector3Int((int)position.x, heightMap[(int)position.x, (int)position.z] + 1, (int)position.z);

        return spawnPosition;
    }

    //Checking view distance
    void CheckViewDistance()
    {
        ChunkCoord playerNewCoord = GetChunkCoord(player.transform.position);
        playerLastChunkCoord = playerNewCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();

        //VOXEL MAP
        for (int x = playerNewCoord.x - data.viewDistance - 1; x < playerNewCoord.x + data.viewDistance + 1; x++)
        {
            for (int z = playerNewCoord.z - data.viewDistance - 1; z < playerNewCoord.z + data.viewDistance + 1; z++)
            {
                if (CheckChunkInMap(new ChunkCoord(x, z)))
                {
                    if (!chunks[x, z].IsMapCreated)
                    {
                        chunksToCreateVoxelMap.Add(new ChunkCoord(x, z));
                    }
                }
            }
        }

        //MESH AND MODIFICATIONS
        for (int x = playerNewCoord.x - data.viewDistance; x < playerNewCoord.x + data.viewDistance; x++)
        {
            for (int z = playerNewCoord.z - data.viewDistance; z < playerNewCoord.z + data.viewDistance; z++)
            {
                if (CheckChunkInMap(new ChunkCoord(x, z)))
                {
                    if (!chunks[x, z].IsMeshCreated)
                    {
                        chunksToCreateMesh.Add(new ChunkCoord(x, z));
                    }

                    activeChunks.Add(new ChunkCoord(x, z));
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].IsMeshCreated = false;
        }
    }

    //Check the ground voxel for player
    public bool CheckGroundVoxel(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);
        int z = Mathf.FloorToInt(position.z);

        int xChunk = Mathf.FloorToInt(position.x / ChunkData.chunkWidth);
        int zChunk = Mathf.FloorToInt(position.z / ChunkData.chunkWidth);

        return data.voxelTypes[chunks[xChunk, zChunk].GetVoxel(new Vector3Int(x, y, z))].isFluid;
    }

    //Check if the voxel is in or outside of the map by given position
    public bool CheckVoxelInMap(Vector3Int position)
    {
        if (position.x < 0 || position.x > mapSize - 1 || position.y < 0 || position.y > ChunkData.chunkHeight - 1 || position.z < 0 || position.z > mapSize)
        {
            return false;
        }

        return true;
    }

    //Check if the chunk is in map or not by given chunkCoord
    bool CheckChunkInMap(ChunkCoord chunkCoord)
    {
        if (chunkCoord.x < 0 || chunkCoord.x > data.chunks - 1 || chunkCoord.z < 0 || chunkCoord.z > data.chunks - 1)
        {
            return false;
        }

        return true;
    }

    public bool CheckFaceDrawInMap(Vector3Int position, bool isTransparent)
    {
        //if the voxel to check is not transparent
        if (!isTransparent)
        {
            //if the neighbour position is outside of the world, dont draw the face
            if (position.x < 0 || position.x > mapSize - 1 || position.z < 0 || position.z > mapSize - 1)
            {
                return false;
            }

            int xCoord = position.x / ChunkData.chunkWidth;
            int zCoord = position.z / ChunkData.chunkWidth;
            bool neighbourIsTransparent;

            if (chunks[xCoord, zCoord] != null && chunks[xCoord, zCoord].IsMapCreated)
            {
                neighbourIsTransparent = data.voxelTypes[chunks[xCoord, zCoord].GetVoxel(position)].isTransparent;
            }
            else
            {
                neighbourIsTransparent = data.voxelTypes[GetVoxelType(position)].isTransparent;
            }

            if (neighbourIsTransparent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        if (isTransparent)
        {
            if (position.x < 0 || position.x > mapSize - 1 || position.z < 0 || position.z > mapSize - 1)
            {
                return false;
            }

            int xCoord = position.x / ChunkData.chunkWidth;
            int zCoord = position.z / ChunkData.chunkWidth;
            bool neighbourIsVisible;

            if (chunks[xCoord, zCoord] != null && chunks[xCoord, zCoord].IsMapCreated)
            {
                neighbourIsVisible = data.voxelTypes[chunks[xCoord, zCoord].GetVoxel(position)].isVisible;
            }
            else
            {
                neighbourIsVisible = data.voxelTypes[GetVoxelType(position)].isVisible;
            }

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

    //Get chunk by position
    public Chunk GetChunk(Vector3Int position)
    {
        int xChunk = position.x / ChunkData.chunkWidth;
        int zChunk = position.z / ChunkData.chunkWidth;

        if (!CheckChunkInMap(new ChunkCoord(xChunk, zChunk)))
        {
            return null;
        }

        return chunks[xChunk, zChunk];
    }

    //Get ChunkCoord by position
    ChunkCoord GetChunkCoord(Vector3 position)
    {
        int xChunk = Mathf.FloorToInt(position.x / ChunkData.chunkWidth);
        int zChunk = Mathf.FloorToInt(position.z / ChunkData.chunkWidth);

        if (!CheckChunkInMap(new ChunkCoord(xChunk, zChunk)))
        {
            return null;
        }

        return new ChunkCoord(xChunk, zChunk);
    }

    //Get the ground voxel types by position
    public int GetVoxelType(Vector3Int position)
    {
        if (position.y == 0 && heightMap[position.x, position.z] == 0)
        {
            return 1;
        }
        else if (position.y == 0 && heightMap[position.x, position.z] > 0)
        {
            return 3;
        }

        // SURFACE
        int currentHeight = heightMap[position.x, position.z];
        int currentType = 0;

        if (position.y == currentHeight)
        {
            for (int i = 0; i < data.surfaces[data.selectedSurface].surfaceVoxels.Length; i++)
            {
                int surfaceHeight = Mathf.FloorToInt(data.heightMultiplier * (data.surfaces[data.selectedSurface].surfaceVoxels[i].height) - 0.01f);

                if (currentHeight <= surfaceHeight)
                {
                    int randomType = Random.Range(0, data.surfaces[data.selectedSurface].surfaceVoxels[i].voxelTypeIndexes.Length);

                    currentType = data.surfaces[data.selectedSurface].surfaceVoxels[i].voxelTypeIndexes[randomType];
                    break;
                }
            }
        }
        else if (position.y <= waterLevel && position.y > currentHeight)
        {
            return 1;
        }
        else if (position.y > currentHeight)
        {
            return 0;
        }
        else currentType = 10;

        return currentType;
    }

    //Create chunks on load
    void CreateChunksOnLoad()
    {
        while (chunksToCreateVoxelMap.Count > 0)
        {
            chunks[chunksToCreateVoxelMap[0].x, chunksToCreateVoxelMap[0].z].CreateVoxelMap();
            chunksToCreateVoxelMap.RemoveAt(0);
        }

        while (chunksToCreateMesh.Count > 0)
        {
            if (!chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].IsMeshCreated)
            {
                chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].CreateChunkObject();
                chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].IsMeshCreated = true;
            }

            chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].UpdateMeshData();
            chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].UpdateMesh();

            /*if (chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions.Count > 0)
            {
                for (int i = 0; i < chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions.Count; i++)
                {
                    Instantiate(enemy, chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions[i], Quaternion.identity);
                    chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions.RemoveAt(i);
                }
            }*/

            chunksToCreateMesh.RemoveAt(0);
        }

        player = Instantiate(player, spawnPosition, Quaternion.identity);
    }

    //Create voxel map for chunks from list
    IEnumerator CreateMap()
    {
        isCreatingVoxelMap = true;

        while (chunksToCreateVoxelMap.Count > 0)
        {
            chunks[chunksToCreateVoxelMap[0].x, chunksToCreateVoxelMap[0].z].CreateVoxelMap();
            chunksToCreateVoxelMap.RemoveAt(0);
            yield return null;
        }

        isCreatingVoxelMap = false;
    }

    //create mesh for chunks from list
    IEnumerator CreateMesh()
    {
        isCreatingMesh = true;

        while (chunksToCreateMesh.Count > 0)
        {
            if (!chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].IsMeshCreated)
            {
                chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].CreateChunkObject();
                chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].IsMeshCreated = true;
            }

            chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].UpdateMeshData();
            chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].UpdateMesh();
            /*
            if(chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions.Count > 0)
            {
                for(int i = 0; i < chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions.Count; i++)
                {
                    Instantiate(enemy, chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions[i], Quaternion.identity);
                    chunks[chunksToCreateMesh[0].x, chunksToCreateMesh[0].z].enemySpawnPositions.RemoveAt(i);
                }
            }*/

            chunksToCreateMesh.RemoveAt(0);
            yield return null;
        }

        isCreatingMesh = false;
    }

    //Flow water to empty positions
    IEnumerator WaterFlow()
    {
        isWaterFlowing = true;
        List<Vector3Int> positions = new List<Vector3Int>(waterToFlow);
        List<ChunkCoord> chunkUpdate = new List<ChunkCoord>();
        waterToFlow.Clear();

        yield return new WaitForSeconds(0.5f);

        foreach (Vector3Int p in positions)
        {
            ChunkCoord cc = GetChunkCoord(p);
            chunks[cc.x, cc.z].SetVoxel(p, 1);

            if (!cc.Exist(chunkUpdate))
            {
                chunkUpdate.Add(cc);
            }

            for (int i = 0; i < 6; i++)
            {
                ChunkCoord ccn = GetChunkCoord(p + ChunkData.voxelFaceChecks[i]);
                Vector3Int pn = p + ChunkData.voxelFaceChecks[i];

                if (chunks[ccn.x, ccn.z].GetVoxel(pn) == 0 && pn.y <= waterLevel)
                {
                    waterToFlow.Add(pn);
                }
            }
        }

        foreach (ChunkCoord cc in chunkUpdate)
        {
            chunks[cc.x, cc.z].UpdateMeshData();
            chunks[cc.x, cc.z].UpdateMesh();
        }

        isWaterFlowing = false;
    }

    //Generate separated voxels
    public void GenerateVoxels(List<Vector3Int> positions, ChunkCoord chunkCoord)
    {
        foreach(Vector3Int p in positions)
        {
            int type = chunks[chunkCoord.x, chunkCoord.z].GetVoxel(p);
            chunks[chunkCoord.x, chunkCoord.z].SetVoxel(p, 0);
            float randomTime = Random.Range(1f, 3f);
            StartCoroutine(GenerateVoxel(p, type, randomTime));
        }
    }

    //Generate separated voxel object
    IEnumerator GenerateVoxel(Vector3Int position, int type, float waitTime)
    {
        Voxel voxel = new Voxel(position, type);
        
        yield return new WaitForSeconds(waitTime);

        voxel.DestroyVoxel();
    }
}