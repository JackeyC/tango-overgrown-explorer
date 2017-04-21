using UnityEngine;
using UnityEngine.UI;

public class TapToPlace : MonoBehaviour
{
    public GameObject[] m_objects = new GameObject[6];
    public Vector3 scale;
    public Text message;
    public RectTransform m_prefabTouchEffect;
    public Canvas m_canvas;
    private TangoPointCloud m_pointCloud;

    //int m_pointsCount;

    void Start()
    {
        m_pointCloud = FindObjectOfType<TangoPointCloud>();
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            // Trigger place object function when single touch ended.
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended)
            {
                RectTransform touchEffectRectTransform = Instantiate(m_prefabTouchEffect) as RectTransform;
                touchEffectRectTransform.transform.SetParent(m_canvas.transform, false);
                Vector2 normalizedPosition = t.position;
                normalizedPosition.x /= Screen.width;
                normalizedPosition.y /= Screen.height;
                touchEffectRectTransform.anchorMin = touchEffectRectTransform.anchorMax = normalizedPosition;
                PlaceObject(t.position);
            }
        }
    }


    void PlaceObject(Vector2 touchPosition)
    {
        
        message.text = "";
        // Find the plane.
        Camera cam = Camera.main;
        Vector3 planeCenter;
        Plane plane;
        if (!m_pointCloud.FindPlane(cam, touchPosition, out planeCenter, out plane))
        {
            message.text = "cannot find plane.";
            Debug.Log("cannot find plane.");
            return;
        }

        Vector3 up = plane.normal;

        int rotation = Random.Range(0, 360);

        // Place object on the surface with random rotation.
        if (Vector3.Angle(plane.normal, Vector3.up) < 5.0f)
        {
            instantiateObject(m_objects.Length, planeCenter, rotation);
        }
        else if (Vector3.Angle(plane.normal, Vector3.up) < 10.0f)
        {
            instantiateObject(m_objects.Length - 1, planeCenter, rotation);
        }
        else if (Vector3.Angle(plane.normal, Vector3.up) < 50.0f)
        {
            instantiateObject(5, planeCenter, rotation);
        }
        else
        {
            message.text = "surface is too steep for object to stand on.";
            Debug.Log("surface is too steep for object to stand on.");
        }
    }

    void instantiateObject(int range, Vector3 coords, int angle)
    {
        var instantiatedObject = Instantiate(m_objects[Random.Range(0, range)], coords, Quaternion.Euler(0, angle, 0)) as GameObject;
        instantiatedObject.transform.localScale = scale;
        ARMarker markerScript = instantiatedObject.GetComponent<ARMarker>();
    }
}