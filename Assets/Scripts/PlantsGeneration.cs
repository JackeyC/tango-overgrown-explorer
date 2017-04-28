using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlantsGeneration : MonoBehaviour
{
    public GameObject[] m_objects = new GameObject[6];
    public Vector3 scale;
    public bool plantsGeneration = true;

    private TangoPointCloud m_pointCloud;

    Vector2 position;

    int row;
    int col;
    Vector3 worldPosition;

    //int m_pointsCount;

    void Start()
    {
        m_pointCloud = FindObjectOfType<TangoPointCloud>();
    }

    void Update()
    {
        if (plantsGeneration)
        {
            position = new Vector2(RandomFromDistribution.RandomRangeNormalDistribution(0, Screen.width, RandomFromDistribution.ConfidenceLevel_e._999), RandomFromDistribution.RandomRangeNormalDistribution(0, Screen.height, RandomFromDistribution.ConfidenceLevel_e._999));
            PlaceObject(position);
        }
    }


    void PlaceObject(Vector2 position)
    {
        // Find the plane.
        Camera cam = Camera.main;
        Vector3 planeCenter;
        Plane plane;
        if (m_pointCloud.FindPlane(cam, position, out planeCenter, out plane))
        {
            Vector3 up = plane.normal;
            Vector3 right = Vector3.Cross(plane.normal, cam.transform.forward).normalized;
            Vector3 forward = Vector3.Cross(right, plane.normal).normalized;

            // Place object on the surface, and make it always face the camera.
            if (Vector3.Angle(plane.normal, Vector3.up) < 10.0f)
            {
                if (!Physics.CheckSphere(position, 100))
                {
                    instantiateObject(m_objects.Length, planeCenter, forward, up);
                }
            }
            else if (Vector3.Angle(plane.normal, Vector3.up) < 50.0f)
            {
                instantiateObject(6, planeCenter, forward, up);
            }
            else
            {
                Debug.Log("surface is too steep for object to stand on.");
            }
        }
    }

    void instantiateObject(int range, Vector3 coords, Vector3 forward, Vector3 up)
    {
        var instantiatedObject = Instantiate(m_objects[Random.Range(0, range)], coords, Quaternion.LookRotation(forward, up)) as GameObject;
        instantiatedObject.transform.localScale = scale;
    }

    public void OnGUI()
    {
        // Toggle Plants Generation
        if (GUI.Button(new Rect(10, Screen.height - 110, 600, 100), "<size=40>Toggle Plants Seeding</size>"))
        {
            plantsGeneration = !plantsGeneration;
        }

        // Destroy Plants
        if (GUI.Button(new Rect(10, Screen.height - 220, 400, 100), "<size=40>Destroy Plants</size>"))
        {
            var plantsToDestroy = GameObject.FindGameObjectsWithTag("Plants");
            foreach (var plant in plantsToDestroy)
            {
                Destroy(plant);
            }
        }
    }
}
