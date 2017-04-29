using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlantsGeneration : MonoBehaviour
{
    public GameObject[] m_objects = new GameObject[6];
    //public Vector3 scale;
    public bool plantsGeneration = true;

    private TangoPointCloud m_pointCloud;
    

    void Start()
    {
        m_pointCloud = FindObjectOfType<TangoPointCloud>();
    }

    void Update()
    {
        if (plantsGeneration)
        {
            Vector2 touchPosition = new Vector2(RandomFromDistribution.RandomRangeNormalDistribution(0, Screen.width, RandomFromDistribution.ConfidenceLevel_e._999), RandomFromDistribution.RandomRangeNormalDistribution(0, Screen.height, RandomFromDistribution.ConfidenceLevel_e._999));
            PlaceObject(touchPosition);
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
            Vector3 normal = plane.normal;

            // 0 - 5 bushes
            // 6 - 9 trees
            // 10 - 33 shrooms
            // 34 - 46 florals
            // 47 vertical
            if (Vector3.Angle(normal, Vector3.up) < 10.0f)
            {
                if (!Physics.CheckSphere(planeCenter, 0.1f))
                {
                    instantiateObject(6, 10, planeCenter, normal);
                }
                else
                {
                    instantiateObject(10, m_objects.Length - 1, planeCenter, normal);
                }
            }
            else if (Vector3.Angle(normal, Vector3.up) < 30.0f)
            {
                if (!Physics.CheckSphere(planeCenter, 0.05f))
                {
                    instantiateObject(0, 6, planeCenter, normal);
                }
                else
                {
                    instantiateObject(10, m_objects.Length - 1, planeCenter, normal);
                }
            }
            else if (Vector3.Angle(normal, Vector3.up) < 60.0f)
            {
                instantiateObject(10, m_objects.Length - 1, planeCenter, normal);
            }
            else if (Vector3.Angle(normal, Vector3.up) > 80.0f)
            {
                if (!Physics.CheckSphere(planeCenter, 0.2f))
                {
                    instantiateObject(m_objects.Length - 1, m_objects.Length, planeCenter + 0.03f * normal, normal);
                }
            }
            //else
            //{
            //    Debug.Log("surface is too steep for object to stand on.");
            //}
        }
    }

    void instantiateObject(int minRange, int maxRange, Vector3 coords, Vector3 normal)
    {
        var rotation = Random.Range(0, 360);
        
        var instantiatedObject = Instantiate(m_objects[Random.Range(minRange, maxRange)], coords, Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.AngleAxis(rotation, Vector3.up)) as GameObject;
        //instantiatedObject.transform.localScale = scale;
    }

    public void OnGUI()
    {
        // Toggle Plants Generation
        if (GUI.Button(new Rect(10, Screen.height - 210, 400, 200), "<size=40>Toggle Seeding</size>"))
        {
            plantsGeneration = !plantsGeneration;
        }

        // Destroy Plants
        if (GUI.Button(new Rect(10, Screen.height - 320, 400, 100), "<size=40>Destroy Plants</size>"))
        {
            var plantsToDestroy = GameObject.FindGameObjectsWithTag("Plants");
            foreach (var plant in plantsToDestroy)
            {
                Destroy(plant);
            }
        }
    }
}
