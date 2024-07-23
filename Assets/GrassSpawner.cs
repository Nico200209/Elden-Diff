using UnityEngine;
using System.Collections.Generic;

public class GrassSpawner : MonoBehaviour
{
    public GameObject grassPrefab; // The grass prefab to spawn
    public LayerMask whatIsGround; // Layer mask for the terrain
    public float spawnRadius = 5.0f; // The radius within which to spawn grass
    public float eraserRadius = 3.0f; // The radius for the eraser tool
    public int density = 10; // Number of grass prefabs to spawn
    public Transform grassParent; // Parent transform for grass instances

    public bool randomRotation = true; // Toggle for random rotation
    public bool randomScale = true; // Toggle for random scale
    public float minScale = 0.5f; // Minimum scale value
    public float maxScale = 2.0f; // Maximum scale value

    private List<Vector3> savedGrassPositions = new List<Vector3>(); // List to store positions of placed grass

    void Start()
    {
        LoadSavedGrassPositions(); // Load saved grass positions on game start
        SpawnSavedGrass(); // Spawn grass at saved positions
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click to add grass
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, whatIsGround))
            {
                SpawnGrass(hit.point);
                SaveGrassPositions(); // Save grass positions after placing new grass
            }
        }

        if (Input.GetMouseButton(1)) // Right mouse button held down to remove grass
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, whatIsGround))
            {
                RemoveGrass(hit.point, eraserRadius);
                SaveGrassPositions(); // Save grass positions after removing grass
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) // Toggle random rotation with 'R' key
        {
            randomRotation = !randomRotation;
        }

        if (Input.GetKeyDown(KeyCode.S)) // Toggle random scale with 'S' key
        {
            randomScale = !randomScale;
        }
    }

    void SpawnGrass(Vector3 center)
    {
        for (int i = 0; i < density; i++)
        {
            Vector3 randomPos = GetRandomPosition(center, spawnRadius);
            Vector3 terrainPos = GetTerrainPosition(randomPos);
            if (terrainPos != Vector3.zero) // Ensure valid terrain position
            {
                GameObject grass = Instantiate(grassPrefab, terrainPos, Quaternion.identity);
                grass.transform.SetParent(grassParent); // Set parent explicitly
                AdjustGrass(grass);
                savedGrassPositions.Add(grass.transform.position); // Add position to saved list
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
            GameObject grass = Instantiate(grassPrefab, position, Quaternion.identity);
            grass.transform.SetParent(grassParent); // Set parent explicitly
            AdjustGrass(grass);
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
            float heightOffset = bounds.extents.y; // Get the half height of the prefab
            grass.transform.position = new Vector3(hit.point.x, hit.point.y + heightOffset, hit.point.z);

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
            Destroy(grass.gameObject);
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
