using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeTest : MonoBehaviour
{
    public Transform model;
    public float rotation;

    void Start()
    {

    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50))
        {
            // model.up = hit.normal;
            // model.LookAt(hit.point + hit.normal, Vector3.up);
            // model.Rotate(Vector3.up, rotation, Space.Self);
            model.rotation = Quaternion.LookRotation(Vector3.Cross(model.right, hit.normal));
            // model.rotation = Quaternion.LookRotation(Vector3.Cross(model.forward, hit.normal));
        }
    }
}
