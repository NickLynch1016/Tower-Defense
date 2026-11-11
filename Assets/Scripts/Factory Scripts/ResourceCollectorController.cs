using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResourceCollectorController : MonoBehaviour
{
    [Header("Nearest Resource Setup")]
    public GameObject[] AllResources;
    public GameObject nearestResource;
    public float distance;
    public float nearestDistance = 99999;
    public int range;

    [Header ("Resource Depositing Setup")]
    public Vector3 station;

    [Header("Resource Collector Stats")]
    public int speed;
    public float health;
    private float maxHealth;
    public bool isPlaced;

    [Header("Held Resources")]
    public GameObject resources_;
    public int resourcesHeld;
    public int maxResources;
    public bool hasTarget;
    public GameObject floatingTXT;

    [Header("Sending Resource Collectors")]
    public GameManager managerCall;
    public bool pressed;

    [Header("Selling/Upgrading Resource Collector")]
    public bool selected;
    public GameObject sellButton;
    public GameObject sellButtonTXT;

    [Header("Resource Collector Visuals")]
    public GameObject resourceVisual;
    public GameObject resourceCollectorCanvas;
    public Slider healthBar;

    void Start()
    {
        managerCall = FindObjectOfType<GameManager>();
        maxHealth = health;
    }
    
    // places down the resource collector
    public void placedDownResourceCollector()
    {
        isPlaced = true;
        station = transform.position;
    }

    void Update()
    {
        if(isPlaced)
        {
            // if send resource collectors is set to always, always send the resource collectors
            if(managerCall.alwaysOrOnPress)
                pressed = true;
            
            // if send resouce collectors is set to on press and the button hasn't been pressed, set sent to false
            if(!managerCall.alwaysOrOnPress && !pressed)
                pressed = false;

            // if the send resource collectors button has been pressed, but there aren't any resources to be picked up, and the resource collector is at the station, make it stay
            AllResources = GameObject.FindGameObjectsWithTag("Resources");
            if(pressed && AllResources.Length == 0 && transform.position == station)
            {
                pressed = false;
            }
            
            // finds a new resource if pressed is true, this resource collector doesn't have one or it's current one is out of range or it isn't the closest resource collector to that resource
            if(pressed) 
            {
                if(nearestResource.Equals(null) || !hasTarget)
                    findNewResource();
                
                else if(!nearestResource.Equals(null) && (Vector3.Distance(transform.position, nearestResource.transform.position) > range || nearestResource.GetComponent<ResourceController>().isClosest(Vector3.Distance(transform.position, nearestResource.transform.position))))
                {
                    findNewResource();
                }
            }
            
            // if there is a nearest resource, it is not targeted, and you have less than the max resources, target it
            if(!nearestResource.Equals(null) && !nearestResource.GetComponent<ResourceController>().isTargetedByMe() && maxResources > resourcesHeld && !hasTarget)
            {
                nearestResource.GetComponent<ResourceController>().setTargetedByMe(health);
                hasTarget = true;
            }

            // if the resource collector has max resources or no target resource and it's not at it's station or the button was set to on press it will move and turn back to its station
            if((!hasTarget || maxResources <= resourcesHeld || nearestResource.Equals(null)) && station != transform.position || !pressed)
            {
                Vector3 pos = Vector3.MoveTowards(transform.position, station, speed * Time.deltaTime);
                transform.position = pos;

                transform.LookAt(new Vector3(station.x, transform.position.y, station.z));
                resourceCollectorCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }
            
            // if the resource collector is sent, has a target, less than max resources, and there is a resource in range it will move and turn to it
            else if(pressed && hasTarget && (resourcesHeld == 0 || maxResources > resourcesHeld) && AllResources.Length > 0 && nearestResource != null)
            {
                Vector3 pos = Vector3.MoveTowards(transform.position, nearestResource.transform.position, speed * Time.deltaTime);
                transform.position = pos;

                transform.LookAt(new Vector3(nearestResource.transform.position.x, transform.position.y, nearestResource.transform.position.z));
                resourceCollectorCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }

            // if the resource collector has returned back to it's station it will deposit the resources
            if(station == transform.position)
            {
                FindObjectOfType<GameManager>().resourcesChange(resourcesHeld);
                resourcesHeld = 0;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                resourceCollectorCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }

            // if the resource collector has resources the visual will be set to active, and vice versa
            if(resourcesHeld > 0)
                resourceVisual.SetActive(true);

            else
                resourceVisual.SetActive(false);
                
            // changes the speed based on the resource collector's held resource amount
            if(resourcesHeld == 0)
                speed = 10;
            if(resourcesHeld == 5)
                speed = 7;
            if(resourcesHeld == 10)
                speed = 5;
            
            // if the resource collector dies remove it's tile, destory it, have it drop it's resources, and have it relinquish any resource it has a claim over
            if(health <= 0)
            {
                FindObjectOfType<BuildingSystem>().removeTile(station);
                Destroy(gameObject);

                if(resourcesHeld > 0)
                {
                    GameObject resources = Instantiate(resources_, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
                    resources.GetComponent<ResourceController>().initializeAMT(resourcesHeld);
                }

                if(!nearestResource.Equals(null) && nearestResource.GetComponent<ResourceController>().isTargetedByMe())
                    nearestResource.GetComponent<ResourceController>().resourceCollectorDied();

                // if this resource collector is behind the factory, reduce the amount of objects behind the factory
                if(transform.position == BuildingSystem.current.GetFactoryLocation() || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z == BuildingSystem.current.GetFactoryLocation().z) || (transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z && transform.position.x == BuildingSystem.current.GetFactoryLocation().x) || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z))
                    FindObjectOfType<FactoryController>().GetComponent<FactoryController>().reduceLockedIn();
            }

            // if the player clicks while the resource collector is being sold and it's not over the sell button or the resource collector, set selected to false, remove the outline, and remove the button
            if(Input.GetMouseButtonDown(0) && selected && !isMouseOverUI() && !(Vector3.Distance(BuildingSystem.GetMouseWorldPosition(), transform.position) <= 2f))
            {    
                sellButton.SetActive(false);
                selected = false;
                GetComponent<Outline>().enabled = false;
            }

            // if the resource collector has no target but hasTargtet is true, set hasTarget to false
            if(nearestResource.Equals(null) && hasTarget)
                hasTarget = false;
        }
    }

    public void sendResources(int i)
    {
        resourcesHeld += i;
    }

    // return true if this resource collector doesn't have max resources
    public bool isNotAtMaxResources()
    {
        return maxResources > resourcesHeld;
    }

    // finds the nearest resource in range, thats not already targeted, and that doesn't have a closer resource collector to it
    public void findNewResource()
    {
        nearestDistance = 99999;
        AllResources = GameObject.FindGameObjectsWithTag("Resources");

        for(int i = 0; i < AllResources.Length; i++)
        {
            distance = Vector3.Distance(transform.position, AllResources[i].transform.position);

            if(distance < range)
            {
                if(distance < nearestDistance && !AllResources[i].GetComponent<ResourceController>().isTargetedByMe() && AllResources[i].GetComponent<ResourceController>().isClosest(distance))
                {
                    nearestResource = AllResources[i];
                    nearestDistance = distance;
                }
            }
        }
    }

    // gives or takes health from this resource collector
    public void damageResourceCollector(float damageToTake)
    {
        if(isPlaced)
        {
            health -= damageToTake;
            healthBar.value = health;
            sellButtonTXT.GetComponent<Text>().text = "SELL FOR: " + Mathf.CeilToInt(health / 3f).ToString() + " RESOURCES";
            if(floatingTXT)
            {
                if(damageToTake < 0)
                    showFloatingTXT(damageToTake * -1);

                else    
                    showFloatingTXT(damageToTake);
            }
        }
    }

    void showFloatingTXT(float DTK)
    {
        var go = Instantiate(floatingTXT, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        go.GetComponent<TextMesh>().text = DTK.ToString();
    }

    // returns the resource collector's health
    public float getResourceCollectorHealth()
    {
        return health;
    }

    // returns the resource collector's max health
    public float getResourceCollectorMaxHealth()
    {
        return maxHealth;
    }

    // returns true is this resource collector is placed
    public bool isResourceCollectorPlaced()
    {
        return isPlaced;
    }

    // returns true if the resource collector is sent
    public bool isResourceCollectorSent()
    {
        return pressed;
    }

    // removes the target from this resource collector
    public void removeTarget()
    {
        hasTarget = false;
    }

    // returns true is this resource collector has a target resource
    public bool resourceCollectorHasTarget()
    {
        return hasTarget;
    }

    // if the send resource collectors button has been pressed, set pressed and sent to true
    public void sendResourceCollector()
    {
        pressed = true;
    }

    public void setToOnPress()
    {
        pressed = false;
    }

    // if the mouse if over the resource collector apply the outline
    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    // if you aren't selling the resource collector and the mouse is off of the resource collector, remove the outline
    void OnMouseExit()
    {
        if(!selected)
            GetComponent<Outline>().enabled = false;
    }

    // if the resource collector is clicked on create the sell resource collector button and set sellingResourceCollector to true
    void OnMouseDown()
    {
        if(isPlaced)
        {
            selected = true;
            sellButton.SetActive(true);
        }
    }

    // adds a third the resource collector's health to the resources and deletes the resource collector
    public void sellResourceCollector()
    {
        FindObjectOfType<GameManager>().GetComponent<GameManager>().resourcesChange(Mathf.CeilToInt(health / 3f));
        health -= 100;
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
}
