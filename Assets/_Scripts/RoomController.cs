using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{

    public List<GameObject> playerSpawnPositions = new List<GameObject>();
    public List<GameObject> enemySpawnPositions = new List<GameObject>();
    private int playerExitCount = 0;
    public GameObject roomActivation;

    public int enemySpawnCount = 5;

    public GameObject[] potentialEnemiesToSpawn;
    private float spawnCounter = 0f;
    private float timeBetweenSpawns = 1.0f;

    public GameObject leftBlocker;
    public GameObject rightBlocker;

    int numberOfEnemiesToSpawn;

    private bool roomActive = false;

    private List<GameObject> enemies = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
      

        if (playerSpawnPositions.Count < 1)
        {
            Debug.LogError("No spawn positions set in Room");
        }

        foreach (GameObject spawnPosition in playerSpawnPositions)
        {
            //"Hide placement graphic
            spawnPosition.SetActive(false);
        }
    }

    public void TriggerOverheadSound()
    {
        AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.roomTransition);
    }

    private void ActivateRoom()
    {
        DetermineSpawnValuesByPlayerCount();

        GameController.instance.TransitionRoom(this);
        AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.roomTransition);
        leftBlocker.SetActive(true);
        roomActive = true;
    }

        // Go to next room
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (GameController.instance.rooms.Count == 1)
            {
                ActivateRoom();
                return;
            }


            // disable the player when exiting the room
            other.gameObject.GetComponent<PlayerController>().DisableMovement();

            playerExitCount++;
            Debug.Log("Detected the player " + other.gameObject.GetComponent<PlayerController>().playerIndex);

            if (playerExitCount >= GameController.instance.playerCount)
            {
                Debug.Log("Leaving this town...");
                //int difficultyFactor = Mathf.RoundToInt((GameController.instance.playerCount - 1) * difficultyModifer);
                // Debug.Log("Difficulty factor "+difficultyFactor);

                ActivateRoom();
            
            }
        }
    }

    private void DetermineSpawnValuesByPlayerCount()
    {
        int playerCount = GameController.instance.playerCount;
        if (playerCount == 1)
            numberOfEnemiesToSpawn = enemySpawnCount;
        else if (playerCount == 2)
        {
            numberOfEnemiesToSpawn = enemySpawnCount;
        }
        else if (playerCount == 3)
        {
            numberOfEnemiesToSpawn = enemySpawnCount * 2;
        }
        else
        {
            numberOfEnemiesToSpawn = enemySpawnCount * 4;
        }
    }

    public void DisableActivationArea()
    {
        roomActivation.SetActive(false);
    }


    public void SpawnEnemies()
    {
        if (potentialEnemiesToSpawn.Length < 1)
        {
            //Debug.LogWarning("No potential enemies in this room");
            return;
        }

        if (enemySpawnPositions.Count < 1)
        {
            //Debug.LogWarning("No potential spawn locations for enemies in this room");
            return;
        }

        if (numberOfEnemiesToSpawn > 0)
        {
            if (spawnCounter <= 0)
            {
                int randomEnemyIndex = UnityEngine.Random.Range(0, potentialEnemiesToSpawn.Length);
                spawnCounter = timeBetweenSpawns;
                int randomSpawn = UnityEngine.Random.Range(0, enemySpawnPositions.Count);

                enemies.Add(Instantiate(potentialEnemiesToSpawn[randomEnemyIndex], enemySpawnPositions[randomSpawn].transform.position, Quaternion.identity));
                numberOfEnemiesToSpawn--;
            }
            else
            {
                spawnCounter -= Time.deltaTime;
            }
        }
    }


    void CleanDeadEnemies()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!roomActive)
            return;

        CleanDeadEnemies();

        // Check Open next room
        if (enemies.Count < 1 && numberOfEnemiesToSpawn < 1)
            rightBlocker.SetActive(false);

        SpawnEnemies();
    }
}
