using System.Collections;
using UnityEngine;

namespace FarmingEngine.EditorTool
{
    /// <summary>
    /// Default Settings file for the CreatObject editor script
    /// </summary>
    
    [CreateAssetMenu(fileName = "CreateObjectSettings", menuName = "FarmingEngine/CreateObjectSettings", order = 100)]
    public class CreateObjectSettings : ScriptableObject
    {

        [Header("Save Folders")]
        public string prefab_folder = "FarmingEngine/Prefabs";
        public string prefab_equip_folder = "FarmingEngine/Prefabs/Equip";
        public string items_folder = "FarmingEngine/Resources/Items";
        public string constructions_folder = "FarmingEngine/Resources/Constructions";
        public string plants_folder = "FarmingEngine/Resources/Plants";
        public string characters_folder = "FarmingEngine/Resources/Characters";

        [Header("Default Values")]
        public Material outline;
        public GameObject death_fx;
        public AudioClip craft_audio;
        public GameObject take_fx;
        public AudioClip take_audio;
        public AudioClip attack_audio;
        public AudioClip build_audio;
        public GameObject build_fx;
        public SAction[] item_actions;
        public SAction equip_action;
        public SAction eat_action;

    }

}