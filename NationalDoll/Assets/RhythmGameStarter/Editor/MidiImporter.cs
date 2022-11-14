using UnityEditor;
using UnityEngine;
using System.IO;
using Melanchall.DryWetMidi.Core;
using System.Text.RegularExpressions;

namespace RhythmGameStarter
{
    public class MidiImporter : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (asset.EndsWith(".mid"))
                {
                    var detectedBpm = -1;
                    var bpmString = "";
                    foreach (var item in Path.GetFileNameWithoutExtension(asset).Split(' '))
                    {
                        if (item.ToLower().Contains("bpm"))
                        {
                            // Debug.Log(item);
                            // Debug.Log(Regex.Match(item, @"\d+").Value);
                            detectedBpm = int.Parse(Regex.Match(item, @"\d+").Value);
                            bpmString = item;
                            break;
                        }
                    }
                    string songName;
                    if (!string.IsNullOrEmpty(bpmString))
                    {
                        songName = Path.GetFileNameWithoutExtension(asset).Replace(bpmString, "").Trim();
                    }
                    else
                    {
                        songName = Path.GetFileNameWithoutExtension(asset);
                    }
                    string filePath = asset.Substring(0, asset.Length - Path.GetFileName(asset).Length);
                    string fileWithoutExt = filePath + songName;
                    string newFileName = fileWithoutExt + ".asset";

                    var rawMidi = MidiFile.Read(asset);

                    SongItem songItem = (SongItem)AssetDatabase.LoadAssetAtPath(newFileName, typeof(SongItem));

                    AudioClip clipFile;
                    clipFile = (AudioClip)AssetDatabase.LoadAssetAtPath(fileWithoutExt + ".mp3", typeof(AudioClip));
                    if (!clipFile)
                        clipFile = (AudioClip)AssetDatabase.LoadAssetAtPath(fileWithoutExt + ".wav", typeof(AudioClip));
                    else if (!clipFile)
                        clipFile = (AudioClip)AssetDatabase.LoadAssetAtPath(fileWithoutExt + ".ogg", typeof(AudioClip));

                    var isNew = false;
                    if (!songItem)
                    {
                        songItem = ScriptableObject.CreateInstance<SongItem>();
                        isNew = true;
                    }

                    if (songItem.bpm == 0 && detectedBpm != -1)
                        songItem.bpm = detectedBpm;

                    songItem.clip = clipFile;

                    SongItemEditor.UpdateBpm(rawMidi, songItem);

                    if (isNew)
                    {
                        Debug.Log("SongItem created for " + songName);
                        AssetDatabase.CreateAsset(songItem, newFileName);
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }


    }
}