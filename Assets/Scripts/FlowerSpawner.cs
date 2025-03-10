using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class FlowerManager : MonoBehaviour
{
    [Header("Prefabs and Materials")]
    public GameObject flowerPrefab;
    public Material[] petalMaterials;

    [Header("Poisson Disk Distribution")]
    public float minDistance = 2f;
    public float spawnRadius = 10f;
    public int k = 30; // Number of samples to generate per point in Poisson Disk

    [Header("Retry Settings")]
    public int maxSpawnRetries = 10; // Maximum retries to find a valid spawn location

    private Camera mainCamera;
    private List<Vector3> spawnedFlowerPositions = new List<Vector3>(); // Keep track of spawned flower positions

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Make sure you have a camera tagged as 'MainCamera' in your scene.");
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GenerateAndSpawnFlowerWithRetry(); // Use the retry version
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
    }

    void GenerateAndSpawnFlowerWithRetry()
    {
        int retryCount = 0;
        bool spawned = false;

        while (retryCount < maxSpawnRetries && !spawned)
        {
            List<Vector3> flowerPositions = GeneratePoissonDiskPositions(spawnedFlowerPositions);
            if (flowerPositions.Count > 0) // Valid position found
            {
                SpawnFlowers(flowerPositions);
                spawned = true; // Set spawned to true to exit the loop
            }
            else
            {
                retryCount++;
                Debug.Log("Flower spawn failed, retrying... Attempt: " + retryCount);
            }
        }

        if (!spawned)
        {
            Debug.LogWarning("Max spawn retries reached. Could not find a valid position to spawn a flower.");
        }
    }


    List<Vector3> GeneratePoissonDiskPositions(List<Vector3> existingFlowerPositions)
    {
        List<Vector3> finalPoints = new List<Vector3>();
        List<Vector3> activePoints = new List<Vector3>();

        // Initialize the first point randomly within the spawn radius
        Vector2 initialPoint2D = Random.insideUnitCircle * spawnRadius;
        Vector3 initialPoint = new Vector3(initialPoint2D.x, 0f, initialPoint2D.y);

        if (IsPointInViewport(initialPoint) && IsFarEnough(initialPoint, existingFlowerPositions)) // Check against existing flowers
        {
            finalPoints.Add(initialPoint);
            activePoints.Add(initialPoint);
        }

        while (activePoints.Count > 0 && finalPoints.Count < 1) // Only try to find one point
        {
            int randomIndex = Random.Range(0, activePoints.Count);
            Vector3 currentPoint = activePoints[randomIndex];
            bool foundNewPoint = false;

            for (int i = 0; i < k; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = Random.Range(minDistance, 2f * minDistance);
                Vector3 sampleOffset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 candidatePoint = currentPoint + sampleOffset;

                if (candidatePoint.magnitude <= spawnRadius && IsPointInViewport(candidatePoint) && IsFarEnough(candidatePoint, existingFlowerPositions)) // Check against existing flowers
                {
                    finalPoints.Add(candidatePoint);
                    activePoints.Add(candidatePoint);
                    foundNewPoint = true;
                    break; // Found a valid point, break to spawn just one flower
                }
            }

            if (!foundNewPoint)
            {
                activePoints.RemoveAt(randomIndex); // Remove point if no valid samples found around it
            }
        }

        return finalPoints; // Will return a list with 0 or 1 positions
    }

    // Modified IsFarEnough to take a List<Vector3> as existing points
    bool IsFarEnough(Vector3 candidate, List<Vector3> existingPoints)
    {
        foreach (Vector3 point in existingPoints)
        {
            if (Vector3.Distance(candidate, point) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    bool IsPointInViewport(Vector3 worldPosition)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f && viewportPoint.z > 0; // z > 0 to ensure it's in front of the camera
    }

    void SpawnFlowers(List<Vector3> positions)
    {
        if (positions.Count == 0) return; // No position found, don't spawn

        foreach (Vector3 pos in positions)
        {
            GameObject flowerInstance = Instantiate(flowerPrefab, pos, Quaternion.identity, transform);

            // Random Petal Material for EACH flower
            Flower flowerScript = flowerInstance.GetComponent<Flower>();
            if (flowerScript != null && petalMaterials.Length > 0)
            {
                int randomIndex = Random.Range(0, petalMaterials.Length);
                Material selectedMaterial = petalMaterials[randomIndex];
                flowerScript.SetPetalMaterial(selectedMaterial); // Call the method to set material in Flower script
            }
            else
            {
                Debug.LogWarning("Flower script not found on instantiated flower, or no petal materials assigned.");
            }
            spawnedFlowerPositions.Add(pos); // Add the new flower position to the list
        }
    }

    void ReloadScene()
    {
        spawnedFlowerPositions.Clear(); // Clear the list of spawned flower positions on reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}