using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class KartCamera : MonoBehaviour
{
    private new Camera camera;
    private float baseFOV;

    public KartController kart;
    public float fovSpeedMult = 0.5f;
    public float fovSmoothing = 8;


    void Start()
    {
        camera = GetComponent<Camera>();
        baseFOV = camera.fieldOfView;
    }

    void Update()
    {
        float ratio = kart.GetCurrentSpeed() / kart.topSpeed;
        ratio = Mathf.Clamp(ratio, 0, ratio);

        float targetFOV = baseFOV * (1 + ratio * fovSpeedMult);

        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothing);
    }
}
