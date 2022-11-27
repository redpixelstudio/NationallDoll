using System;
using System.Collections;
using System.Collections.Generic;
using RhythmGameStarter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI songName;
    [SerializeField] private Button button;
    [HideInInspector] public SongItem chosenSong;

    private Action<SongItem> call;

    public void Initialize(SongItem newSongItem, Action<SongItem> callback)
    {
        chosenSong = newSongItem;
        songName.text = newSongItem.name;
        call = callback;
    }

    private void Start()
    {
        button.onClick.AddListener(() => call(chosenSong));
    }
}
