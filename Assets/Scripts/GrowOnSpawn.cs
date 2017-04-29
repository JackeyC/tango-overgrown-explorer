using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowOnSpawn : MonoBehaviour {

    public float growthSpeed = 0.1f;
    public float scale = 0.02f;
    public float sizeDeviation = 0.01f;
    float growthRate;

	void Start () {
        scale = Random.Range(scale - sizeDeviation, scale + sizeDeviation);
        transform.localScale = new Vector3 (0, 0, 0);
        gameObject.tag = "Plants";
	}
	
	void Update ()
    {
        if (transform.localScale.x < scale)
        {
            growthRate = growthSpeed * scale * Time.deltaTime;
            transform.localScale += new Vector3(growthRate, growthRate, growthRate);
        }
    }
}
