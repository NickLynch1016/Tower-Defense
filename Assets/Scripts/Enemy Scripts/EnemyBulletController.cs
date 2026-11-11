using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletController : MonoBehaviour
{
    [Header("Enemies")]
    public List<GameObject> targets;
    
    [Header("Bullet Stats")]
    public float damage;
    public float speed;

    [Header("Movement and Position Setup")]
    public Quaternion rotate;
    public Vector3 prevPos;
    
    // for deleting the bullet
    public float time = 3f;
    private float timer;

    void Start()
    {
        timer = 0f;
        prevPos = transform.position;
    }

    void Update()
    {   
        prevPos = transform.position;

        transform.Translate(rotate * Vector3.forward * Time.deltaTime * speed);
        
        RaycastHit[] hits = Physics.RaycastAll(new Ray(prevPos, (transform.position - prevPos).normalized), (transform.position - prevPos).magnitude);

        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider.gameObject.CompareTag("Turret") && !hits[i].collider.gameObject.Equals(null))
            {
                hits[i].collider.gameObject.GetComponent<TurretController>().damageTurret(damage);
                Destroy(gameObject);
            }

            else if(hits[i].collider.gameObject.CompareTag("Bomber") && !hits[i].collider.gameObject.Equals(null))
            {
                hits[i].collider.gameObject.GetComponent<BomberController>().damageBomber(damage);
                Destroy(gameObject);
            }

            else if(hits[i].collider.gameObject.CompareTag("ResourceCollector") && !hits[i].collider.gameObject.Equals(null))
            {
                hits[i].collider.gameObject.GetComponent<ResourceCollectorController>().damageResourceCollector(damage);
                Destroy(gameObject);
            }
        }

        Debug.DrawLine(transform.position, prevPos);
        
        // bullet will be destroyed after 3 seconds of existing
        if(timer > time)
            Destroy(gameObject);

        timer += Time.deltaTime;
    }

    // is used by the shooter to give the bullet it's damage
    public void instantiateDamage(float damageToGive)
    {
        damage = damageToGive;
    }

    // is used by the shooter to give the bullet it's speed to determine the bullets previous position for raycast collision checking
    public void instantiateSpeed(float speedToGive)
    {
        speed = speedToGive;
    }

    // gets the direction the shooter was facing when it shot
    public void instantiateSpawnPos(Transform spawnPos)
    {
        rotate = spawnPos.rotation;
    }
}
