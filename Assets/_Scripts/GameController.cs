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

    public GameObject bigExplosion;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

       // if (musicActive)
       //     AudioPlayer.PlayMusic(AudioPlayer.instance.fmodAudio.musicTune01);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKey(KeyCode.P))
        {
            GameOver();
        }
    }

    public void CreatePlayers(int selectPlayerCount)
    {
        UIManager.instance.HidePlayerSelection();
        playerCount = selectPlayerCount;

        // Create and position players
        for (int i = 0; i < selectPlayerCount; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            players.Add(player.GetComponent<PlayerController>());
            PositionPlayer(i);
        }

        
        if (selectPlayerCount == 1)
        {
            GameObject player = Instantiate(playerPrefab);
            player.GetComponent<PlayerController>().isAI = true;
            players.Add(player.GetComponent<PlayerController>());
            PositionPlayer(1);
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
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void GameOver()
    {
        if (gameover)
        {
            return;
        }

        Instantiate(bigExplosion, rooms[currentActiveRoom].transform.position, Quaternion.identity);


        foreach (PlayerController player in players)
        {
            player.StopSounds();
            Destroy(player.gameObject);
        }

        playerCount = 0;

        players.Clear();

        gameover = true;
        StartCoroutine("WaitToGameover");

    }

    
}
