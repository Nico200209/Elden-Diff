using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject grassPrefab; // The grass prefab to spawn
    public LayerMask whatIsGround; // Layer mask for the terrain
    public float radius = 5.0f; // The radius within which to spawn grass
    public int density = 10; // Number of grass prefabs to spawn

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, whatIsGround))
            {
                SpawnGrass(hit.point);
            }
        }
    }

    void SpawnGrass(Vector3 center)
    {
        for (int i = 0; i < density; i++)
        {
            Vector3 randomPos = GetRandomPosition(center, radius);
            Vector3 terrainPos = GetTerrainPosition(randomPos);
            if (terrainPos != Vector3.zero) // Ensure valid terrain position
            {
                InstantiateGrass(terrainPos);
            }
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

    void InstantiateGrass(Vector3 position)
    {
        GameObject grass = Instantiate(grassPrefab, position, Quaternion.identity);
        AdjustGrassPosition(grass);
    }

    void AdjustGrassPosition(GameObject grass)
    {
        // Perform a raycast from the grass prefab down to find the terrain
        RaycastHit hit;
        Vector3 start = new Vector3(grass.transform.position.x, grass.transform.position.y + 100, grass.transform.position.z);

        if (Physics.Raycast(start, Vector3.down, out hit, Mathf.Infinity, whatIsGround))
        {
            Bounds bounds = grass.GetComponent<Renderer>().bounds;
            float heightOffset = bounds.extents.y; // Get the half height of the prefab
            grass.transform.position = new Vector3(hit.point.x, hit.point.y + heightOffset, hit.point.z);
        }
    }
}
