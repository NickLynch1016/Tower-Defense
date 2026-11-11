using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManager;

public class TutorialScript : MonoBehaviour
{
    [Header("Arrow")]
    public GameObject arrowImage;
    public Animator arrowController;

    [Header("Text")]
    public GameObject Text;

    [Header("Step1")]
    public bool step1;
    public bool onFactoryStart;
    public bool arrowToFactory;
    public GameObject factory;
    public FactoryController factoryCaller;

    [Header("Step2")]
    public bool step2;
    public bool draggedTurret;
    public GameObject turretButton;
    public GameObject resourcesTXT;
    public GameObject turret;

    [Header("Step3")]
    public bool step3;
    public bool startwave;
    public bool wave1;
    public bool wave2;
    public GameObject startButton;
    public GameObject startButtonTXT;
    public GameObject factoryHealthTXT;
    public GameObject shooter_;
    public GameObject resourceCollector_;
    public GameObject bomber_;

    [Header("Step4")]
    public bool step4;
    public bool collectorPlacedCheck;
    public bool draggedResourceCollector;
    public bool placingResourceCollector;
    public bool gotResource;
    public bool changeResourceCollectorSettings;
    public GameObject resourceCollectorButton;
    public GameObject resourceCollector;
    public GameObject resourceCollectorSettingsButton;
    public GameObject exampleResource;

    [Header ("Step5")]
    public bool step5;
    public bool wave3;
    public bool startWave3;
    public bool secondTurretCall;
    public bool secondTurretCreated;
    public List<GameObject> turrets;

    void Start()
    {
        arrowController.SetBool("arrowToFactory", true);
        turretButton.SetActive(false);
    }
    
