 using UnityEngine;

public class FloatingCoin : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 90f;

    [Header("Floating")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.25f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate the coin
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Float up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

