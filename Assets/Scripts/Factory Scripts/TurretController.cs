using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TurretController : MonoBehaviour
{
    [Header("Bullet Variables")]
    public float bulletSpeed;
    public float bulletDamage;
    public float normalFireRate; 
    public float shotgunFireRate;
    public GameObject normalBody;

    [Header("Rocket Variables")]
    public float rocketSpeed;
    public float rocketStrength;
    public float rocketFireRate;
    public GameObject rocketBody;

    [Header("Initial Setup")]
    public Transform bulletSpawnPosition;
    public GameObject bulletPrefab;
    public GameObject rocketPrefab;
    public float health;
    private float maxHealth;
    private float fireRate;

    [Header("Nearest Enemy Setup")]
    public List<GameObject> AllEnemies;
    public GameObject nearestEnemy;
    public int numberOfEnemies;
    public float distance;
    public float nearestDistance = 99999;
    public int range;
    public float sinceShotTimer;

    [Header("Selling/Upgrading Turret")]
    public bool selected;
    public bool upgradeAble;
    public GameObject sellButton;
    public GameObject sellButtonTXT;
    public GameObject upgradeCurrentTurret;
    public GameObject upgradeCurrentTurretTXT;
    public GameObject upgradeNewTurret;
    public GameObject upgradeNewTurretTXT;

    [Header("Turret Visuals")]
    public GameObject turretCanvas;
    public Slider healthBar;
    public Animator turretController;
    public GameObject floatingTXT;

    [Header("Type of Turret")]
    public bool normal;
    public bool shotgun;
    public bool rocket;

    private float timer;
    public bool isPlaced;

    void Start()
    {
        findNewEnemy();
        maxHealth = health;

        if(rocket)
        {
            rocketBody.SetActive(true);
            normalBody.SetActive(false);
        }

        else if(normal || shotgun)
        {
            normalBody.SetActive(true);
            rocketBody.SetActive(false);
        }
    }

    // is called when the turret is placed down in the BuildingSystem script
    public void placedDownTurret()
    {
        isPlaced = true;
    }
    
    void Update()
    {   
        if(isPlaced) 
        {
            if(health <= 0)
            {
                FindObjectOfType<BuildingSystem>().removeTile(transform.position);
                Destroy(gameObject);
            }

            if(nearestEnemy.Equals(null) || Vector3.Distance(bulletSpawnPosition.position, nearestEnemy.transform.position) > range)
            {
                findNewEnemy();
            }

            // checks when and if the turret can shoot
            if(numberOfEnemies > 0 && nearestEnemy != null && Vector3.Distance(bulletSpawnPosition.position, nearestEnemy.transform.position) < range)
            {
                // turns the turret towards it's target
                transform.LookAt(new Vector3(nearestEnemy.transform.position.x, transform.position.y, nearestEnemy.transform.position.z));
                turretCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);

                if(timer > fireRate && normal)
                {
                    shootOne();
                    timer = 0f;
                }

                else if(timer > fireRate && shotgun)
                {
                    shootShotgun();
                    timer = 0f;
                }
            }

            // if this turret doesn't have an enemy or there are no enemies make it face the enemy spawn position
            else if(numberOfEnemies == 0 || nearestEnemy == null || Vector3.Distance(bulletSpawnPosition.position, nearestEnemy.transform.position) > range)
            {    
                transform.LookAt(new Vector3(50f, 0.5f, 0f));
                turretCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }
            
            // if the player clicks while the turret is being sold and it's not over the sell button or the turret, set selected to false, remove the outline, and remove the button
            if(Input.GetMouseButtonDown(0) && selected && !isMouseOverUI() && !(Vector3.Distance(BuildingSystem.GetMouseWorldPosition(), transform.position) <= 2f))
            {    
                sellButton.SetActive(false);
                selected = false;
                GetComponent<Outline>().enabled = false;
            }
        }

        // makes the turret face the enemy spawn position before it's placed down
        else
            transform.LookAt(new Vector3(50f, 0.5f, 0f));

        if(health <= 0)
        {
            // if this turret is behind the factory and it's died, reduce the amount of objects behind the factory
            if(transform.position == BuildingSystem.current.GetFactoryLocation() || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z == BuildingSystem.current.GetFactoryLocation().z) || (transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z && transform.position.x == BuildingSystem.current.GetFactoryLocation().x) || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z))
                FindObjectOfType<FactoryController>().GetComponent<FactoryController>().reduceLockedIn();
        }

        findFireRate();

        timer += Time.deltaTime;
    }
    
    // creates and fires a bullet after giving it it's damage
    void shootOne()
    {
        turretController.SetBool("shoot", true);
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPosition.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        bullet.GetComponent<BulletController>().isNormal(true); 
        bullet.GetComponent<BulletController>().instantiateDamage(bulletDamage); 
        bullet.GetComponent<BulletController>().instantiateSpeed(bulletSpeed);
        bullet.GetComponent<BulletController>().instantiateSpawnPos(bulletSpawnPosition);
        Invoke("stopshooting", 0.1f);
    }

    // creates and fires four smaller bullets
    void shootShotgun()
    {
        turretController.SetBool("shoot", true);
        
        for(int i = 0; i < 4; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPosition.position + new Vector3(Random.Range(0f, 0.75f), Random.Range(0f, 0.75f), Random.Range(0f, 0.75f)), Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
            bullet.GetComponent<BulletController>().isShotgun(true); 
            bullet.GetComponent<BulletController>().instantiateDamage(bulletDamage); 
            bullet.GetComponent<BulletController>().instantiateSpeed(bulletSpeed);
            bullet.GetComponent<BulletController>().instantiateSpawnPos(bulletSpawnPosition);
        }
        
        Invoke("stopshooting", 0.1f);
    }

    void shootRocket()
    {
        GameObject rocket = Instantiate(rocketPrefab, bulletSpawnPosition.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        rocket.GetComponent<RocketController>().instantiateStrength(rocketStrength);
        rocket.GetComponent<RocketController>().instantiateSpeed(rocketSpeed);
        rocket.GetComponent<RocketController>().instantiateTarget(nearestEnemy);
    }

    // finds the nearest bomber, enemy resource collector, or shooter
    public void findNewEnemy()
    {
        nearestDistance = 99999;
        AllEnemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyBomber"));    
        AllEnemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyResourceCollector")); 
        AllEnemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyShooter")); 
        numberOfEnemies = AllEnemies.Count;   

        for(int i = 0; i < numberOfEnemies; i++)
        {
            distance = Vector3.Distance(bulletSpawnPosition.position, AllEnemies[i].transform.position);

            if(distance < range)
            {
                if(distance < nearestDistance)
                {
                    nearestEnemy = AllEnemies[i];
                    nearestDistance = distance;
                    
                    if(timer < 1)
                        timer = 0.9f;
                }
            }
        }

        AllEnemies.Clear();
    }

    // gives this turret's damage to the bullet it shoots
    public float getBulletDamage()
    {
        return bulletDamage;
    }

    // gives or takes health from this turret
    public void damageTurret(float damageToTake)
    {
        if(isPlaced)
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

    // returns the turret's health
    public float getTurretHealth()
    {
        return health;
    }

    // returns the turret's max health
    public float getTurretMaxHealth()
    {
        return maxHealth;
    }

    // returns true if this turret is placed
    public bool isTurretPlaced()
    {
        return isPlaced;
    }

    // if the mouse is over the turret apply the outline
    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    // if you aren't selling the turret and the mouse is off of the turret, remove the outline
    void OnMouseExit()
    {
        if(!selected)
            GetComponent<Outline>().enabled = false;
    }

    // if the turret is clicked on create the sell turret button and set sellingTurret to true
    void OnMouseDown()
    {
        if(isPlaced)
        {
            selected = true;
            sellButton.SetActive(true);

            if(upgradeAble)
            {
                upgradeCurrentTurret.SetActive(true);
                upgradeNewTurret.SetActive(true);
            }
        }
    }

    // adds half the turret's health to the resources and deletes the turret 
    public void sellTurret()
    {
        FindObjectOfType<GameManager>().GetComponent<GameManager>().resourcesChange(Mathf.CeilToInt(health / 2f));
        health -= 100;
    }

    // stops the shooting animation 
    private void stopshooting()
    {
        turretController.SetBool("shoot", false);
    }

    // if this enemy is in range, target it
    public void targetThis(GameObject enemyToTarget)
    {
        distance = Vector3.Distance(bulletSpawnPosition.position, enemyToTarget.transform.position);

        if(distance < range)
        {
            nearestEnemy = enemyToTarget;
            nearestDistance = distance;
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

    // determines the fire rate depending on what type of turret it is
    void findFireRate()
    {
        if(normal)
            fireRate = normalFireRate;

        else if(shotgun)
            fireRate = shotgunFireRate;

        else if(rocket)
            fireRate = rocketFireRate;
    }
}