    void Update()
    {
        if(true)
        {
            startButton.SetActive(false);
        }
        
        // step 1
        if(!step1)
        {
            // if the factory is not created flash the arrow next to the factory button, once they create the factory, urge the player to drag the factory with the arrow
            if(FindObjectOfType<BuildingSystem>().GetComponent<BuildingSystem>().isFactoryPlaced())
            {
                factory = GameObject.FindGameObjectWithTag("Factory");

                // when the factory is placed for the first time, the arrow will be reset and the a factory call will be created
                if(factory != null && !onFactoryStart)
                {
                    arrowImage.SetActive(false);
                    arrowImage.SetActive(true);
                    arrowController.SetBool("arrowToFactory", false);
                    arrowController.SetBool("draggingFactory", true);
                    factoryCaller = FindObjectOfType<FactoryController>();
                    onFactoryStart = true;
                    arrowToFactory = true;
                }

                if(factory != null)
                {
                    // if the factory is not placed but the mouse has been dragged, urge the player to place it with some moving text and remove the arrow
                    if(Input.GetMouseButtonDown(0))
                    {
                        Text.SetActive(true);
                        arrowImage.SetActive(false);
                        arrowController.SetBool("draggingFactory", false);
                    }
                }
            }

            // if the factory is placed remove the text to end step 1
            else
            {
                Text.SetActive(false);
                step1 = true;
                arrowImage.SetActive(true);
                arrowController.SetBool("arrowToTurret", true);
                turretButton.SetActive(true);
            }
        }

        // step 2
        else if(step1 && !step2)
        {   
            turret = GameObject.FindGameObjectWithTag("Turret");

            // once they create the turret but haven't yet placed it or dragged it, urge the player to drag the turret with moving text
            if(turret != null && !turret.GetComponent<TurretController>().isTurretPlaced() && !draggedTurret)
            {
                arrowController.SetBool("arrowToTurret", false);
                arrowImage.SetActive(false);
                resourcesTXT.SetActive(true);
                Text.SetActive(true);
                Text.GetComponent<Text>().text = "DRAG TURRET";

                if(Input.GetMouseButtonDown(0))
                    draggedTurret = true;
            }

            // once the turret is placed end step 2
            else if(turret != null && turret.GetComponent<TurretController>().isTurretPlaced())
            {
                Text.SetActive(false);
                step2 = true;

            }
            
            // if the turret has been dragged but not placed, urge the player to place it by flashing the text
            else if(draggedTurret)
            {
                Text.GetComponent<Text>().text = "PRESS SPACE TO PLACE";
            }
        }

        // step 3
        else if(step2 && !step3)
        {
            GameObject shooter = GameObject.FindGameObjectWithTag("EnemyShooter");
            GameObject resourceCollector = GameObject.FindGameObjectWithTag("EnemyResourceCollector");
            
            // if the wave hasn't been started yet, flash the arrow next to the button
            if(!wave1)
            {
                startButton.SetActive(true);
                arrowImage.SetActive(true);
                factoryHealthTXT.SetActive(true);
                arrowController.SetBool("arrowToStart", true);
            }

            // if wave 1 is done show the start button and arrow
            else if(shooter == null && wave1 && !wave2)
            {
                startButton.SetActive(true);
                arrowImage.SetActive(true);
                arrowController.SetBool("arrowToStart", true);
            }

            else
            {
                arrowImage.SetActive(false);
                startButton.SetActive(false);
            }

            // if wave 2 is done end step 3
            if(wave2 && shooter == null && resourceCollector == null)
                step3 = true;
        }

        // step 4
        else if(step3 && !step4)
        {
            resourceCollector = GameObject.FindGameObjectWithTag("ResourceCollector");
            
            // if the player hasn't created a resource collector yet, and urge them to by flashing the arrow next to the button
            if(resourceCollector == null && !collectorPlacedCheck)
            {
                resourceCollectorButton.SetActive(true);
                arrowImage.SetActive(true);
                arrowController.SetBool("arrowToResourceCollector", true);
                collectorPlacedCheck = true;
            }
            
            // if the player creates the resource collector urge them to drag it with text, and remove the arrow
            else if(resourceCollector != null && !resourceCollector.GetComponent<ResourceCollectorController>().isResourceCollectorPlaced() && !draggedResourceCollector)
            {
                Text.SetActive(true);
                arrowImage.SetActive(false);
                Text.GetComponent<Text>().text = "DRAG RESOURCE COLLECTOR";

                if(Input.GetMouseButtonDown(0))
                {
                    draggedResourceCollector = true;
                    Text.GetComponent<Text>().text = "PRESS SPACE TO PLACE";
                }
            }

            else if(resourceCollector != null && resourceCollector.GetComponent<ResourceCollectorController>().isResourceCollectorPlaced() && !placingResourceCollector)
            {
                Text.SetActive(false);
                arrowImage.SetActive(false);
                placingResourceCollector = true;
            }

            // if the player places the resource collector, wait until it's back, then ask them to change the settings
            else if(resourceCollector != null && resourceCollector.GetComponent<ResourceCollectorController>().isResourceCollectorPlaced() && FindObjectOfType<GameManager>().getResources() == 5 && !gotResource)
            {
                resourceCollectorSettingsButton.SetActive(true);
                arrowImage.SetActive(true);
                arrowController.SetBool("arrowToResourceCollectorSettings", true);
                gotResource = true;
            }

            // if the player changed the resource collector settings create a resource and ask them to press the send resource collector button
            else if(!FindObjectOfType<GameManager>().getResourceCollectorSettings() && !changeResourceCollectorSettings)
            {
                arrowImage.SetActive(false);
                arrowImage.SetActive(true);
                exampleResource.SetActive(true);
                arrowController.SetBool("arrowToSendResourceCollectorButton", true);
                changeResourceCollectorSettings = true;
            }
            
            // if the player changed the setting and pressed send resource collector, remove the arrow
            if(!FindObjectOfType<GameManager>().getResourceCollectorSettings() && GameObject.FindGameObjectWithTag("ResourceCollector").GetComponent<ResourceCollectorController>().isResourceCollectorSent() && FindObjectOfType<GameManager>().getResources() == 5)
                arrowImage.SetActive(false);

            // if the resource collector came back after being sent end step 4
            else if(!FindObjectOfType<GameManager>().getResourceCollectorSettings() && FindObjectOfType<GameManager>().getResources() == 10)
                step4 = true;
        }

        // step 5
        if(step4 && !step5)
        {
            turrets.AddRange(GameObject.FindGameObjectsWithTag("Turret"));
            // when step 5 starts, reset the arrow image and make it point at the turret button
            if(!secondTurretCall)
            {
                arrowImage.SetActive(true);
                arrowController.SetBool("arrowToTurret", true);
                secondTurretCall = true;
            }

            // if the player has created a turret show the text to place it
            else if(turrets.Count == 2 && !secondTurretCreated)
            {
                arrowImage.SetActive(false);
                Text.SetActive(true);
                Text.GetComponent<Text>().text = "DRAG THEN PLACE";
                secondTurretCreated = true;
            }

            // if the player has placed the second turret then point the arrow at the start wave button
            else if(turrets.Count == 2 && turrets[1].GetComponent<TurretController>().isTurretPlaced() && !wave3)
            {
                startButton.SetActive(true);
                Text.SetActive(false);
                arrowImage.SetActive(true);
                arrowController.SetBool("arrowToStart", true);
            }

            turrets.Clear();
        }
    }

    public void startWave()
    {
        // on wave 1 spawn one shooter
        if(!wave1)
        {
            wave1 = true;
            startButtonTXT.GetComponent<Text>().text = "START WAVE: 2";
            spawnShooter();
        }

        // on wave 2 spawn a shooter wait a couple seconds, then spawn a resource collector
        else if(!wave2)
        {
            startButtonTXT.GetComponent<Text>().text = "START WAVE: 3";
            spawnShooter();
            Invoke("spawnResourceCollector", 3f);
            wave2 = true;
        }

        else if(!wave3)
        {
            arrowImage.SetActive(false); 
            startButtonTXT.GetComponent<Text>().text = "START WAVE: 4";
            spawnShooter();
            Invoke("spawnBomber", 1.5f);
            Invoke("spawnResourceCollector", 3f);
            wave3 = true;
        }

    }

    private void spawnResourceCollector()
    {
        GameObject resourceCollector = Instantiate(resourceCollector_, new Vector3(50f, 0.5f, -5f), Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
    }

    private void spawnBomber()
    {
        GameObject bomber = Instantiate(bomber_, new Vector3(50f, 0.5f, -5f), Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
    }

    private void spawnShooter()
    {
        GameObject shooter = Instantiate(shooter_, new Vector3(50f, 0.5f, -5f), Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
    }
}
