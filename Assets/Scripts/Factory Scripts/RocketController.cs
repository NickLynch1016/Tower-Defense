using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Rocket Stats")]
    public float speed;
    public float strength;

    [Header("Targeting Setup")]
    public GameObject target;

    [Header("Movement Setup")]
    public Vector3 prevPos;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // is used by the turret to give the bullet it's damage
    public void instantiateStrength(float damageToGive)
    {
        strength = damageToGive;
    }

    // is used by the turret to give the bullet it's speed to determine the bullets previous position for raycast collision checking
    public void instantiateSpeed(float speedToGive)
    {
        speed = speedToGive;
    }

    // gets the direction the turret was facing when it shot
    public void instantiateTarget(GameObject targetToGive)
    {
        target = targetToGive;
    }
}
