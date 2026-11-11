using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCollectorEnemyController : MonoBehaviour
{
    [Header("Resource Collector Stats")]
    public float speed;
    public float health;

    [Header("Movement Points")]
    public List<GameObject> movementPoints;
    public bool factoryInWay;
    public int currentMovementPoint;

    [Header("Held Resources")]
    public GameObject resources_;
    public int resourcesHeld;
    public int maxResources;

    [Header("Dropped Resource Recovery")]
    public GameObject[] resourcesDropped;
    public GameObject nearestDroppedResource;
    public float distance;
    public float nearestDistance = 99999;
    public int range;
    public bool hasTarget;

    [Header("Enemy Resource Collector Visuals")]
    public GameObject HealthBarCanvas;
    public Slider healthBar;
    public bool selected;
    public List<GameObject> turrets;
    public GameObject floatingTXT;
    
    void Start()
    {
        // if the factory is in the path, move to the left of the factory
        if(BuildingSystem.current.GetFactoryLocation().x > transform.position.x && BuildingSystem.current.GetFactoryLocation().x - 5 < transform.position.x)
        {
            movementPoints[0].transform.position = new Vector3(50f, 0.5f, BuildingSystem.current.GetFactoryLocation().z - 7.5f);
            movementPoints[1].transform.position = new Vector3(BuildingSystem.current.GetFactoryLocation().x - 7.5f, 0.5f, BuildingSystem.current.GetFactoryLocation().z - 7.5f);
            movementPoints[2].transform.position = new Vector3(BuildingSystem.current.GetFactoryLocation().x - 7.5f, 0.5f, BuildingSystem.current.GetFactoryLocation().z + 7.5f);
            movementPoints[3].transform.position = new Vector3(50f, 0.5f, BuildingSystem.current.GetFactoryLocation().z + 7.5f);
            factoryInWay = true;
        }

        // if the factory is in the path, move to the right of the factory
        else if(BuildingSystem.current.GetFactoryLocation().x < transform.position.x && BuildingSystem.current.GetFactoryLocation().x + 5 > transform.position.x)
        {
            movementPoints[0].transform.position = new Vector3(50f, 0.5f, BuildingSystem.current.GetFactoryLocation().z - 7.5f);
            movementPoints[1].transform.position = new Vector3(BuildingSystem.current.GetFactoryLocation().x + 7.5f, 0.5f, BuildingSystem.current.GetFactoryLocation().z - 7.5f);
            movementPoints[2].transform.position = new Vector3(BuildingSystem.current.GetFactoryLocation().x + 7.5f, 0.5f, BuildingSystem.current.GetFactoryLocation().z + 7.5f);
            movementPoints[3].transform.position = new Vector3(50f, 0.5f, BuildingSystem.current.GetFactoryLocation().z + 7.5f);
            factoryInWay = true;
        }
    }
    
    void Update()
    {
        resourcesDropped = GameObject.FindGameObjectsWithTag("Resources");
        findNearestResource();

        // if there is a nearest resources, it is not targeted, and you have less than the max resources, target it
        if(!nearestDroppedResource.Equals(null) && nearestDroppedResource.GetComponent<ResourceController>().isNotTargetedByEnemy() && maxResources > resourcesHeld && !hasTarget)
        {
            nearestDroppedResource.GetComponent<ResourceController>().setTargetedByEnemy();
            hasTarget = true;
        }

        // Destroys the enemy if it's health reaches 0 and has it drop it's resources
        if(health <= 0)
        {
            // if this enemy resource collector has a target it will relinquish it
            if(!nearestDroppedResource.Equals(null))
                nearestDroppedResource.GetComponent<ResourceController>().enemyResourceCollectorDied();

            Destroy(gameObject);
            Vector3 spawnPos = transform.position;
            spawnPos.y -= 0.5f;
            GameObject resources = Instantiate(resources_, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
            resources.GetComponent<ResourceController>().initializeAMT(resourcesHeld);
        }

        // if you have a target, it is in range, and you have less than max resources, move and turn to it
        if(hasTarget && nearestDistance < range && maxResources > resourcesHeld)
        {
            //Vector3 destination = nearestDroppedResource.transform.position;
            Vector3 pos = Vector3.MoveTowards(transform.position, nearestDroppedResource.transform.position, speed * Time.deltaTime);
            pos.y = 0.5f;
            transform.position = pos;

            transform.LookAt(new Vector3(nearestDroppedResource.transform.position.x, transform.position.y, nearestDroppedResource.transform.position.z));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        // otherwise, move and turn to the base
        else
        {
            if(factoryInWay)
            {
                Vector3 pos = Vector3.MoveTowards(transform.position, movementPoints[currentMovementPoint].transform.position, speed * Time.deltaTime);
                transform.position = pos;

                transform.LookAt(new Vector3(movementPoints[currentMovementPoint].transform.position.x, transform.position.y, movementPoints[currentMovementPoint].transform.position.z));
                HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }

            else
            {
                Vector3 pos = Vector3.MoveTowards(transform.position, movementPoints[4].transform.position, speed * Time.deltaTime);
                transform.position = pos;

                transform.LookAt(new Vector3(movementPoints[4].transform.position.x, transform.position.y, movementPoints[4].transform.position.z));
                HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }
            
            // if the enemy reached the end, delete them
            if(transform.position == movementPoints[currentMovementPoint].transform.position && currentMovementPoint == 4)
                Destroy(gameObject);
            
            // if the enemy reached the current movement point, change to the next one
            else if(transform.position == movementPoints[currentMovementPoint].transform.position)
                currentMovementPoint++;
        }

        // if the player clicks off of the resource collector, remove the outline
        if(Input.GetMouseButtonDown(0) && selected && !(Vector3.Distance(BuildingSystem.GetMouseWorldPosition(), transform.position) <= 2f))
        {
            selected = false;
            GetComponent<Outline>().enabled = false;
        }

        // if the object is no longer selected remove the outline
        if(!selected)
            GetComponent<Outline>().enabled = false;

        // if the enemy is at the base it will disappear
        if(movementPoints[4].transform.position.z <= transform.position.z)
            Destroy(gameObject);
    }

    public void sendResources(int i)
    {
        resourcesHeld += i;
    }

    public bool isNotAtMaxResources()
    {
        return maxResources > resourcesHeld;
    }

    public void removeTarget()
    {
        hasTarget = false;
    }

    // finds the nearest resource
    private void findNearestResource()
    {
        nearestDistance = 99999;

        for(int i = 0; i < resourcesDropped.Length; i++)
        {
            distance = Vector3.Distance(transform.position, resourcesDropped[i].transform.position);

            if(distance < range)
            {
                if(distance < nearestDistance)
                {
                    nearestDroppedResource = resourcesDropped[i];
                    nearestDistance = distance;
                }
            }
        }
    }

    public void damageResourceCollector(float damageToTake)
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

    // if the mouse is over the resource collector apply the outline
    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    // if you aren't selecting the resource collector and the mouse is off of the resource collector, remove the outline
    void OnMouseExit()
    {
        if(!selected)
            GetComponent<Outline>().enabled = false;
    }

    // if the resource collector is clicked select the resource collector and have all defenses target it
    void OnMouseDown()
    {
        selected = true;
        turrets.AddRange(GameObject.FindGameObjectsWithTag("Turret"));
        turrets.AddRange(GameObject.FindGameObjectsWithTag("Bomber"));
        
        for(int i = 0; i < turrets.Count; i++)
        {
            // gets all turrets to target it
            if(turrets[i].CompareTag("Turret"))
                turrets[i].GetComponent<TurretController>().targetThis(gameObject);

            // gets all bombers to target it
            else if(turrets[i].CompareTag("Bomber"))
                turrets[i].GetComponent<BomberController>().targetThis(gameObject);
        }

        turrets.Clear();
    }

    public void deselect()
    {
        selected = false;
    }
}
