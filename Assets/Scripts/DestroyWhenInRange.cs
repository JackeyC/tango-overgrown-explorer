using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenInRange : MonoBehaviour {

    public string target;
    public float range;

    void Update () {
        GameObject targetObject = GameObject.Find(target);
        if ((targetObject.transform.position - transform.position).magnitude < range)
        {
            Destroy(gameObject);
        }
	}
}
