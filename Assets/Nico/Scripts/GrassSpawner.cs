using UnityEngine;
using System.Collections.Generic;

public class GrassSpawner : MonoBehaviour
{
    [Header("Grass Prefab Settings")]
    public GameObject grassPrefab; // The grass prefab to spawn
    public Transform grassParent; // Parent transform for grass instances
    public LayerMask whatIsGround; // Layer mask for the terrain
    public float heightOffset = 0.0f; // Vertical offset for the grass placement

    [Header("Grass Spawning Settings")]
    public float spawnRadius = 5.0f; // The radius within which to spawn grass
    public int density = 10; // Number of grass prefabs to spawn
    public float maxDistance = 50f; // Maximum distance for grass to be active

    [Header("Eraser Tool Settings")]
    public float eraserRadius = 3.0f; // The radius for the eraser tool
    public bool densityReductionMode = false; // Toggle for density reduction mode
    [Range(0f, 1f)]
    public float densityReductionFactor = 0.5f; // Factor for density reduction (0 to 1)

    [Header("Grass Appearance Settings")]
    public bool randomRotation = true; // Toggle for random rotation
    public bool randomScale = true; // Toggle for random scale
    public float minScale = 0.5f; // Minimum scale value
    public float maxScale = 2.0f; // Maximum scale value

    [Header("Keybindings")]
    public KeyCode spawnKey = KeyCode.Mouse0; // Key to spawn grass
    public KeyCode removeKey = KeyCode.Mouse1; // Key to remove grass
    public KeyCode toggleRotationKey = KeyCode.R; // Key to toggle random rotation
    public KeyCode toggleScaleKey = KeyCode.S; // Key to toggle random scale
    public KeyCode toggleDensityReductionKey = KeyCode.D; // Key to toggle density reduction mode

