using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MusicManager : MonoBehaviour
{

    public GameObject[] musics;

    public void PlaySong(int index)
    {

        for (int i = 0; i < musics.Length; i++)
        {
            musics[i].SetActive(false);
        }

        gameObject.SetActive(true);
    }
}
