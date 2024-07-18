using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    public GameObject[] flowerPrefabs; // Array of flower prefabs to spawn
    public float spawnRadius = 5f; // Radius within which to spawn the flowers
    public int minFlowers = 5; // Minimum number of flowers to spawn
    public int maxFlowers = 15; // Maximum number of flowers to spawn
    public LayerMask terrainLayerMask; // Layer mask to specify the terrain layer
    private bool isAddingMode = true; // Flag to toggle between adding and removing flowers

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
            {
                if (isAddingMode)
                {
                    SpawnFlowers(hit.point);
                }
                else
                {
                    RemoveFlowers(hit.point);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isAddingMode = !isAddingMode;
        }
    }

    void SpawnFlowers(Vector3 position)
    {
        int flowerCount = Random.Range(minFlowers, maxFlowers);

        for (int i = 0; i < flowerCount; i++)
        {
            Vector3 randomPosition = position + Random.insideUnitSphere * spawnRadius;
            randomPosition.y = position.y + 10f; // Start the raycast from above the terrain

            if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayerMask))
            {
                randomPosition.y = hit.point.y; // Set y to the exact terrain height

                GameObject flowerPrefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
                Instantiate(flowerPrefab, randomPosition, Quaternion.identity);
            }
        }
    }

    void RemoveFlowers(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, spawnRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Flower")) // Ensure the collider has the "Flower" tag
            {
                Destroy(collider.gameObject);
            }
        }
    }
}
