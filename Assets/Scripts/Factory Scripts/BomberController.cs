using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BomberController : MonoBehaviour
{
    [Header("Bomber Stats")]
    public float health;
    public float speed;
    private float walkingSpeed;
    public float strength;
    public int range;

    [Header("Nearest Enemy Setup")]
    public List<GameObject> AllEnemies;
    public GameObject nearestEnemy;
    public float distance;
    public float nearestDistance = 99999;
    public bool isResourceCollector;
    public bool canMove;
    public bool hasSelectedTarget;

    [Header("Explosion Setup")]
    [SerializeField] private GameObject explosionPrefab;
    public float explosionDisplayTime;
    public List<GameObject> shooters;
    public List<GameObject> enemyResourceCollectors;
    public List<GameObject> enemyBombers;

    [Header("Bomber Visuals")]
    public GameObject HealthBarCanvas;
    public Slider healthBar;
    public GameObject floatingTXT;

    [Header("Parent Station Setup")]
    public GameObject parentStation;
    public void getParentStation(GameObject parent)
    {
        parentStation = parent;
        health = parentStation.GetComponent<BomberStationController>().getHealth();
        speed = parentStation.GetComponent<BomberStationController>().getSpeed();
        walkingSpeed = parentStation.GetComponent<BomberStationController>().getSpeed() / 2;
        strength = parentStation.GetComponent<BomberStationController>().getStrength(); 
        range = parentStation.GetComponent<BomberStationController>().getRange(); 
    }
    
    void Update()
    {        
        // if the bomber goes to or below 0 health or it reaches it's target it will explode and a new bomber will spawn
        if(health <= 0 || (!nearestEnemy.Equals(null) && Vector3.Distance(transform.position, nearestEnemy.transform.position) < 1.25f))
            explode();

        // if there is no nearest enemy or it's out of range or it's not a resource collector, find a new one
        if(nearestEnemy.Equals(null) || (!nearestEnemy.Equals(null) && Vector3.Distance(transform.position, nearestEnemy.transform.position) > range) || !nearestEnemy.CompareTag("EnemyResourceCollector") && !hasSelectedTarget)
        {
            findNewEnemy();
            transform.LookAt(new Vector3(50f, 0.5f, 0f));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        // if the nearest enemy is in range, move and look towards it
        if(!nearestEnemy.Equals(null) && Vector3.Distance(transform.position, nearestEnemy.transform.position) < range && canMove)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, nearestEnemy.transform.position, speed * Time.deltaTime);
            pos.y = 0f;
            transform.position = pos;

            transform.LookAt(new Vector3(nearestEnemy.transform.position.x, transform.position.y, nearestEnemy.transform.position.z));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        // if the nearest enemy no longer exists while the bomber is moving towards it, move and turn back to the station
        else if(nearestEnemy.Equals(null) || (!nearestEnemy.Equals(null) && Vector3.Distance(transform.position, nearestEnemy.transform.position) > range) && transform.position != parentStation.transform.position)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, parentStation.transform.position, walkingSpeed * Time.deltaTime);
            transform.position = pos;

            transform.LookAt(new Vector3(parentStation.transform.position.x, transform.position.y, parentStation.transform.position.z));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        // if the target dies, set hasSelectedTarget to false
        if(nearestEnemy.Equals(null))
            hasSelectedTarget = false;
    }

    // finds the resource collector, then shooter, and ignores bombers
    public void findNewEnemy()
    {
        nearestDistance = 99999;
        AllEnemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyResourceCollector")); 
        AllEnemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyShooter")); 
        
        for(int i = 0; i < AllEnemies.Count; i++)
        {
            distance = Vector3.Distance(transform.position, AllEnemies[i].transform.position);
            
            if(distance < range && AllEnemies[i].CompareTag("EnemyResourceCollector"))
                isResourceCollector = true;

            else    
                isResourceCollector = false;
        }

        for(int i = 0; i < AllEnemies.Count; i++)
        {
            distance = Vector3.Distance(transform.position, AllEnemies[i].transform.position);

            if(distance < range)
            {
                // if there is a resource collector in range target it
                if(isResourceCollector && distance < nearestDistance && AllEnemies[i].CompareTag("EnemyResourceCollector"))
                {
                    nearestEnemy = AllEnemies[i];
                    nearestDistance = distance;
                }
                
                // otherwise just target the nearest enemy
                else if(distance < nearestDistance)
                {
                    nearestEnemy = AllEnemies[i];
                    nearestDistance = distance;
                }
            }
        }

        AllEnemies.Clear();
    }
    
    // if the mouse if over the resource collector apply the outline
    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    // if you aren't selling the resource collector and the mouse is off of the resource collector, remove the outline
    void OnMouseExit()
    {
        GetComponent<Outline>().enabled = false;
    }

    // if the resource collector is clicked on create the sell resource collector button and set sellingResourceCollector to true
    void OnMouseDown()
    {
        explode();
    }

    // returns true is the mouse is over UI
    private bool isMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject() && isMouseOverUIIgnore();
    }

    // returns true is there an object that you want to click
    private bool isMouseOverUIIgnore()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for(int i = 0; i < raycastResultList.Count; i++)
        {
            if(!raycastResultList[i].gameObject.CompareTag("SellButtonUI"))
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }

        return raycastResultList.Count > 0;
    }

    private void explode()
    {
        // damages every shooter based on it's distance to them
        shooters.AddRange(GameObject.FindGameObjectsWithTag("EnemyShooter"));
        for(int i = 0; i < shooters.Count; i++)
        {
            float explosionDamageToShooter = (-1f/3f) * (Vector3.Distance(transform.position, shooters[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToShooter) > 0)
                shooters[i].GetComponent<ShooterEnemyController>().damageShooter(Mathf.RoundToInt(explosionDamageToShooter));
        }

        // damages every enemy resource collector based on it's distance to them
        enemyResourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("EnemyResourceCollector"));
        for(int i = 0; i < enemyResourceCollectors.Count; i++)
        {
            float explosionDamageToResourceCollector = (-1f/3f) * (Vector3.Distance(transform.position, enemyResourceCollectors[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToResourceCollector) > 0)
                enemyResourceCollectors[i].GetComponent<ResourceCollectorEnemyController>().damageResourceCollector(Mathf.RoundToInt(explosionDamageToResourceCollector));
        }

        // damages every enemy bomber based on it's distance to them
        enemyBombers.AddRange(GameObject.FindGameObjectsWithTag("EnemyBomber"));
        for(int i = 0; i < enemyBombers.Count; i++)
        {
            float explosionDamageToBomber = (-1f/3f) * (Vector3.Distance(transform.position, enemyBombers[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToBomber) > 0)
                enemyBombers[i].GetComponent<BomberEnemyController>().damageBomber(Mathf.RoundToInt(explosionDamageToBomber));
        }

        shooters.Clear();
        enemyResourceCollectors.Clear();
        enemyBombers.Clear();
        explosionEffects();
        Destroy(gameObject);
        parentStation.GetComponent<BomberStationController>().removeBomber();
    }
    
    // handles the explosion effects
    public void explosionEffects()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, explosionDisplayTime);
    }

    // damages the bomber
    public void damageBomber(float damageToTake)
    {
        health -= damageToTake;
        healthBar.value = health;
        if(floatingTXT)
            {
                if(damageToTake < 0)
                    showFloatingTXT(damageToTake * -1);

                else    
                    showFloatingTXT(damageToTake);
            }
    }

    void showFloatingTXT(float DTK)
    {
        var go = Instantiate(floatingTXT, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        go.GetComponent<TextMesh>().text = DTK.ToString();
    }

    public void sendBomber()
    {
        canMove = true;
    }

    // if this enemy is in range and is not a bomber, target it
    public void targetThis(GameObject enemyToTarget)
    {
        distance = Vector3.Distance(transform.position, enemyToTarget.transform.position);

        if(distance < range && !enemyToTarget.CompareTag("EnemyBomber"))
        {
            nearestEnemy = enemyToTarget;
            nearestDistance = distance;
            hasSelectedTarget = true;
        }

        // if the enemy is out of range deselect it
        else
        {
            if(enemyToTarget.tag == "EnemyBomber")
                enemyToTarget.GetComponent<BomberEnemyController>().deselect();

            else if(enemyToTarget.tag == "EnemyShooter")
                enemyToTarget.GetComponent<ShooterEnemyController>().deselect();

            else if(enemyToTarget.tag == "EnemyResourceCollector")
                enemyToTarget.GetComponent<ResourceCollectorEnemyController>().deselect();   
        }
    }

    // removes the selected target from this bomber
    public void noMoreSelectedTarget()
    {
        hasSelectedTarget = false;
    }
}
