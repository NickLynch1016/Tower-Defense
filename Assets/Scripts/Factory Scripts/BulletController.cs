using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Type")]
    public bool normal;
    public bool shotgun;
    public float damageFalloff;

    [Header("Bullet Stats")]
    public float damage;
    public float speed;

    [Header("Movement and Position Setup")]
    public Quaternion rotate;
    public Vector3 prevPos;

    [Header("Time Before Deletion")]
    public float time = 3f;
    private float timer;

    void Start()
    {
        timer = 0f;   
        if(shotgun)
            damageFalloff = damage / 7;

        prevPos = transform.position;
    }

    void Update()
    {   
        prevPos = transform.position;

        transform.Translate(rotate * Vector3.forward * Time.deltaTime * speed);
        
        RaycastHit[] hits = Physics.RaycastAll(new Ray(prevPos, (transform.position - prevPos).normalized), (transform.position - prevPos).magnitude);

        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].collider.gameObject.CompareTag("EnemyShooter") && !hits[i].collider.gameObject.Equals(null))
            {
                hits[i].collider.gameObject.GetComponent<ShooterEnemyController>().damageShooter(Mathf.CeilToInt(damage));
                Destroy(gameObject);
            }

            else if(hits[i].collider.gameObject.CompareTag("EnemyBomber") && !hits[i].collider.gameObject.Equals(null))
            {
                hits[i].collider.gameObject.GetComponent<BomberEnemyController>().damageBomber(Mathf.CeilToInt(damage));
                Destroy(gameObject);
            }

            else if(hits[i].collider.gameObject.CompareTag("EnemyResourceCollector") && !hits[i].collider.gameObject.Equals(null))
            {
                hits[i].collider.gameObject.GetComponent<ResourceCollectorEnemyController>().damageResourceCollector(Mathf.CeilToInt(damage));
                Destroy(gameObject);
            }
        }

        Debug.DrawLine(transform.position, prevPos);
        
        // bullet will be destroyed after 3 seconds of existing
        if(timer > time)
            Destroy(gameObject);

        // shotgun bullet damage fall off
        if(shotgun && timer % 0.033f == 0f)
        {
            damage -= damageFalloff;
        }

        timer += Time.deltaTime;
    }

    // is used by the turret to give the bullet it's damage
    public void instantiateDamage(float damageToGive)
    {
        if(normal)
            damage = damageToGive;

        else if(shotgun)
            damage = damageToGive * 0.5f;
    }

    // is used by the turret to give the bullet it's speed to determine the bullets previous position for raycast collision checking
    public void instantiateSpeed(float speedToGive)
    {
        speed = speedToGive;
    }

    // gets the direction the turret was facing when it shot
    public void instantiateSpawnPos(Transform spawnPos)
    {
        rotate = spawnPos.rotation;
    }

    public void isNormal(bool isIt)
    {
        normal = isIt;
    }
    
    public void isShotgun(bool isIt)
    {
        shotgun = isIt;
    }
}
