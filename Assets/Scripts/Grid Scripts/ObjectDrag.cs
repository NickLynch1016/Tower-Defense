using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offset;
    public int size = 5;
    public bool seeThrough;
    
    void Update()
    {
        if(!FindObjectOfType<BuildingSystem>().GetComponent<BuildingSystem>().isFactoryPlaced())
        {
            if(transform.position == BuildingSystem.current.GetFactoryLocation() || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z == BuildingSystem.current.GetFactoryLocation().z) || (transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z && transform.position.x == BuildingSystem.current.GetFactoryLocation().x) || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z) && !seeThrough)
            {
                FindObjectOfType<FactoryController>().GetComponent<FactoryController>().setSeeThrough();
                seeThrough = true;
            }

            else if(transform.position != BuildingSystem.current.GetFactoryLocation() && !(transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z == BuildingSystem.current.GetFactoryLocation().z) && !(transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z && transform.position.x == BuildingSystem.current.GetFactoryLocation().x) && !(transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z) && seeThrough)
            {
                FindObjectOfType<FactoryController>().GetComponent<FactoryController>().setNormal();
                seeThrough = false;
            }
        }
    }
    
    // gets the position of the mouse
    private void OnMouseDown()
    {
        offset = transform.position - BuildingSystem.GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        // current position
        Vector3 pos = BuildingSystem.GetMouseWorldPosition() + offset;

        // the area around the factory in which you can build on
        Vector3 area = BuildingSystem.current.getSpawnArea();

        // checks if the factory is placed
        if(BuildingSystem.current.isFactoryPlaced()) 
            transform.position = BuildingSystem.current.SnapCoordinateToGrid(pos);
        
        // controlls the range of the area you can place turrets down
        else if(BuildingSystem.current.SnapCoordinateToGrid(pos).x < BuildingSystem.current.SnapCoordinateToGrid(area).x + size
            && BuildingSystem.current.SnapCoordinateToGrid(pos).x > BuildingSystem.current.SnapCoordinateToGrid(area).x - size
            && BuildingSystem.current.SnapCoordinateToGrid(pos).z < BuildingSystem.current.SnapCoordinateToGrid(area).z + size
            && BuildingSystem.current.SnapCoordinateToGrid(pos).z > BuildingSystem.current.SnapCoordinateToGrid(area).z - size)
            transform.position = BuildingSystem.current.SnapCoordinateToGrid(pos);
    }

    public bool isBehind()
    {
        if(transform.position == BuildingSystem.current.GetFactoryLocation() || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z == BuildingSystem.current.GetFactoryLocation().z) || (transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z && transform.position.x == BuildingSystem.current.GetFactoryLocation().x) || (transform.position.x - 3 == BuildingSystem.current.GetFactoryLocation().x && transform.position.z - 3 == BuildingSystem.current.GetFactoryLocation().z))
            return true;

        else 
            return false;
    }
}
