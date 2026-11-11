using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject resourceCollector_;
    public GameObject bomber_;
    public GameObject shooter_;

    [Header("Waves")]
    public GameObject StartWaveButton;
    public GameObject StartWaveTXT;
    public int wave = 1;
    public List<GameObject> enemies;

    public void startSpawning()
    {
        if(!FindObjectOfType<BuildingSystem>().isFactoryPlaced())
        {
            StartCoroutine(testWave(wave));
            wave++;
            StartWaveButton.SetActive(false);
            StartWaveTXT.GetComponent<Text>().text = "START WAVE: " + wave.ToString();
        }
    }
    
    IEnumerator testWave(int waveNum)
    {
        for(int i = 0; i < (waveNum * 2); i++)
        {
            int randomEnemy = Random.Range(0, 3);
            if(randomEnemy == 0)
            {
                spawnShooter();
                yield return new WaitForSeconds(1f);
            }

            else if(randomEnemy == 1)
            {
                spawnResourceCollector();
                yield return new WaitForSeconds(1f);
            }

            else if(randomEnemy == 2)
            {
                spawnBomber();
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void Update()
    {
        enemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyResourceCollector"));
        enemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyBomber"));
        enemies.AddRange(GameObject.FindGameObjectsWithTag("EnemyShooter"));

        if(enemies.Count == 0)
            StartWaveButton.SetActive(true);

        enemies.Clear();
    }

    private void spawnResourceCollector()
    {
        Vector3 spawnPos = transform.position;
        GameObject ResourceCollectorEnemy = Instantiate(resourceCollector_, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
    }

    private void spawnBomber()
    {
        Vector3 spawnPos = transform.position;
        GameObject bomber = Instantiate(bomber_, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
    }

    private void spawnShooter()
    {
        Vector3 spawnPos = transform.position;
        GameObject shooter = Instantiate(shooter_, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
    }
}
