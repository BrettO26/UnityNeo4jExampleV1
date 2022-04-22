using UnityEngine;

public class RotateWithNeo4j : MonoBehaviour
{
    public Vector3 Rotate;
    void Update()
    {
        transform.Rotate(Rotate * Time.deltaTime * PullNeo4j.instance.DataOut);
    }
}