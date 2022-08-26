using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sparks : MonoBehaviour
{
    public SpriteRenderer mainSparks, innerSparks;

    public Color[] mainColors;
    public Color[] innerColors;

    public float[] scaleAmounts;
    public float scaleSpeed;
    public AnimationCurve scaleCurve;

    private float lastScaleAmt;
    private float currentScaleTarget;
    private float timeSinceLevelSet;

    public void SetSparkLevel(int level)
    {
        lastScaleAmt = currentScaleTarget;
        timeSinceLevelSet = 0;

        if (level == 0)
        {
            currentScaleTarget = 0;
            return;
        }

        mainSparks.color = mainColors[level - 1];
        innerSparks.color = innerColors[level - 1];

        currentScaleTarget = scaleAmounts[level - 1];
    }

    void Update()
    {
        timeSinceLevelSet += Time.deltaTime;
        float value = scaleCurve.Evaluate(timeSinceLevelSet * scaleSpeed);
        // float scale = Mathf.Lerp(lastScaleAmt, currentScaleTarget, value);
        float scale = lastScaleAmt + (currentScaleTarget - lastScaleAmt) * value;
        scale = Mathf.Clamp(scale, 0, scale);
        transform.localScale = Vector3.one * scale;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetSparkLevel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetSparkLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetSparkLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetSparkLevel(3);
    }
}
