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

    int numberOfEnemiesToSpawn;


    // Start is called before the first frame update
    void Start()
    {
        numberOfEnemiesToSpawn = enemySpawnCount;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            // disable the player when exiting the room
            other.gameObject.GetComponent<PlayerController>().DisableMovement();

            playerExitCount++;
            Debug.Log("Detected the player "+ other.gameObject.GetComponent<PlayerController>().playerIndex);

            if (playerExitCount >= GameController.instance.playerCount)
            {
                Debug.Log("Leaving this town...");
                GameController.instance.TransitionRoom(this);
                AudioPlayer.PlaySFX(AudioPlayer.instance.fmodAudio.roomTransition);
            }
        }
    }

    public void DisableActivationArea()
    {
        roomActivation.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (potentialEnemiesToSpawn.Length < 1)
        {
            Debug.LogWarning("No potential enemies in this room");
            return;
        }

        if (enemySpawnPositions.Count < 1 )
        {
            Debug.LogWarning("No potential spawn locations for enemies in this room");
            return;
        }


        if (numberOfEnemiesToSpawn > 0)
        { 
            if (spawnCounter <= 0)
            {
                int randomEnemyIndex = Random.Range(0, potentialEnemiesToSpawn.Length);
                spawnCounter = timeBetweenSpawns;
                int randomSpawn = Random.Range(0, enemySpawnPositions.Count);

                Instantiate(potentialEnemiesToSpawn[randomEnemyIndex], enemySpawnPositions[randomSpawn].transform.position, Quaternion.identity);
                numberOfEnemiesToSpawn--;
            } else
            {
                spawnCounter -= Time.deltaTime;
            }
        }
    }
}
