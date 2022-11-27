using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using RhythmGameStarter;
using UnityEngine;

public class SongChooser : MonoBehaviour
{
    [SerializeField] public List<SongItem> SongItems;
    [SerializeField] public Canvas choosingCanvas;
    [SerializeField] private SongBlock block;

    [HideInInspector] public SongItem chosenSong;

    private void Start()
    {
        foreach (var song in SongItems)
        {
            var newBlock = Instantiate(block, choosingCanvas.transform.GetChild(0).transform);
            newBlock.Initialize(song, ChooseSong);
        }
        choosingCanvas.gameObject.SetActive(false);
    }

    private void ChooseSong(SongItem newSong)
    {
        chosenSong = newSong;
    }

    public async Task WaitForSong()
    {
        while (chosenSong == null)
        {
            await Task.Yield();
        }
        Debug.Log(chosenSong);
        choosingCanvas.gameObject.SetActive(false);
    }
}
