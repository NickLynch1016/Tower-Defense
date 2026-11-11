using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager current;
    
    [Header("GamePlay Canvas")]
    public GameObject gamePlayCanvas;
    
    [Header("GameOver Canvas")]
    public GameObject gameOverCanvas;

    [Header("Factory Setup")]
    public GameObject factoryHealthTXT;
    public int factoryHealth;

    [Header("Resources Setup")]
    public GameObject resourcesTXT;
    public int resources;
    
    [Header("Resource Collector Setup")]
    public List<GameObject> resourceCollectors;
    public GameObject ResourceCollectorSendButton;
    public GameObject onPressTXT;
    public bool alwaysOrOnPress = true;

    void Update()
    {
        if(factoryHealth <= 0)
            lose();
    }

    // decreases the health of the factory and changes the text
    public void healthDecrease(int healthToTake)
    {
        factoryHealth -= healthToTake;
        FindObjectOfType<FactoryController>().changeHealth(healthToTake);
        factoryHealthTXT.GetComponent<Text>().text = "Factory Health: " + factoryHealth.ToString();
    }

    // changes the resource amount and changes the text
    public void resourcesChange(int i)
    {
        resources += i;
        resourcesTXT.GetComponent<Text>().text = "Resources: " + resources.ToString();
    }

    // returns the current amount of resources
    public int getResources()
    {
        return resources;
    }

    // returns the current state of the resource collector settings
    public bool getResourceCollectorSettings()
    {
        return alwaysOrOnPress;
    }

    // stops the game and sets the game over canvas to active
    public void lose()
    {
        gamePlayCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    // switches the resource collector setting based on what it was before
    public void changeResourceCollectorSettings()
    {
        if(alwaysOrOnPress)
        {
            alwaysOrOnPress = false;
            ResourceCollectorSendButton.SetActive(true);
            onPressTXT.GetComponent<Text>().text = "SEND RESOURCE COLLECTORS: ON PRESS";
            setButtonToOnPress();
        }

        else
        {
            alwaysOrOnPress = true;
            ResourceCollectorSendButton.SetActive(false);
            onPressTXT.GetComponent<Text>().text = "SEND RESOURCE COLLECTORS: ALWAYS";
        }
    }

    // sends all the resource collector when the send resource collectors button is pressed
    public void sendResourceCollectors()
    {
        resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));

        for(int i = 0; i < resourceCollectors.Count; i++)
        {
            resourceCollectors[i].GetComponent<ResourceCollectorController>().sendResourceCollector();
        }

        resourceCollectors.Clear();
    }

    // sends all the resource collectors back to their station if the button is set to on press
    public void setButtonToOnPress()
    {
        resourceCollectors.AddRange(GameObject.FindGameObjectsWithTag("ResourceCollector"));

        for(int i = 0; i < resourceCollectors.Count; i++)
        {
            resourceCollectors[i].GetComponent<ResourceCollectorController>().setToOnPress();
        }

        resourceCollectors.Clear();
    }
}
