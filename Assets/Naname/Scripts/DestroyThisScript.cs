using UnityEngine;

public class DestroyThisScript : MonoBehaviour
{
    private void OnEnable()
    {
        Destroy(gameObject);
    }
}