using UnityEngine;

public class MoveNotes : MonoBehaviour
{
    float noteSpeed = 11 * 1.6f;

    void Update()
    {
        transform.position -= transform.forward * noteSpeed * Time.deltaTime;
    }
}