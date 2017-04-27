using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsFollower : MonoBehaviour
{

    public float acceleration = 0.3f;
    public float forwardSpeed = 2;
    public float liftSpeed = 0.2f;
    public float turnSpeed = 0.5f;

    public GameObject targetToFollow;

    public bool displayWaypoints;
    public GameObject waypointPrefab;

    List<Vector3> waypoints = new List<Vector3>();

    Vector3 distnaceToTarget;
    float currentForwardSpeed;
    float decelerateDistance;
    float maxSpeed;

    void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            // Debug only
            addWaypoints();
        }

        // Run when there's waypoint(s)
        if (waypoints.Count != 0)
        {
            distnaceToTarget = waypoints[0] - transform.position;

            // Constrain Airship rotation towards target around y axis only.
            distnaceToTarget.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(distnaceToTarget), turnSpeed * Time.deltaTime);

            // Acceleration and Deceleration
            if (currentForwardSpeed == 0)
            {
                if (distnaceToTarget.magnitude < forwardSpeed * forwardSpeed / acceleration)
                {
                    maxSpeed = Mathf.Sqrt(acceleration * distnaceToTarget.magnitude);
                }
                else
                {
                    maxSpeed = forwardSpeed;
                }
                decelerateDistance = 0.5f * maxSpeed * maxSpeed / acceleration;
            }

            // Deceleration
            if (distnaceToTarget.magnitude < decelerateDistance && currentForwardSpeed > 0.1)
            {
                currentForwardSpeed -= acceleration * Time.deltaTime;
                Mathf.Clamp(currentForwardSpeed, 0, forwardSpeed);
            }
            
            // Acceleration
            else if (currentForwardSpeed < maxSpeed)
            {
                currentForwardSpeed += acceleration * Time.deltaTime;
            }
            
            // Check Horizontal distance
            transform.position += transform.forward * currentForwardSpeed * Time.deltaTime;
            
            // Up/Down movement
            if (waypoints[0].y - transform.position.y > 0.05)
            {
                transform.position += transform.up * liftSpeed * Time.deltaTime;
            }
            if (waypoints[0].y - transform.position.y < -0.05)
            {
                transform.position -= transform.up * liftSpeed * Time.deltaTime;
            }

            // Remove current waypoint when reached
            if ((waypoints[0] - transform.position).magnitude < 0.3f)
            {
                waypoints.Remove(waypoints[0]);
            }

            // Display waypoints
            if (displayWaypoints)
            {
                foreach (Vector3 waypoint in waypoints)
                {
                    GameObject[] existingWaypoints = GameObject.FindGameObjectsWithTag("Waypoints");
                    foreach (GameObject i in existingWaypoints)
                    {
                        //if (waypoint != i.transform);
                    }
                    Instantiate(waypointPrefab, waypoint, Quaternion.identity);
                }
            }
        }
    }

    void addWaypoints()
    {
        waypoints.Add(targetToFollow.transform.position);
    }

    public void OnGUI()
    {
        // Add waypoint when button touched
        if (GUI.Button(new Rect(Screen.width - 310, Screen.height - 310, 300, 300), "<size=40>Set Waypoint</size>"))
        {
            addWaypoints();
        }
        if (GUI.Toggle(new Rect(Screen.width - 310, 10, 300, 100), displayWaypoints, "<size=40>Toggle Waypoint</size>"))
        {
            displayWaypoints = !displayWaypoints;
        }
    }
}
