using System.Collections.Generic;
using UnityEngine;

public class MovementFollower : MonoBehaviour {

    //public bool onButtonClicked;

    public float speed = 1;

    public GameObject targetToFollow;
    //Vector3 targetPreviousPosition;
    //Vector3 targetDeltaPosition;
    //Vector3 startPosition;

    Vector3 destinationOffset = new Vector3(0, 0.2f, 1);
    List<Vector3> waypoints = new List<Vector3>();

    float lastTime;
	
	void Update ()
    {
        if (waypoints.Count != 0)
        {
            Vector3 lookDirection = waypoints[0] - transform.position;
            lookDirection.y = transform.position.y;
            transform.position += transform.forward * speed * Time.deltaTime;

            //transform.position = Vector3.MoveTowards(transform.position, waypoints[0], speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 1 * Time.deltaTime);
            if ((transform.position - waypoints[0]).magnitude < 0.1f)
            {
                waypoints.Remove(waypoints[0]);
            }
        }
        //if (onButtonClicked)
        //{
            //targetDeltaPosition = targetToFollow.transform.position - targetPreviousPosition;
            //if (Vector3.Magnitude(targetDeltaPosition) > 0.01f)
            //{
            //    print("delta > 0.1");
            //    if (pathWaypoints.Count == 0)
            //    {
            //        pathWaypoints.Add(startPosition + targetDeltaPosition);
            //        print("first point registered");
            //    }
            //    else
            //    {
            //        pathWaypoints.Add(startPosition + pathWaypoints[pathWaypoints.Count - 1] + targetDeltaPosition);
            //    }
            //    print("point registered");
            //}
            //if (pathWaypoints.Count != 0)
            //{
            //    transform.position = Vector3.MoveTowards(transform.position, pathWaypoints[0], speed * Time.deltaTime);
            //    var lookDirection = pathWaypoints[0] - transform.position;
            //    lookDirection.y = transform.position.y;
            //    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 1 * Time.deltaTime);
            //    if ((transform.position - pathWaypoints[0]).magnitude < 0.5f)
            //    {
            //        pathWaypoints.Remove(pathWaypoints[0]);
            //        print("point removed");
            //    }
            //}
            //targetPreviousPosition = targetToFollow.transform.position;
        //}
    }

    public void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width - 310, Screen.height - 310, 300, 300), "<size=50>Move Ship</size>"))
        {
            //targetPreviousPosition = targetToFollow.transform.position;
            waypoints.Add(targetToFollow.transform.position - Vector3.up * 0.2f);
            //onButtonClicked = !onButtonClicked;
            //startPosition = transform.position;
            //lastTime = Time.time;
        }
    }
}
