using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartController : MonoBehaviour
{
    [Header("Healing Setup")]
    public float health;
    public List<GameObject> turrets;
    public List<GameObject> resourceCollectors;
    public List<GameObject> bomberStations;

    [Header("BuildingSystem and GameManager Reference")]
    public BuildingSystem system;
    public GameManager manager;
    
    void Awake()
    {
        system = FindObjectOfType<BuildingSystem>();
        manager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        // if you press space while you aren't placing another object, destory the heart and give health to the object under it
        if(Input.GetKeyDown(KeyCode.Space))
        {
            turrets.AddRange(GameObject.FindGameObjectsWithTag("Turret"));

            for(int i = 0; i < turrets.Count; i++)
            {
                // if the heart is above the turret and the turret is placed, destory the heart and heal the turret
                if(turrets[i].transform.position.x == transform.position.x && turrets[i].transform.position.z == transform.position.z)
                {
                    // gives the max health minus the current health
                    float healthToGive = turrets[i].GetComponent<TurretController>().getTurretMaxHealth() - turrets[i].GetComponent<TurretController>().getTurretHealth();
                    
                    // if you have the resources to get this turret to max health, give them
                    if(!turrets[i].Equals(null) && healthToGive > 0 && manager.resources >= healthToGive)
                    {
                        turrets[i].GetComponent<TurretController>().damageTurret(Mathf.CeilToInt(-healthToGive));
                        FindObjectOfType<GameManager>().resourcesChange(Mathf.CeilToInt(-healthToGive));
                    }

                    // if you don't have enough resources to get the turret to max health, but still have some, give them
                    else if(!turrets[i].Equals(null) && healthToGive > 0 && manager.resources > 0 && manager.resources < healthToGive)
                    {
                        turrets[i].GetComponent<TurretController>().damageTurret(-manager.resources);
                        FindObjectOfType<GameManager>().resourcesChange(-manager.resources);
                    }

                    // if the object under has max health
                    else if(healthToGive <= 0)
                        print("Nothing To Heal!");

                    // if you have no resources
                    else
                        print("Not Enough Resources!");
                }
            }

            resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));

            for(int i = 0; i < resourceCollectors.Count; i++)
            {
                // if the heart is above the turret and the turret is placed, destory the heart and heal the turret
                if(!resourceCollectors[i].Equals(null) && resourceCollectors[i].transform.position.x == transform.position.x && resourceCollectors[i].transform.position.z == transform.position.z)
                {
                    // gives the max health minus the current health
                    float healthToGive = resourceCollectors[i].GetComponent<ResourceCollectorController>().getResourceCollectorMaxHealth() - resourceCollectors[i].GetComponent<ResourceCollectorController>().getResourceCollectorHealth();
                    
                    // if you have the resources to get this resource collector to max health, give them
                    if(healthToGive > 0 && manager.resources >= healthToGive)
                    {
                        resourceCollectors[i].GetComponent<ResourceCollectorController>().damageResourceCollector(Mathf.CeilToInt(-healthToGive));
                        FindObjectOfType<GameManager>().resourcesChange(Mathf.CeilToInt(-healthToGive));
                    }

                    // if you don't have enough resources to get the resource collector to max health, but still have some, give them
                    else if(!resourceCollectors[i].Equals(null) && healthToGive > 0 && manager.resources > 0 && manager.resources < healthToGive)
                    {
                        resourceCollectors[i].GetComponent<ResourceCollectorController>().damageResourceCollector(-manager.resources);
                        FindObjectOfType<GameManager>().resourcesChange(-manager.resources);
                    }

                    // if the object under has max health
                    else if(healthToGive <= 0)
                        print("Nothing To Heal!");

                    // if you have no resources
                    else
                        print("Not Enough Resources!");
                }
            }

            bomberStations.AddRange(GameObject.FindGameObjectsWithTag("BomberStation"));

            for(int i = 0; i < bomberStations.Count; i++)
            {
                // if the heart is above the bomber station, destory the heart and heal the turret
                if(!bomberStations[i].Equals(null) && bomberStations[i].transform.position.x == transform.position.x && bomberStations[i].transform.position.z == transform.position.z)
                {
                    // gives the max health minus the current health
                    float healthToGive = bomberStations[i].GetComponent<BomberStationController>().getBomberStationMaxHealth() - bomberStations[i].GetComponent<BomberStationController>().getBomberStationHealth();
                    
                    // if you have the resources to get this resource collector to max health, give them
                    if(healthToGive > 0 && manager.resources >= healthToGive)
                    {
                        bomberStations[i].GetComponent<BomberStationController>().damageBomberStation(Mathf.CeilToInt(-healthToGive));
                        FindObjectOfType<GameManager>().resourcesChange(Mathf.CeilToInt(-healthToGive));
                    }

                    // if you don't have enough resources to get the bomber station to max health, but still have some, give them
                    else if(!bomberStations[i].Equals(null) && healthToGive > 0 && manager.resources > 0 && manager.resources < healthToGive)
                    {
                        bomberStations[i].GetComponent<BomberStationController>().damageBomberStation(-manager.resources);
                        FindObjectOfType<GameManager>().resourcesChange(-manager.resources);
                    }

                    // if the object under has max health
                    else if(healthToGive <= 0)
                        print("Nothing To Heal!");

                    // if you have no resources
                    else
                        print("Not Enough Resources!");
                }
            }

            Destroy(gameObject);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RaycastScript>().removeHeart();
            turrets.Clear();
            resourceCollectors.Clear();  
            bomberStations.Clear();
        }

        // if you press escape the heart will be deleted
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(gameObject);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<RaycastScript>().removeHeart();
        }
    }
}