    private List<Transform> grassPool = new List<Transform>(); // Pool of grass objects
    private List<Vector3> savedGrassPositions = new List<Vector3>(); // List to store positions of placed grass
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        LoadSavedGrassPositions(); // Load saved grass positions on game start
        InitializeGrassPool(); // Initialize grass object pool
        SpawnSavedGrass(); // Spawn grass at saved positions
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey)) // Key to add grass
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, whatIsGround))
            {
                SpawnGrass(hit.point);
                SaveGrassPositions(); // Save grass positions after placing new grass
            }
        }

        if (Input.GetKey(removeKey)) // Key held down to remove grass
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, whatIsGround))
            {
                if (densityReductionMode)
                {
                    ReduceDensity(hit.point, eraserRadius);
                }
                else
                {
                    RemoveGrass(hit.point, eraserRadius);
                }
                SaveGrassPositions(); // Save grass positions after modifying grass
            }
        }

        if (Input.GetKeyDown(toggleRotationKey)) // Toggle random rotation
        {
            randomRotation = !randomRotation;
        }

        if (Input.GetKeyDown(toggleScaleKey)) // Toggle random scale
        {
            randomScale = !randomScale;
        }

        if (Input.GetKeyDown(toggleDensityReductionKey)) // Toggle density reduction mode
        {
            densityReductionMode = !densityReductionMode;
        }

        UpdateGrassVisibility();
    }

    void SpawnGrass(Vector3 center)
    {
        for (int i = 0; i < density; i++)
        {
            Vector3 randomPos = GetRandomPosition(center, spawnRadius);
            Vector3 terrainPos = GetTerrainPosition(randomPos);
            if (terrainPos != Vector3.zero) // Ensure valid terrain position
            {
                Transform grass = GetPooledGrass();
                grass.position = terrainPos;
                grass.gameObject.SetActive(true);
                AdjustGrass(grass.gameObject);
                savedGrassPositions.Add(grass.position); // Add position to saved list
            }
        }
    }

    void SaveGrassPositions()
    {
        // Convert List<Vector3> to JSON string
        string grassPositionsJson = JsonHelper.ToJson(savedGrassPositions.ToArray());
        PlayerPrefs.SetString("GrassPositions", grassPositionsJson);
        PlayerPrefs.Save();
    }

    void LoadSavedGrassPositions()
    {
        // Load JSON string from PlayerPrefs
        string grassPositionsJson = PlayerPrefs.GetString("GrassPositions", "");
        if (!string.IsNullOrEmpty(grassPositionsJson))
        {
            // Deserialize JSON string back to Vector3 array
            Vector3[] positionsArray = JsonHelper.FromJson<Vector3>(grassPositionsJson);
            savedGrassPositions = new List<Vector3>(positionsArray);
        }
    }

    void SpawnSavedGrass()
    {
        foreach (Vector3 position in savedGrassPositions)
        {
            Transform grass = GetPooledGrass();
            grass.position = position;
            grass.gameObject.SetActive(true);
            AdjustGrass(grass.gameObject);
        }
    }

    Vector3 GetRandomPosition(Vector3 center, float radius)
    {
        float x = Random.Range(-radius, radius);
        float z = Random.Range(-radius, radius);
        return new Vector3(center.x + x, center.y, center.z + z);
    }

    Vector3 GetTerrainPosition(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(position.x, position.y + 100, position.z), Vector3.down, out hit, Mathf.Infinity, whatIsGround))
        {
            return hit.point;
        }
        return Vector3.zero; // Return zero vector if no hit
    }

    void AdjustGrass(GameObject grass)
    {
        // Perform a raycast from above the grass prefab down to find the terrain
        RaycastHit hit;
        Vector3 start = new Vector3(grass.transform.position.x, grass.transform.position.y + 100, grass.transform.position.z);

        if (Physics.Raycast(start, Vector3.down, out hit, Mathf.Infinity, whatIsGround))
        {
            Bounds bounds = grass.GetComponent<Renderer>().bounds;
            float prefabHeightOffset = bounds.extents.y; // Get the half height of the prefab
            grass.transform.position = new Vector3(hit.point.x, hit.point.y + prefabHeightOffset + heightOffset, hit.point.z);

            // Apply random rotation if enabled
            if (randomRotation)
            {
                grass.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
            }

            // Apply random scale if enabled
            if (randomScale)
            {
                float scale = Random.Range(minScale, maxScale);
                grass.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    void RemoveGrass(Vector3 center, float radius)
    {
        List<Transform> grassToRemove = new List<Transform>();

        foreach (Transform grass in grassParent)
        {
            if (Vector3.Distance(grass.position, center) <= radius)
            {
                grassToRemove.Add(grass);
            }
        }

        foreach (Transform grass in grassToRemove)
        {
            savedGrassPositions.Remove(grass.position);
            grass.gameObject.SetActive(false); // Deactivate instead of destroying
        }
    }

    void ReduceDensity(Vector3 center, float radius)
    {
        List<Transform> grassWithinRadius = new List<Transform>();

        foreach (Transform grass in grassParent)
        {
            if (Vector3.Distance(grass.position, center) <= radius)
            {
                grassWithinRadius.Add(grass);
            }
        }

        int grassToRemoveCount = Mathf.FloorToInt(grassWithinRadius.Count * densityReductionFactor); // Reduce density based on factor

        for (int i = 0; i < grassToRemoveCount; i++)
        {
            int randomIndex = Random.Range(0, grassWithinRadius.Count);
            Transform grass = grassWithinRadius[randomIndex];
            grassWithinRadius.RemoveAt(randomIndex);
            savedGrassPositions.Remove(grass.position);
            grass.gameObject.SetActive(false); // Deactivate instead of destroying
        }
    }

    void InitializeGrassPool()
    {
        for (int i = 0; i < density * 10; i++)
        {
            GameObject grass = Instantiate(grassPrefab, grassParent);
            grass.SetActive(false);
            grassPool.Add(grass.transform);
        }
    }

    Transform GetPooledGrass()
    {
        foreach (Transform grass in grassPool)
        {
            if (!grass.gameObject.activeInHierarchy)
            {
                return grass;
            }
        }

        // If no inactive grass is available, instantiate a new one
        GameObject newGrass = Instantiate(grassPrefab, grassParent);
        newGrass.SetActive(false);
        grassPool.Add(newGrass.transform);
        return newGrass.transform;
    }

    void UpdateGrassVisibility()
    {
        Vector3 playerPosition = mainCamera.transform.position;
        foreach (Transform grass in grassParent)
        {
            float distance = Vector3.Distance(playerPosition, grass.position);
            grass.gameObject.SetActive(distance <= maxDistance);
        }
    }

    // Helper class for JSON serialization/deserialization of arrays
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}
