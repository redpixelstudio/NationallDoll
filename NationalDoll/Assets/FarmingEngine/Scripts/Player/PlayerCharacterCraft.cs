using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FarmingEngine
{
    /// <summary>
    /// Class that manages the player character crafting and building
    /// </summary>
    
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerCharacterCraft : MonoBehaviour
    {
        public GroupData[] craft_groups;
        public float craft_energy = 1f;
        public float build_energy = 2f;

        public UnityAction<CraftData> onCraft;
        public UnityAction<Buildable> onBuild;

        private PlayerCharacter character;

        private Buildable current_buildable = null;
        private CraftData current_build_data = null;
        private CraftData current_crafting = null;
        private GameObject craft_progress = null;
        private float build_timer = 0f;
        private float craft_timer = 0f;
        private bool clicked_build = false;

        private bool build_pay_cost = false;
        private UnityAction<Buildable> build_callback = null;

        private void Awake()
        {
            character = GetComponent<PlayerCharacter>();
        }

        void Start()
        {
            if (PlayerUI.Get(character.player_id))
            {
                PlayerUI.Get(character.player_id).onCancelSelection += CancelBuilding;
            }
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (character.IsDead())
                return;

             build_timer += Time.deltaTime;
             craft_timer += Time.deltaTime;

            PlayerControls controls = PlayerControls.Get(character.player_id);

            //Cancel building
            if (controls.IsPressUICancel() || controls.IsPressPause())
                CancelBuilding();

            //Cancel crafting
            if (current_crafting != null && character.IsMoving())
                CancelCrafting();

            //Complete crafting after timer
            if (current_crafting != null)
            {
                if (craft_timer > current_crafting.craft_duration)
                    CompleteCrafting();
            }
        }

        //---- Crafting ----

        public bool CanCraft(CraftData item, bool skip_near = false)
        {
            if (item == null)
                return false;

            CraftCostData cost = item.GetCraftCost();
            bool can_craft = true;

            Dictionary<GroupData, int> item_groups = new Dictionary<GroupData, int>(); //Add to groups so that fillers are not same than items

            foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
            {
                AddCraftCostItemsGroups(item_groups, pair.Key, pair.Value);
                if (!character.Inventory.HasItem(pair.Key, pair.Value))
                    can_craft = false; //Dont have required items
            }

            foreach (KeyValuePair<GroupData, int> pair in cost.craft_fillers)
            {
                int value = pair.Value + CountCraftCostGroup(item_groups, pair.Key);
                if (!character.Inventory.HasItemInGroup(pair.Key, value))
                    can_craft = false; //Dont have required items
            }

            foreach (KeyValuePair<CraftData, int> pair in cost.craft_requirements)
            {
                if (CountRequirements(pair.Key) < pair.Value)
                    can_craft = false; //Dont have required constructions
            }

            if (!skip_near && cost.craft_near != null && !character.IsNearGroup(cost.craft_near) && !character.EquipData.HasItemInGroup(cost.craft_near))
                can_craft = false; //Not near required construction

            if (character.Attributes.GetAttributeValue(AttributeType.Energy) < craft_energy)
                can_craft = false; //Not enough energy

            return can_craft;
        }

        private void AddCraftCostItemsGroups(Dictionary<GroupData, int> item_groups, ItemData item, int quantity)
        {
            foreach (GroupData group in item.groups) {
                if (item_groups.ContainsKey(group))
                    item_groups[group] += quantity;
                else
                    item_groups[group] = quantity;
            }
        }

        private int CountCraftCostGroup(Dictionary<GroupData, int> item_groups, GroupData group)
        {
            if (item_groups.ContainsKey(group))
                return item_groups[group];
            return 0;
        }

        public void PayCraftingCost(CraftData item, bool build=false)
        {
            CraftCostData cost = item.GetCraftCost();
            foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
            {
                character.Inventory.UseItem(pair.Key, pair.Value);
            }
            foreach (KeyValuePair<GroupData, int> pair in cost.craft_fillers)
            {
                character.Inventory.UseItemInGroup(pair.Key, pair.Value);
            }
            float cost_energy = build ? build_energy : craft_energy;
            character.Attributes.AddAttribute(AttributeType.Energy, -cost_energy);
        }

        public int CountRequirements(CraftData requirement)
        {
            if (requirement is ItemData)
                return character.Inventory.CountItem((ItemData)requirement);
            else
                return CraftData.CountSceneObjects(requirement);
        }

        //----- Craftin process -----

        public void StartCraftingOrBuilding(CraftData data)
        {
            if (CanCraft(data))
            {
                ConstructionData construct = data.GetConstruction();
                PlantData plant = data.GetPlant();

                if (construct != null)
                    CraftConstructionBuildMode(construct);
                else if (plant != null)
                    CraftPlantBuildMode(plant, 0);
                else
                    StartCrafting(data);

                TheAudio.Get().PlaySFX("craft", data.craft_sound);
            }
        }

        public void CraftPlantBuildMode(PlantData plant, int stage, bool pay_craft_cost = true, UnityAction<Buildable> callback = null)
        {
            if (!pay_craft_cost || CanCraft(plant))
            {
                CancelCrafting();

                Plant aplant = Plant.CreateBuildMode(plant, transform.position, stage);
                current_buildable = aplant.GetBuildable();
                current_buildable.StartBuild(character);
                current_build_data = plant;
                clicked_build = false;
                build_pay_cost = pay_craft_cost;
                build_callback = callback;
                build_timer = 0f;
            }
        }

        public void CraftConstructionBuildMode(ConstructionData item, bool pay_craft_cost = true, UnityAction<Buildable> callback = null)
        {
            if (!pay_craft_cost || CanCraft(item))
            {
                CancelCrafting();

                Construction construction = Construction.CreateBuildMode(item, transform.position + transform.forward * 1f);
                current_buildable = construction.GetBuildable();
                current_buildable.StartBuild(character);
                current_build_data = item;
                clicked_build = false;
                build_pay_cost = pay_craft_cost;
                build_callback = callback;
                build_timer = 0f;
            }
        }

        public void CraftCharacterBuildMode(CharacterData item, bool pay_craft_cost = true, UnityAction<Buildable> callback = null)
        {
            if (!pay_craft_cost || CanCraft(item))
            {
                CancelCrafting();

                Character acharacter = Character.CreateBuildMode(item, transform.position + transform.forward * 1f);
                current_buildable = acharacter.GetBuildable();
                if(current_buildable != null)
                    current_buildable.StartBuild(character);
                current_build_data = item;
                clicked_build = false;
                build_pay_cost = pay_craft_cost;
                build_callback = callback;
                build_timer = 0f;
            }
        }

        //Start crafting with timer
        public void StartCrafting(CraftData data)
        {
            if (data != null && current_crafting == null)
            {
                current_crafting = data;
                craft_timer = 0f;
                character.StopMove();

                if (AssetData.Get().action_progress != null && data.craft_duration > 0.1f)
                {
                    craft_progress = Instantiate(AssetData.Get().action_progress, transform);
                    craft_progress.GetComponent<ActionProgress>().duration = data.craft_duration;
                }

                if (data.craft_duration < 0.01f)
                    CompleteCrafting();
            }
        }

        //After reached build position, start crafting duration / anim
        public void StartCraftBuilding()
        {
            if (current_buildable != null)
            {
                StartCraftBuilding(current_buildable.transform.position); //Use current position
            }
        }

        public void StartCraftBuilding(Vector3 pos)
        {
            if (current_build_data != null && current_buildable != null && current_crafting == null)
            {
                if (!build_pay_cost || CanCraft(current_build_data, true)){

                    current_buildable.SetBuildPositionTemporary(pos); //Set to position to test the condition, before applying it
                    if (current_buildable.CheckIfCanBuild())
                    {
                        current_buildable.SetBuildPosition(pos);
                        StartCrafting(current_build_data);
                        character.FaceTorward(pos);
                    }
                }
            }
        }

        //End of the craft timer
        public void CompleteCrafting()
        {
            if (current_crafting != null)
            {
                if (current_buildable != null)
                    CompleteBuilding(current_buildable.transform.position);
                else
                    CraftCraftable(current_crafting);
                current_crafting = null;
            }
        }

        public void CancelCrafting()
        {
            current_crafting = null;
            if (craft_progress != null)
                Destroy(craft_progress);
            CancelBuilding();
        }

        public void CancelBuilding()
        {
            if (current_buildable != null)
            {
                Destroy(current_buildable.gameObject);
                current_buildable = null;
                current_build_data = null;
                build_callback = null;
                clicked_build = false;
            }
        }

        //Order to move to and build there
        public void TryBuildAt(Vector3 pos)
        {
            bool in_range = character.interact_type == PlayerInteractBehavior.MoveAndInteract || IsInBuildRange();
            if (!in_range)
                return;

            if (!clicked_build && current_buildable != null)
            {
                current_buildable.SetBuildPositionTemporary(pos); //Set build position before checkifcanbuild

                bool can_build = current_buildable.CheckIfCanBuild();
                if (can_build)
                {
                    current_buildable.SetBuildPosition(pos);
                    clicked_build = true; //Give command to build
                    character.MoveTo(pos);
                }
            }
        }

        //----- Crafting Completion -----

        //Craft immediately
        public void CraftCraftable(CraftData data)
        {
            ItemData item = data.GetItem();
            ConstructionData construct = data.GetConstruction();
            PlantData plant = data.GetPlant();
            CharacterData character = data.GetCharacter();

            if (item != null)
                CraftItem(item);
            else if (construct != null)
                CraftConstruction(construct);
            else if (plant != null)
                CraftPlant(plant, 0);
            else if (character != null)
                CraftCharacter(character);
        }

        public void CraftItem(ItemData item, bool pay_craft_cost = true)
        {
            if (!pay_craft_cost || CanCraft(item))
            {
                if (pay_craft_cost)
                    PayCraftingCost(item);

                if (character.Inventory.CanTakeItem(item, item.craft_quantity))
                    character.Inventory.GainItem(item, item.craft_quantity);
                else
                    Item.Create(item, transform.position, item.craft_quantity);

                character.SaveData.AddCraftCount(item.id);
                character.Attributes.GainXP(item.craft_xp_type, item.craft_xp);

                if (onCraft != null)
                    onCraft.Invoke(item);
            }
        }

        public Character CraftCharacter(CharacterData character, bool pay_craft_cost = true)
        {
            if (!pay_craft_cost || CanCraft(character))
            {
                if (pay_craft_cost)
                    PayCraftingCost(character);

                Vector3 pos = transform.position + transform.forward * 0.8f;
                Character acharacter = Character.Create(character, pos);

                this.character.SaveData.AddCraftCount(character.id);
                this.character.Attributes.GainXP(character.craft_xp_type, character.craft_xp);

                if (onCraft != null)
                    onCraft.Invoke(character);

                return acharacter;
            }
            return null;
        }

        public Plant CraftPlant(PlantData plant, int stage, bool pay_craft_cost = true)
        {
            if (!pay_craft_cost || CanCraft(plant))
            {
                if (pay_craft_cost)
                    PayCraftingCost(plant);

                Vector3 pos = transform.position + transform.forward * 0.4f;
                Plant aplant = Plant.Create(plant, pos, stage);

                character.SaveData.AddCraftCount(plant.id);
                character.Attributes.GainXP(plant.craft_xp_type, plant.craft_xp);

                if (onCraft != null)
                    onCraft.Invoke(plant);

                return aplant;
            }
            return null;
        }

        public Construction CraftConstruction(ConstructionData construct, bool pay_craft_cost = true)
        {
            if (!pay_craft_cost || CanCraft(construct))
            {
                if (pay_craft_cost)
                    PayCraftingCost(construct);

                Vector3 pos = transform.position + transform.forward * 1f;
                Construction aconstruct = Construction.Create(construct, pos);

                character.SaveData.AddCraftCount(construct.id);
                character.Attributes.GainXP(construct.craft_xp_type, construct.craft_xp);

                if (onCraft != null)
                    onCraft.Invoke(construct);

                return aconstruct;
            }
            return null;
        }

        public void CompleteBuilding()
        {
            if (current_buildable != null)
            {
                CompleteBuilding(current_buildable.transform.position);
            }
        }

        public void CompleteBuilding(Vector3 pos)
        {
            CraftData item = current_crafting;
            if (item != null && current_buildable != null && (!build_pay_cost || CanCraft(item, true)))
            {
                current_buildable.SetBuildPositionTemporary(pos); //Set to position to test the condition, before applying it

                if (current_buildable.CheckIfCanBuild())
                {
                    current_buildable.SetBuildPosition(pos);

                    character.FaceTorward(pos);

                    if (build_pay_cost)
                        PayCraftingCost(item, true);

                    Buildable buildable = current_buildable;
                    buildable.FinishBuild();

                    character.SaveData.AddCraftCount(item.id);
                    character.Attributes.GainXP(item.craft_xp_type, item.craft_xp);

                    UnityAction<Buildable> bcallback = build_callback;
                    current_buildable = null;
                    current_build_data = null;
                    build_callback = null;
                    clicked_build = false;
                    character.StopAutoMove();

                    PlayerUI.Get(character.player_id)?.CancelSelection();
                    TheAudio.Get().PlaySFX("craft", buildable.build_audio);

                    if (onBuild != null)
                        onBuild.Invoke(buildable);

                    if (bcallback != null)
                        bcallback.Invoke(buildable);

                    character.TriggerBusy(1f);
                }
            }
        }

        //---- Values and getters

        public void LearnCraft(string craft_id)
        {
            character.SaveData.UnlockID(craft_id);
        }
		
		public bool HasLearnt(string craft_id)
        {
            return character.SaveData.IsIDUnlocked(craft_id);
        }

        public int CountTotalCrafted(CraftData craftable)
        {
            if(craftable != null)
                return character.SaveData.GetCraftCount(craftable.id);
            return 0;
        }

        public void ResetCraftCount(CraftData craftable)
        {
            if (craftable != null)
                character.SaveData.ResetCraftCount(craftable.id);
        }

        public void ResetCraftCount()
        {
            character.SaveData.ResetCraftCount();
        }

        //Did it click to order to build
        public bool ClickedBuild()
        {
            return clicked_build;
        }

        public bool CanBuild()
        {
            return current_buildable != null && current_buildable.IsBuilding() && build_timer > 0.5f;
        }

        public bool IsInBuildRange()
        {
            if (current_buildable == null)
                return false;
            Vector3 dist = (character.GetInteractCenter() - current_buildable.transform.position);
            return dist.magnitude < current_buildable.GetBuildRange(character);
        }

        public bool IsBuildMode()
        {
            return current_buildable != null && current_buildable.IsBuilding();
        }

        public bool IsCrafting()
        {
            return current_crafting != null;
        }

        public float GetCraftProgress()
        {
            if (current_crafting != null && current_crafting.craft_duration > 0.01f)
                return craft_timer / current_crafting.craft_duration;
            return 0f;
        }

        public Buildable GetCurrentBuildable()
        {
            return current_buildable; //Can be null if not in build mode
        }

        public CraftData GetCurrentCrafting()
        {
            return current_crafting;
        }

        public PlayerCharacter GetCharacter()
        {
            return character;
        }

        public CraftStation GetCraftStation()
        {
            CraftStation station = CraftStation.GetNearestInRange(transform.position);
            return station;
        }

        public List<GroupData> GetCraftGroups()
        {
            CraftStation station = CraftStation.GetNearestInRange(transform.position);
            if (station != null)
                return new List<GroupData>(station.craft_groups);
            else
                return new List<GroupData>(craft_groups);
        }
    }
}
