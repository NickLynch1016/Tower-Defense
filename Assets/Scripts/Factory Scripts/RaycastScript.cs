using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastScript : MonoBehaviour
{
    Camera cam;
    public LayerMask mask;
    public GameObject heart_;
    public bool heartExists;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 100f;
        mousePos = cam.ScreenToWorldPoint(mousePos);
        Debug.DrawRay(transform.position, mousePos - transform.position, Color.yellow);
        
        // if you click on the factory tile while nothing is being placed currently, a heart will spawn
        if(Input.GetMouseButtonDown(0) && !BuildingSystem.current.isFactoryPlaced() && !BuildingSystem.current.isPlacing())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 150, mask) && !heartExists)
            {
                Vector3 spawnPos = new Vector3(BuildingSystem.current.GetFactoryLocation().x, 0f, BuildingSystem.current.GetFactoryLocation().z);
                GameObject heart = Instantiate(heart_, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
                heart.AddComponent<ObjectDrag>();
                heartExists = true;
                BuildingSystem.current.placeSomthing(true);
            }
        }
    }

    public void removeHeart()
    {
        heartExists = false;
        BuildingSystem.current.placeSomthing(false);
    }
}
