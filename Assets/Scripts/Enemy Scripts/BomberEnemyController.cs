using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BomberEnemyController : MonoBehaviour
{
    [Header("Bomber Stats")]
    public float health;
    public float speed;
    public int strength;
    public int range;

    [Header("Target Position")]
    public Vector3 factoryPosition;
    public GameObject enemyBase;
    private bool foundFactory;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    public float explosionDisplayTime;
    public List<GameObject> turrets;
    public List<GameObject> resourceCollectors;
    public List<GameObject> bombers;
    public List<GameObject> bomberStations;

    [Header("Bomber Visuals")]
    public GameObject HealthBarCanvas;
    public Slider healthBar;
    public bool selected;
    public GameObject floatingTXT;

    void Start()
    {
        factoryPosition = FindObjectOfType<BuildingSystem>().GetFactoryLocation();
        factoryPosition.Set(factoryPosition.x, 0.5f, factoryPosition.z);
    }
    
    void Update()
    {   
        // once the factory is in range the bomber's speed will increase
        if(Vector3.Distance(transform.position, factoryPosition) < range && !foundFactory)
        {
            speed *= 2;
            foundFactory = true;
        }

        // if the factory is in range the bomber will move and turn to it
        if(Vector3.Distance(transform.position, factoryPosition) < range)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, factoryPosition, speed * Time.deltaTime);
            transform.position = pos;

            transform.LookAt(new Vector3(factoryPosition.x, transform.position.y, factoryPosition.z));
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        // otherwise it will just stay in line with the resource collectors and shooters
        else
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, enemyBase.transform.position, speed * Time.deltaTime);
            HealthBarCanvas.transform.rotation = Quaternion.Euler(0, 45, 0);
            transform.position = pos;
        }

        // if the factory attacker reaches the factory or it's health goes to zero or below it will explode
        if(transform.position == factoryPosition || health <= 0)
        {
            explode();
            Destroy(gameObject);
        }

        // if the player clicks off of the bomber, remove the outline
        if(Input.GetMouseButtonDown(0) && selected && !(Vector3.Distance(BuildingSystem.GetMouseWorldPosition(), transform.position) <= 2f))
        {
            selected = false;
            GetComponent<Outline>().enabled = false;
        }

        // if the object is no longer selected remove the outline
        if(!selected)
            GetComponent<Outline>().enabled = false;
    }

    private void explode()
    {
        // damages the factory based on it's distance to it
        float explosionDamageToFactory = (-1f/3f) * (Vector3.Distance(transform.position, factoryPosition)) + strength;
        if(Mathf.RoundToInt(explosionDamageToFactory) > 0)
            FindObjectOfType<GameManager>().healthDecrease(Mathf.RoundToInt(explosionDamageToFactory));

        // damages every turret based on it's distance to them
        turrets.AddRange(GameObject.FindGameObjectsWithTag("Turret"));
        for(int i = 0; i < turrets.Count; i++)
        {
            float explosionDamageToTurret = (-1f/3f) * (Vector3.Distance(transform.position, turrets[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToTurret) > 0)
                turrets[i].GetComponent<TurretController>().damageTurret(Mathf.RoundToInt(explosionDamageToTurret));
        }

        // damages every resource collector based on it's distance to them
        resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));
        for(int i = 0; i < resourceCollectors.Count; i++)
        {
            float explosionDamageToResourceCollector = (-1f/3f) * (Vector3.Distance(transform.position, resourceCollectors[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToResourceCollector) > 0)
                resourceCollectors[i].GetComponent<ResourceCollectorController>().damageResourceCollector(Mathf.RoundToInt(explosionDamageToResourceCollector));
        }

        // damages every bomber based on it's distance to them
        bombers.AddRange(GameObject.FindGameObjectsWithTag("Bomber"));
        for(int i = 0; i < bombers.Count; i++)
        {
            float explosionDamageToBomber = (-1f/3f) * (Vector3.Distance(transform.position, bombers[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToBomber) > 0)
                bombers[i].GetComponent<BomberController>().damageBomber(Mathf.RoundToInt(explosionDamageToBomber));
        }

        // damages every bomber station based on it's distance to them
        bomberStations.AddRange(GameObject.FindGameObjectsWithTag("BomberStation"));
        for(int i = 0; i < bombers.Count; i++)
        {
            float explosionDamageToBomberStation = (-1f/3f) * (Vector3.Distance(transform.position, bomberStations[i].transform.position)) + strength;
            if(Mathf.RoundToInt(explosionDamageToBomberStation) > 0)
                bomberStations[i].GetComponent<BomberStationController>().damageBomberStation(Mathf.RoundToInt(explosionDamageToBomberStation));
        }

        explosionEffects();
        turrets.Clear();
        resourceCollectors.Clear();
        bombers.Clear();
        bomberStations.Clear();
    }

    // handles the explosion effects
    public void explosionEffects()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, explosionDisplayTime);
    }

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

    // if the mouse is over the bomber apply the outline
    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    // if you aren't selecting the bomber and the mouse is off of the bomber, remove the outline
    void OnMouseExit()
    {
        if(!selected)
            GetComponent<Outline>().enabled = false;
    }

    // if the bomber is clicked select the bomber
    void OnMouseDown()
    {
        selected = true;
        turrets.AddRange(GameObject.FindGameObjectsWithTag("Turret"));
        
        for(int i = 0; i < turrets.Count; i++)
        {
            turrets[i].GetComponent<TurretController>().targetThis(gameObject);
        }

        turrets.Clear();
    }

    public void deselect()
    {
        selected = false;
    }
}