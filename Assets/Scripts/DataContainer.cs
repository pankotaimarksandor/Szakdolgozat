using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    #region Singleton
    public static DataContainer Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [HideInInspector]
    public bool isPlaying = true;
    [HideInInspector]
    public int playerHealth = 100;
    public int enemyWaves = 4;

    [HideInInspector]
    public int difficulty = 0;

    [Header("Height curve")]
    public AnimationCurve heightCurve;
    public AnimationCurve gradientCurve;
    public bool useCurve;

    [Header("Surfaces")]
    public Surface[] surfaces;
    [HideInInspector]
    public int selectedSurface;
    [HideInInspector]
    public float waterHeight = 0.1f;

    [Header("Voxel types")]
    public VoxelType[] voxelTypes;

    [Header("Voxel materials")]
    public Material voxelGround;
    public Material voxelWater;

    [HideInInspector]
    public int textureSize = 256;
    [HideInInspector]
    public int selectedTexture = 0;
    public int textureAtlasSize = 6;

    [HideInInspector]
    public int mapBorderWidth = 256;

    [Header("Chunk settings")]
    public int chunks = 128;
    public int viewDistance = 4;

    [HideInInspector]
    public int noiseOctaves = 3;
    [HideInInspector]
    public float noiseScale = 50f;
    [HideInInspector]
    public float noisePersistance = 0.5f;
    [HideInInspector]
    public float noiseLacunarity = 2f;
    [HideInInspector]
    public int[] noiseOffsets = { 0, 0, 0, 0 };

    [Header("Structures")]
    public StructureZone[] structureZones;
    [HideInInspector]
    public bool structuresCreated;

    public int heightMultiplier = 200;
}
