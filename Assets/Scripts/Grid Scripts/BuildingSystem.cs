using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;

    [Header("Grid and Tiles")]
    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap MainTileMap;
    [SerializeField] private TileBase WhiteTile;

    [Header("Factory")]
    public GameObject factory;

    [Header("Turret")]
    public GameObject turret;
    public GameObject turretButton;
    public bool turretButtonPressed;


    [Header("Resource Collector")]
    public GameObject resourceCollector;
    public GameObject resourceCollectorButton;
    public bool resourceCollecterButtonPressed;

    [Header("Bomber")]
    public GameObject bomber;
    public GameObject bomberButton;
    public bool bomberButtonPressed;

    
    [Header("Placing")]
    public bool factoryPlaced;
    public bool placing;
    public Vector3 spawnPosition;
    private PlaceableObject objectToPlace;
    private RaycastScript rayScript;

    [Header("GameManager Reference")]
    public GameManager manager;
    
    // calls before the game starts
    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
        manager = FindObjectOfType<GameManager>();
        rayScript = FindObjectOfType<RaycastScript>();
    }

    private void Update()
    {   
        if(!objectToPlace)
            return;

        if(Input.GetKeyDown(KeyCode.Space) && placing)
        {
            if(CanBePlaced(objectToPlace))
            {
                // placing a turret down
                if(!objectToPlace.Equals(null) && factoryPlaced && turretButtonPressed && manager.resources >= 10)
                {
                    objectToPlace.Place();
                    Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                    TakeArea(start, objectToPlace.Size); 
                    turretButtonPressed = false;
                    placing = false;
                    objectToPlace.GetComponent<TurretController>().placedDownTurret();
                    FindObjectOfType<GameManager>().resourcesChange(-10);
                    
                    // if the turret is behind the factory, increase the amount of objects behind the factory
                    if(objectToPlace.GetComponent<ObjectDrag>().isBehind())
                        FindObjectOfType<FactoryController>().GetComponent<FactoryController>().makeSeeThroughAlways();
                }
                
                // placing a resource collector down
                else if(!objectToPlace.Equals(null) && factoryPlaced && resourceCollecterButtonPressed && manager.resources >= 5)
                {
                    objectToPlace.Place();
                    Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                    TakeArea(start, objectToPlace.Size); 
                    resourceCollecterButtonPressed = false;
                    placing = false;
                    objectToPlace.GetComponent<ResourceCollectorController>().placedDownResourceCollector();
                    FindObjectOfType<GameManager>().resourcesChange(-5);
                    
                    // if the resource collector is behind the factory, increase the amount of objects behind the factory
                    if(objectToPlace.GetComponent<ObjectDrag>().isBehind())
                        FindObjectOfType<FactoryController>().GetComponent<FactoryController>().makeSeeThroughAlways();
                }

                // placing a bomber station down
                else if(!objectToPlace.Equals(null) && factoryPlaced && bomberButtonPressed && manager.resources >= 15)
                {
                    objectToPlace.Place();
                    Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                    TakeArea(start, objectToPlace.Size); 
                    bomberButtonPressed = false;
                    placing = false;
                    objectToPlace.GetComponent<BomberStationController>().placedDownBomberStation();
                    FindObjectOfType<GameManager>().resourcesChange(-15);
                    
                    // if the resource collector is behind the factory, increase the amount of objects behind the factory
                    if(objectToPlace.GetComponent<ObjectDrag>().isBehind())
                        FindObjectOfType<FactoryController>().GetComponent<FactoryController>().makeSeeThroughAlways();
                }
                
                // placing the factory, setting the object spawn position, and disabling the factory boxcollider
                else if(!objectToPlace.Equals(null) && !factoryPlaced)
                {
                    objectToPlace.Place();
                    Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                    TakeArea(start, objectToPlace.Size); 
                    factoryPlaced = true;
                    placing = false;
                    spawnPosition = FindObjectOfType<PlaceableObject>().GetStartPosition();
                    spawnPosition.Set(spawnPosition.x + 1f, spawnPosition.y - 2f, spawnPosition.z + 1f);
                    objectToPlace.GetComponent<BoxCollider>().enabled = false;
                }
            }

            // Failing to place the object on a non-taken tile will delete the object
            else
            {
                if(!rayScript.heartExists)
                {
                    Destroy(objectToPlace.gameObject);
                    placing = false;
                }
            }
        }

        // Backing out of placing down the object
        else if(Input.GetKeyDown(KeyCode.Escape) && placing && !rayScript.heartExists)
        {
            // if this object is behind the factory, set the factory back to normal
            if(objectToPlace.transform.position == GetFactoryLocation() || (objectToPlace.transform.position.x - 3 == GetFactoryLocation().x && objectToPlace.transform.position.z == GetFactoryLocation().z) || (objectToPlace.transform.position.z - 3 == GetFactoryLocation().z && objectToPlace.transform.position.x == GetFactoryLocation().x) || (objectToPlace.transform.position.x - 3 == GetFactoryLocation().x && objectToPlace.transform.position.z - 3 == GetFactoryLocation().z))
                FindObjectOfType<FactoryController>().GetComponent<FactoryController>().setNormal();
            
            Destroy(objectToPlace.gameObject);
            placing = false;
        }
    }

    // Sends the factory location so the factory attackers know where to go
    public Vector3 GetFactoryLocation()
    {
        return spawnPosition; 
    }

    // Returns the mouse position
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit raycastHit))
            return raycastHit.point;
        
        else
            return Vector3.zero;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tileMap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach(var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tileMap.GetTile(pos);
            counter ++;
        }
        
        return array;
    }

    // creates the object prefab
    public void InitializeWithObject(GameObject prefab)
    {
        placing = true;

        // factory spawn position
        if(factoryPlaced == false)
            spawnPosition = SnapCoordinateToGrid(new Vector3(50f, -2f, 50f));

        // creates an object at spawnPosition inside the WorldObjectHolder and adds the ObjectDrag script to it
        GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();
    }

    // checks if the object is on white tiles
    private bool CanBePlaced(PlaceableObject placeableObject)
    {
        BoundsInt area = new BoundsInt();
        area.position = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
        area.size = placeableObject.Size;
        area.size = new Vector3Int(area.size.x + 1, area.size.y + 1, area.size.z);

        TileBase[] baseArray = GetTilesBlock(area, MainTileMap);
        
        foreach(var a in baseArray)
        {
            if(a == WhiteTile)
            {
                return false;
            }
        }
    
        return true;
    }

    // Creates a white tile underneath the object after its been placed down
    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTileMap.BoxFill(start, WhiteTile, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    public Vector3 getSpawnArea()
    {
        // if the factory is placed return it's position as the place to spawn turrets
        if(factoryPlaced)
            return spawnPosition;

        return Vector3.zero;
    }

    public bool isFactoryPlaced()
    {
        // if factory is not placed return true
        if(!factoryPlaced)
            return true;

        // if the factory if placed return false
        return false;
    }

    public void createFactory()
    {
        if(placing == false && factoryPlaced == false) 
        {
            InitializeWithObject(factory);
        }
    }

    public void createTurret()
    {
        turretButtonPressed = true;
        resourceCollecterButtonPressed = false;
        bomberButtonPressed = false;

        
        if(placing == false && factoryPlaced == true) 
        {
            InitializeWithObject(turret);
        }
    }

    public void createResourceCollector()
    {
        resourceCollecterButtonPressed = true;
        turretButtonPressed = false;
        bomberButtonPressed = false;
        
        if(placing == false && factoryPlaced == true) 
        {
            InitializeWithObject(resourceCollector);
        }
    }

    public void createBomber()
    {
        bomberButtonPressed = true;
        turretButtonPressed = false;
        resourceCollecterButtonPressed = false;
        
        if(placing == false && factoryPlaced == true) 
        {
            InitializeWithObject(bomber);
        }
    }

    public void removeTile(Vector3 objectPosition)
    {
        Vector3Int position = grid.WorldToCell(objectPosition);
        MainTileMap.SetTile(position, null);
    }

    // returns true if you are currently placing something
    public bool isPlacing()
    {
        return placing;
    }

    // changes placing to _
    public void placeSomthing(bool whatToDo)
    {
        placing = whatToDo;
    }
}