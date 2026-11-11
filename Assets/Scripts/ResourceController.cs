using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    public int resourcesAMT;
    public bool isResourceCollectorTouching;

    [Header("Nearest Resource Collector")]
    public List<GameObject> resourceCollectors;
    public List<GameObject> enemyResourceCollectors;

    [Header("Targeted")]
    public bool targetedByEnemy;
    public bool targetedByMe;
    
    void Update()
    {
        // is collected by a resource collectors
        resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));
        for(int i = 0; i < resourceCollectors.Count; i++)
        {
            if(!resourceCollectors[i].Equals(null) && resourceCollectors[i].transform.position == transform.position && resourceCollectors[i].GetComponent<ResourceCollectorController>().isNotAtMaxResources())
            {
                resourceCollectors[i].GetComponent<ResourceCollectorController>().sendResources(5);
                resourcesAMT -= 5;
                resourceCollectors[i].GetComponent<ResourceCollectorController>().removeTarget();
                if(resourcesAMT == 0)
                    Destroy(gameObject);
            }
        }

        // is collected by an enemy resource collector
        enemyResourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("EnemyResourceCollector"));
        for(int i = 0; i < enemyResourceCollectors.Count; i++)
        {
            if(!enemyResourceCollectors[i].Equals(null) && (enemyResourceCollectors[i].transform.position.z >= transform.position.z - 0.1f) && enemyResourceCollectors[i].GetComponent<ResourceCollectorEnemyController>().isNotAtMaxResources())
            {
                enemyResourceCollectors[i].GetComponent<ResourceCollectorEnemyController>().sendResources(5);
                resourcesAMT -= 5;
                enemyResourceCollectors[i].GetComponent<ResourceCollectorEnemyController>().removeTarget();
                if(resourcesAMT == 0)
                    Destroy(gameObject);
            }
        }

        if(targetedByMe)
            targetedByMe = checkTargeted();
        
        resourceCollectors.Clear();
        enemyResourceCollectors.Clear();
    }

    // if targetedByEnemy is true but no one is targeting this resource, set isTargetedByMe to false
    public bool checkTargeted()
    {
        resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));
        
        for(int i = 0; i < resourceCollectors.Count; i++)
        {
            if(targetedByMe && resourceCollectors[i].GetComponent<ResourceCollectorController>().resourceCollectorHasTarget())
                return true;
        }

        return false;
    }
    
    // gives the resources a value once it's been dropped by a resource collector
    public void initializeAMT(int amt)
    {
        resourcesAMT = amt;
    }

    public void setTargetedByEnemy()
    {
        targetedByEnemy = true;
    }

    public bool isNotTargetedByEnemy()
    {
        return !targetedByEnemy;
    }

    public void enemyResourceCollectorDied()
    {
        targetedByEnemy = false;
    }

    public void setTargetedByMe(float resourceCollectorHealth)
    {
        if(resourceCollectorHealth > 0)
            targetedByMe = true;
    }

    public bool isTargetedByMe()
    {
        return targetedByMe;
    }

    public void resourceCollectorDied()
    {
        targetedByMe = false;
    }

    public bool isClosest(float distanceToResource)
    {   
        resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));
        for(int i = 0; i < resourceCollectors.Count; i++)
        {
            // if the called resource collector is not null, is closer than the calling resource collector, its not at max resources, and it's placed, return false, you can't claim this resource
            if(!resourceCollectors[i].Equals(null) && distanceToResource != Vector3.Distance(resourceCollectors[i].transform.position, transform.position) && Vector3.Distance(resourceCollectors[i].transform.position, transform.position) < distanceToResource && resourceCollectors[i].GetComponent<ResourceCollectorController>().isNotAtMaxResources() && resourceCollectors[i].GetComponent<ResourceCollectorController>().isResourceCollectorPlaced())  
                return false;
        }

        resourceCollectors.Clear();
        return true;
    }
}
