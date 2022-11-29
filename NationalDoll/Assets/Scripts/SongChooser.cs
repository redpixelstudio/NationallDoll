using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using RhythmGameStarter;
using UnityEngine;
using UnityEngine.UI;

public class SongChooser : MonoBehaviour
{
    [SerializeField] public List<SongItem> SongItems;
    [SerializeField] public RectTransform choosingCanvas;
    [SerializeField] private SongBlock block;
    
    [Header("Pause")]
    [SerializeField] private Sprite pauseOn;
    [SerializeField] private Sprite pauseOff;
    [SerializeField] public Button pauseButton;

    [HideInInspector] public SongItem chosenSong;

    private bool isRhythmPaused;

    private void Start()
    {
        pauseButton.onClick.AddListener(PauseResumeRhythm);
        pauseButton.gameObject.SetActive(false);
        foreach (var song in SongItems)
        {
            var newBlock = Instantiate(block, choosingCanvas.transform);
            newBlock.Initialize(song, ChooseSong);
        }
        choosingCanvas.gameObject.SetActive(false);
    }

    private void PauseResumeRhythm()
    {
        isRhythmPaused = !isRhythmPaused;
        RhythmController.I.isSongPaused = isRhythmPaused;
        pauseButton.image.sprite = isRhythmPaused ? pauseOff : pauseOn;
        if (isRhythmPaused)
        {
            RhythmController.I.playerObject.character_anim.animator.speed = 0;
            RhythmController.I.songManager.PauseSong();
        }
        else
        {
            RhythmController.I.songManager.ResumeSong();
            RhythmController.I.playerObject.character_anim.animator.speed = 1;
        }
    }

    private void ChooseSong(SongItem newSong)
    {
        pauseButton.gameObject.SetActive(true);
        chosenSong = newSong;
        isRhythmPaused = false;
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
