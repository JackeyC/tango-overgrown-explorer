using UnityEngine;

public class DestroyWhenInRange : MonoBehaviour {

    public string target;
    public float range;

    void Update () {
        GameObject targetObject = GameObject.Find(target);
        if ((targetObject.transform.position + new Vector3(0, 0.2f, 0) - transform.position).magnitude < range)
        {
            Destroy(gameObject);
        }
	}
}
