using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTXTController : MonoBehaviour
{
    public float DestroyTime = 3f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    public Vector3 randomize = new Vector3(0.5f, 0, 0);

    void Start()
    {
        Destroy(gameObject, DestroyTime);
        transform.localPosition += offset;
        transform.localPosition += new Vector3(Random.Range(-randomize.x, randomize.x), Random.Range(-randomize.y, randomize.y), Random.Range(-randomize.z, randomize.z));
    }
}
