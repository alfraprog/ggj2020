using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject playerPrefab;

    public List<RoomController> rooms = new List<RoomController>();

    private List<PlayerController> players = new List<PlayerController>();

    private int currentActiveRoom = 0;
    public int playerCount;
    private int currentSpawnedPlayerCount = 0;

    public bool musicActive = false;
    private bool gameover;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (rooms.Count < 1)
        {
            Debug.LogError("No rooms in game controller");
            
        }
       // if (musicActive)
       //     AudioPlayer.PlayMusic(AudioPlayer.instance.fmodAudio.musicTune01);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreatePlayers(int selectPlayerCount)
    {
        Debug.Log("Creating " + selectPlayerCount + "players");
        UIManager.instance.HidePlayerSelection();
        playerCount = selectPlayerCount;

        // Create and position players
        for (int i = 0; i < selectPlayerCount; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            players.Add(player.GetComponent<PlayerController>());
            PositionPlayer(i);
        }
       
    }

    public void PositionPlayer(int playerIndex)
    {
        if (rooms.Count < 1)
        {
            Debug.LogWarning("Can't position player. Don't worry if you are just testing. This will be taken care by the RoomGenerator");
            return;
        }

        players[playerIndex].transform.position = rooms[currentActiveRoom].playerSpawnPositions[playerIndex].transform.position;
    }

    public void TransitionRoom(RoomController roomController)
    {
        currentActiveRoom++;
        if (currentActiveRoom >= rooms.Count)
        {
            currentActiveRoom = rooms.Count - 1;
        }
        CameraController.instance.target = rooms[currentActiveRoom].transform;

        rooms[currentActiveRoom].DisableActivationArea();

        for (int i = 0; i < players.Count; i++)
        {
            // re - enable the player controller
            players[i].EnableMovement();
            // Position to new spawn locations
            PositionPlayer(i);
        }
        
    }

    public int GetSpawnedPlayerCount()
    {
        return currentSpawnedPlayerCount++;
    }

    IEnumerator WaitToGameover()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void GameOver()
    {
        if (gameover)
        {
            return;
        }


        gameover = true;
        StartCoroutine("WaitToGameover");

    }
}
