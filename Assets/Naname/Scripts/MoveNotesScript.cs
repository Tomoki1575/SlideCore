using UnityEngine;

public class MoveNotes : MonoBehaviour
{
    private float noteSpeed = 11 * 1.6f;

    private void Update()
    {
        transform.position -= transform.forward * noteSpeed * Time.deltaTime;
    }
}