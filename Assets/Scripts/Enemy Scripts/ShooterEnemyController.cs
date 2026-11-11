using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShooterEnemyController : MonoBehaviour
{   
    [Header("Shooter Stats")]
    public float bulletSpeed;
    public int bulletDamage;
    public float fireRate; 
    public float speed;
    public float health;

    [Header("Movement Points")]
    public List<GameObject> movementPoints;
    public bool factoryInWay;
    public int currentMovementPoint;

    [Header("Nearest Target Setup")]
    public List<GameObject> allTargets;
    public GameObject nearestTarget;
    public int numberOfTargets;
    public float distance;
    public float nearestDistance = 99999;
    public int range;

    [Header("Shooting Setup")]
    public Transform bulletSpawnPosition;
    public GameObject enemyBulletPrefab;

    [Header("Shooter Visuals")]
    public GameObject HealthBarCanvas;
    public Slider healthBar;
    public Animator shooterController;
    public GameObject floatingTXT;

    [Header("Selecting Setup")]
    public bool selected;
    public List<GameObject> turrets;

    private float timer;

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
        // if the shooter's health is 0 or it's at the base, destory it
        if(health <= 0)
            Destroy(gameObject);

        // if the shooter doesn't have a nearest target or it's current one is out of range or the current one is a bomber station, find a new target
        if(nearestTarget.Equals(null) || Vector3.Distance(bulletSpawnPosition.position, nearestTarget.transform.position) > range || (!nearestTarget.Equals(null) && nearestTarget.CompareTag("BomberStation")))
        {
            findNearestTarget();
        }
        
        // if the shooter has a target in range, look at it and shoot it
        if(numberOfTargets > 0 && !nearestTarget.Equals(null) && Vector3.Distance(bulletSpawnPosition.position, nearestTarget.transform.position) < range)
        {
            transform.LookAt(new Vector3(nearestTarget.transform.position.x, transform.position.y, nearestTarget.transform.position.z));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);

            // checks when and if the shooter can shoot
            if(timer > fireRate)
            {
                shoot();
                timer = 0f;
            }
        }

        // if the shooter doesn't have a target look towards the enemy base
        else
        {
            transform.LookAt(new Vector3(movementPoints[4].transform.position.x, transform.position.y, movementPoints[4].transform.position.z));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            
        }

        // if the factory is in the way move and around it
        if(factoryInWay)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, movementPoints[currentMovementPoint].transform.position, speed * Time.deltaTime);
            transform.position = pos;

            // if the shooter doesn't have a target, look to the next movement point
            if(nearestTarget.Equals(null))
            {
                transform.LookAt(new Vector3(movementPoints[currentMovementPoint].transform.position.x, transform.position.y, movementPoints[currentMovementPoint].transform.position.z));
                HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            }
        }

        // if the factory is not in the way, just move towards the enemy base
        else
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, movementPoints[4].transform.position, speed * Time.deltaTime);
            transform.position = pos;
        }

        // if the enemy reached the base, delete them
        if(transform.position == movementPoints[4].transform.position)
            Destroy(gameObject);
            
        // if the enemy reached the current movement point, change to the next one
        else if(transform.position == movementPoints[currentMovementPoint].transform.position)
            currentMovementPoint++;

        // if the player clicks off of the shooter, remove the outline
        if(Input.GetMouseButtonDown(0) && selected && !(Vector3.Distance(BuildingSystem.GetMouseWorldPosition(), transform.position) <= 2f))
        {
            selected = false;
            GetComponent<Outline>().enabled = false;
        }

        timer += Time.deltaTime;
    }

    void shoot()
    {
        shooterController.SetBool("shoot", true);
        GameObject bullet = Instantiate(enemyBulletPrefab, bulletSpawnPosition.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        bullet.GetComponent<EnemyBulletController>().instantiateDamage(bulletDamage); 
        bullet.GetComponent<EnemyBulletController>().instantiateSpeed(bulletSpeed);
        bullet.GetComponent<EnemyBulletController>().instantiateSpawnPos(bulletSpawnPosition);
        Invoke("stopShooting", 0.1f);
    }

    private void findNearestTarget()
    {
        nearestDistance = 99999;
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Turret"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("Bomber"));
        allTargets.AddRange(GameObject.FindGameObjectsWithTag("BomberStation"));
        numberOfTargets = allTargets.Count;

        for(int i = 0; i < allTargets.Count; i++)
        {
            distance = Vector3.Distance(bulletSpawnPosition.position, allTargets[i].transform.position);

            // if this target is in range and is the closest target and is a bomber station thats not under, or isn't a bomber station, set this to be the target
            if(distance < range && distance < nearestDistance && ((allTargets[i].CompareTag("BomberStation") && !allTargets[i].GetComponent<BomberStationController>().isUnder()) || !allTargets[i].CompareTag("BomberStation")))
            {   
                nearestTarget = allTargets[i];
                nearestDistance = distance;
                    
                if(timer < 1)
                    timer = 0.9f;
            }
        }

        allTargets.Clear();
    }

    public void damageShooter(float damageToTake)
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

    // stops the shooting animation 
    private void stopShooting()
    {
        shooterController.SetBool("shoot", false);
    }

    // if the mouse is over the shooter apply the outline
    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    // if you aren't selecting the shooter and the mouse is off of the shooter, remove the outline
    void OnMouseExit()
    {
        if(!selected)
            GetComponent<Outline>().enabled = false;
    }

    // if the shooter is clicked select the shooter and have all defenses target it
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

    // deselects this shooter when called
    public void deselect()
    {
        selected = false;
    }
}
