﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlantsGeneration : MonoBehaviour
{
    public GameObject[] m_objects = new GameObject[6];
    public Vector3 scale;
    private TangoPointCloud m_pointCloud;

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
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                PlaceObject(new Vector2(i, j));
            }
        }
    }


    void PlaceObject(Vector2 position)
    {
        // Find the plane.
        Camera cam = Camera.main;
        Vector3 planeCenter;
        Plane plane;
        if (!m_pointCloud.FindPlane(cam, position, out planeCenter, out plane))
        {
            Debug.Log("cannot find plane.");
            return;
        }

        Vector3 up = plane.normal;
        Vector3 right = Vector3.Cross(plane.normal, cam.transform.forward).normalized;
        Vector3 forward = Vector3.Cross(right, plane.normal).normalized;

        // Place object on the surface, and make it always face the camera.
        if (Vector3.Angle(plane.normal, Vector3.up) < 5.0f)
        {
            instantiateObject(m_objects.Length, planeCenter, forward, up);
        }
        else if (Vector3.Angle(plane.normal, Vector3.up) < 10.0f)
        {
            instantiateObject(m_objects.Length - 1, planeCenter, forward, up);
        }
        else if (Vector3.Angle(plane.normal, Vector3.up) < 50.0f)
        {
            instantiateObject(5, planeCenter, forward, up);
        }
        else
        {
            Debug.Log("surface is too steep for object to stand on.");
        }
    }

    void instantiateObject(int range, Vector3 coords, Vector3 forward, Vector3 up)
    {
        var instantiatedObject = Instantiate(m_objects[Random.Range(0, range)], coords, Quaternion.LookRotation(forward, up)) as GameObject;
        instantiatedObject.transform.localScale = scale;
        ARMarker markerScript = instantiatedObject.GetComponent<ARMarker>();
    }
}