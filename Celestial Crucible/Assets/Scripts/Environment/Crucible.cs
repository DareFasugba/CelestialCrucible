using UnityEngine;

public class Crucible : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CrucibleManager.Instance.AddCrucible();
            Destroy(gameObject);
        }
    }
}

