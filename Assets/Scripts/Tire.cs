using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tire : MonoBehaviour
{
    public Transform model;

    public float rotateSpeed = 5;
    public float tiltAmount = 1;

    float tilt;
    float speed;

    void Start()
    {

    }

    void Update()
    {
        model.Rotate(Vector3.right, speed * rotateSpeed * Time.deltaTime, Space.Self);
        transform.localRotation = Quaternion.AngleAxis(tilt * tiltAmount, Vector3.up);
    }

    public void SetTilt(float tilt)
    {
        this.tilt = tilt;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
