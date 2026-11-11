using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FactoryController : MonoBehaviour
{
    [Header("Changing Materials")]
    public Material normal;
    public Material seeThrough;
    public bool isSeeThrough;
    public int objectsBehind = 0;

    [Header("Selecting Setup")]
    public bool selected;

    [Header("Factory Canvas Setup")]
    public Slider healthBar;

    public GameObject floatingTXT;
    
    public void setSeeThrough()
    {
        GetComponent<MeshRenderer>().material = seeThrough;
        isSeeThrough = true;

    }

    public void setNormal()
    {   
        if(objectsBehind <= 0)
        {
            GetComponent<MeshRenderer>().material = normal;
            isSeeThrough = false;
            objectsBehind = 0;
        }
    }

    public void makeSeeThroughAlways()
    {
        objectsBehind += 1;
    }

    public void reduceLockedIn()
    {
        objectsBehind --;
        setNormal();
    }

    void OnMouseOver()
    {
        GetComponent<Outline>().enabled = true;
    }

    public void changeHealth(int damageToTake)
    {
        healthBar.value -= damageToTake;
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
        var go = Instantiate(floatingTXT, transform.position, Quaternion.identity, transform);
        go.GetComponent<TextMesh>().text = DTK.ToString();
    }
}
