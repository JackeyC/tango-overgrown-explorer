using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : MonoBehaviour {

    public string animationName;
    public float speed = 1;

    public void playAnimation ()
    {
        Animation animation = GetComponent<Animation>();
        animation[animationName].speed = speed;
        animation.Play();
    }
}
