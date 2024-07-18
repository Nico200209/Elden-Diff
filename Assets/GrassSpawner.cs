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
        AdjustGrassPosition(grass, position);
    }

    void AdjustGrassPosition(GameObject grass, Vector3 position)
    {
        Bounds bounds = grass.GetComponent<Renderer>().bounds;
        float heightOffset = bounds.size.y / 2.0f; // Get half the height of the prefab
        grass.transform.position = new Vector3(position.x, position.y + heightOffset, position.z);
    }
}
