using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BomberStationController : MonoBehaviour
{
    public bool isPlaced;
    public float health;
    private float timer;
    public float createBomberTimer;
    public GameObject bomber_;
    public Slider healthBar;
    public Animator bomberStationController;
    public GameObject HealthBarCanvas;
    private bool first = true;
    public GameObject childBomber;
    public bool hasBomber;
    public GameObject sellButtonTXT;
    public bool under;
    private float maxHealth;
    public GameObject floatingTXT;

    [Header("Bomber Stats")]
    public float bomberHealth;
    public float bomberSpeed;
    public float bomberStrength;
    public int bomberRange;

    [Header("Selling/Upgrading Setup")]
    public bool selected;
    public GameObject sellButton;

    
    public void placedDownBomberStation()
    {
        isPlaced = true;
        timer = createBomberTimer;
        maxHealth = health;
    }
    
    void Update()
    {
        if(health <= 0)
        {
            FindObjectOfType<BuildingSystem>().removeTile(transform.position);
            Destroy(gameObject);
            Destroy(childBomber);
        }

        // if the player clicks while the bomber station is being sold and it's not over the sell button or the bomber station, set selected to false, remove the outline, and remove the button
        if(Input.GetMouseButtonDown(0) && selected && !isMouseOverUI() && !(Vector3.Distance(BuildingSystem.GetMouseWorldPosition(), transform.position) <= 3f))
        {    
            selected = false;
            sellButton.SetActive(false);
            GetComponent<Outline>().enabled = false;
        }

        if(isPlaced)
        {
            if(first)
            {
                hasBomber = true;
                first = false;
                under = false;
                createBomber();
                releaseBomber();
            }
            // if this station doesn't already have a child bomber and it's waited for it, create one
            else if(childBomber.Equals(null) && createBomberTimer < timer && !hasBomber)
            {
                hasBomber = true;
                under = false;
                bomberStationController.SetBool("Releasing", false);
                bomberStationController.SetBool("Creating", true);
                Invoke("createBomber", 2.5f);
            }
        }

        if(health <= 0)
        {
            // if this turret is behind the factory and it's died, reduce the amount of objects behind the factory
            if(transform.position == BuildingSystem.current.GetFactoryLocation() || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z == BuildingSystem.current.GetFactoryLocation().z) || (transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z && transform.position.x == BuildingSystem.current.GetFactoryLocation().x) || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z))
                FindObjectOfType<FactoryController>().GetComponent<FactoryController>().reduceLockedIn();
        }
        
        timer += Time.deltaTime;
    }

    public void createBomber()
    {
        childBomber = Instantiate(bomber_, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        childBomber.GetComponent<BomberController>().getParentStation(gameObject);
        releaseBomber();
    }

    private void releaseBomber()
    {
        bomberStationController.SetBool("Creating", false);
        bomberStationController.SetBool("Releasing", true);
        Invoke("sendChildBomber", 1.78f);
    }

    private void sendChildBomber()
    {
        if(!childBomber.Equals(null))
        {
            childBomber.GetComponent<BomberController>().sendBomber();
            under = true;
        }
    }

    public void removeBomber()
    {
        hasBomber = false;
        timer = 0f;
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

    // adds half the bomber's health to the resources and deletes the bomber 
    public void sellBomberStation()
    {
        FindObjectOfType<GameManager>().GetComponent<GameManager>().resourcesChange(Mathf.CeilToInt(health / 2f));
        health -= 100;
    }

    // returns the health the bomber should be
    public float getHealth()
    {
        return bomberHealth;
    }

    // returns the speed the bomber should be
    public float getSpeed()
    {
        return bomberSpeed;
    }

    // returns the strength the bomber should be
    public float getStrength()
    {
        return bomberStrength;
    }

    // returns the range the bomber should be
    public int getRange()
    {
        return bomberRange;
    }

    // if the bomber station is not under, or is being healed, change it's health
    public void damageBomberStation(float damageToTake)
    {
        if(!under || damageToTake < 0)
        {   
            health -= damageToTake;
            healthBar.value = health;
            sellButtonTXT.GetComponent<Text>().text = "SELL FOR: " + Mathf.CeilToInt(health / 2f).ToString() + " RESOURCES";
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

    // returns whether or not the bomber station is under the ground
    public bool isUnder()
    {
        return under;
    }

    // returns the bomber station's health
    public float getBomberStationHealth()
    {
        return health;
    }

    // returns the bomber station's max health
    public float getBomberStationMaxHealth()
    {
        return maxHealth;
    }
}
