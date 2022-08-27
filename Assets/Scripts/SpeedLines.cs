using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedLines : MonoBehaviour
{
    public KartController kart;
    public Image image;
    public float alphaMult = 2;

    void Update()
    {
        float ratio = kart.GetCurrentSpeed() / kart.topSpeed;
        ratio = Mathf.Clamp(ratio, 0, ratio);

        float alpha = ratio - 1;
        Color color = new Color(1, 1, 1, alpha * alphaMult);
        image.color = color;
    }
}
