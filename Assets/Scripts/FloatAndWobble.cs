using UnityEngine;

public class FloatAndWobble : MonoBehaviour {

    public float floatAmplitude = 0.05f;
    public float floatFrequency = 0.5f;

    public float wobbleAmplitude = 5;
    public float wobbleFrequency = 0.5f;

	void Update () {
        // Up/Down floating
        transform.localPosition = Vector3.up * floatAmplitude * Mathf.Sin(floatFrequency * Time.time);

        // Left/Right wobbling
        transform.localRotation = Quaternion.Euler(0, 0, wobbleAmplitude * Mathf.Sin(wobbleFrequency * Time.time));
    }
}
