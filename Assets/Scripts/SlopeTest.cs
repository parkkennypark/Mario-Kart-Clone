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
        Debug.DrawRay(model.position, -model.up * 50);
        if (Physics.Raycast(model.position, -model.up, out hit, 50))
        {
            // model.up = hit.normal;
            // model.LookAt(hit.point + hit.normal, Vector3.up);
            // model.Rotate(Vector3.up, rotation, Space.Self);
            // model.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, hit.normal));

            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            // model.rotation = Quaternion.LookRotation(Vector3.Cross(model.forward, hit.normal));

            // float xRot = Vector3.SignedAngle(Vector3.up, hit.normal, transform.right);
            // float zRot = Vector3.SignedAngle(Vector3.up, hit.normal, Vector3.up);
            // float dot = Vector3.Dot(transform.forward, hit.normal);
            // Quaternion target = Quaternion.Euler(xRot, 0, -zRot);
            // transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5);

        }
    }
}
