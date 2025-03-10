using UnityEngine;

public class Flower : MonoBehaviour
{
    [Header("Prefab and Count")]
    public GameObject petalPrefab;
    public int numberOfPetals = 34; // Set to 34 as requested

    [Header("Layered Phyllotaxis Parameters")]
    [Tooltip("Number of layers in the phyllotaxis pattern.")]
    public int numberOfLayers = 2; // Set to 2 layers as requested
    [Tooltip("Radii for each layer, define in increasing order. Size should be equal to numberOfLayers.")]
    public float[] layerRadii;
    [Tooltip("Number of petals for each layer, define for each layer. Size should be equal to numberOfLayers.")]
    public int[] petalsPerLayerCount; // New array for petal count per layer
    [Tooltip("The angle in degrees for phyllotaxis within each layer, Golden Angle is approximately 137.5 degrees.")]
    [Range(0f, 360f)]
    public float angleDegrees = 137.5f; // Golden Angle
    [Tooltip("Scales the angle increment within each layer.")]
    public float angleScale = 1f; // Scale the angle increment
    [Tooltip("Offsets the starting angle for the first petal.")]
    public float startAngleOffsetDegrees = 0f;

    [Header("Layer 1 Specific Settings")]
    [Tooltip("Vertical offset for petals in Layer 1.")]
    public float layer1VerticalOffset = 0.1f;
    [Tooltip("Upward angle in degrees for petals in Layer 1.")]
    [Range(0f, 90f)]
    public float layer1UpwardAngle = 15f; // Example upward angle

    [Header("Optional Rotation")]
    public bool rotateFlower = false;
    [Tooltip("Rotation speed around the central axis.")]
    public float rotationSpeed = 10f;

    private Material currentPetalMaterial; // Store the material to be applied

    public void SetPetalMaterial(Material material)
    {
        currentPetalMaterial = material;
    }

    private void Start()
    {
        GrowFlower();
    }

    void GrowFlower()
    {
        if (petalPrefab == null)
        {
            Debug.LogError("Petal Prefab is not assigned! Please assign a prefab in the Inspector.");
            return;
        }

        if (layerRadii == null || layerRadii.Length != numberOfLayers)
        {
            Debug.LogError("Layer Radii array is not properly configured. Ensure it has " + numberOfLayers + " elements.");
            return;
        }

        if (petalsPerLayerCount == null || petalsPerLayerCount.Length != numberOfLayers)
        {
            Debug.LogError("Petals Per Layer Count array is not properly configured. Ensure it has " + numberOfLayers + " elements.");
            return;
        }

        int petalCount = 0; // Keep track of total petals instantiated

        for (int layerIndex = 0; layerIndex < numberOfLayers; layerIndex++)
        {
            float currentLayerRadius = layerRadii[layerIndex];
            int currentLayerPetalCount = petalsPerLayerCount[layerIndex];

            for (int i = 0; i < currentLayerPetalCount; i++)
            {
                petalCount++;

                float angleRadians = (startAngleOffsetDegrees + (petalCount - 1) * angleDegrees * angleScale) * Mathf.Deg2Rad;

                float x = currentLayerRadius * Mathf.Cos(angleRadians);
                float z = currentLayerRadius * Mathf.Sin(angleRadians);

                Vector3 petalPosition = new Vector3(x, 0f, z);
                Quaternion petalRotation = new Quaternion();

                // Apply Layer 1 specific vertical offset and upward rotation
                if (layerIndex == 0) // Layer 1 is index 0
                {
                    petalPosition.y += layer1VerticalOffset; // Vertical offset

                    // Apply upward rotation around local X-axis AFTER outward facing rotation
                    Quaternion upwardRotation = Quaternion.Euler(-layer1UpwardAngle, 0f, 0f); // Negative angle to rotate upwards around local X-axis
                    Quaternion petalLookRotation = Quaternion.LookRotation(petalPosition - Vector3.zero); // Recalculate look rotation with potentially changed position
                    Quaternion basePetalRotation = petalLookRotation * Quaternion.Euler(90f, 0f, 0f); // Base outward rotation
                    petalRotation = basePetalRotation * upwardRotation; // Apply upward rotation after base rotation
                }
                else
                {
                    // For other layers (Layer 2 and beyond), use the standard outward facing rotation
                    Quaternion petalLookRotation = Quaternion.LookRotation(petalPosition - Vector3.zero);
                    petalRotation = petalLookRotation * Quaternion.Euler(90f, 0f, 0f);
                }

                GameObject petalInstance = Instantiate(petalPrefab, Vector3.zero, petalRotation, transform); // Instantiate at local zero, then set localPosition
                petalInstance.transform.localPosition = petalPosition; // Explicitly set localPosition

                // Apply material to the instantiated petal
                if (currentPetalMaterial != null)
                {
                    MeshRenderer[] petalRenderers = petalInstance.GetComponentsInChildren<MeshRenderer>();
                    if (petalRenderers.Length > 0)
                    {
                        foreach (MeshRenderer petalRenderer in petalRenderers)
                        {
                            petalRenderer.material = currentPetalMaterial;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Petal prefab in Flower script does not have any MeshRenderers in its hierarchy.");
                    }
                }
            }
        }

        // Optionally rotate the entire flower
        if (rotateFlower)
        {
            transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.time);
        }
    }
}