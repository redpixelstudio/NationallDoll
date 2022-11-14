using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Plants can be sowed (from a seed) and their fruit can be harvested. They can also have multiple growth stages.
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Buildable))]
    [RequireComponent(typeof(UniqueID))]
    [RequireComponent(typeof(Destructible))]
    public class Plant : Craftable
    {
        [Header("Plant")]
        public PlantData data;
        public int growth_stage = 0;

        [Header("Time")]
        public TimeType time_type = TimeType.GameDays; //Time type (days or hours) for grow_time and fruit_grow_time
        
        [Header("Growth")]
        public float grow_time = 8f; //Game-hours or game-days
        public bool grow_require_water = true; //Require water to grow ?
        public bool regrow_on_death; //If true, will go back to stage 1 instead of being destroyed
        public float soil_range = 1f; //How far the watered soil can be from the plant

        [Header("Harvest")]
        public ItemData fruit;
        public float fruit_grow_time = 0f; //In game hours or game-days
        public bool fruit_require_water = true; //Require water to grow fruit ?
        public Transform fruit_model;
        public bool death_on_harvest;

        [Header("FX")]
        public GameObject gather_fx;
        public AudioClip gather_audio;

        [HideInInspector]
        public bool was_spawned = false; //If true, means it was crafted or loaded from save file

        private Selectable selectable;
        private Buildable buildable;
        private Destructible destruct;
        private UniqueID unique_id;
        private Soil soil;

        private int nb_stages = 1;
        private bool has_fruit = false;
        private float update_timer = 0f;

        private static List<Plant> plant_list = new List<Plant>();

        protected override void Awake()
        {
            base.Awake();
            plant_list.Add(this);
            selectable = GetComponent<Selectable>();
            buildable = GetComponent<Buildable>();
            destruct = GetComponent<Destructible>();
            unique_id = GetComponent<UniqueID>();
            selectable.onDestroy += OnDeath;
            buildable.onBuild += OnBuild;

            if(data != null)
                nb_stages = Mathf.Max(data.growth_stage_prefabs.Length, 1);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            plant_list.Remove(this);
        }

        void Start()
        {
            if (!was_spawned && PlayerData.Get().IsObjectRemoved(GetUID()))
            {
                Destroy(gameObject);
                return;
            }

            //Soil
            if (!buildable.IsBuilding())
                soil = Soil.GetNearest(transform.position, soil_range);

            //Fruit
            if (PlayerData.Get().HasCustomInt(GetSubUID("fruit")))
                has_fruit = PlayerData.Get().GetCustomInt(GetSubUID("fruit")) > 0;

            //Grow time
            if (!PlayerData.Get().HasCustomFloat(GetSubUID("grow_time")))
                ResetGrowTime();

            RefreshFruitModel();
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (buildable.IsBuilding())
                return;

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            if (!IsFullyGrown() && HasUID())
            {
                bool can_grow = !grow_require_water || HasWater();
                if (can_grow && GrowTimeFinished())
                {
                    GrowPlant();
                    return;
                }
            }

            if (!has_fruit && fruit != null && HasUID())
            {
                bool can_grow = !fruit_require_water || HasWater();
                if (can_grow && FruitGrowTimeFinished())
                {
                    GrowFruit();
                    return;
                }
            }

            //Auto water
            if (!HasWater())
            {
                if (TheGame.Get().IsWeather(WeatherEffect.Rain))
                    Water();
                Sprinkler nearest = Sprinkler.GetNearestInRange(transform.position);
                if (nearest != null)
                    Water();
            }
        }

        public void GrowPlant()
        {
            if (!IsFullyGrown())
            {
                GrowPlant(growth_stage + 1);
            }
        }

        public void GrowPlant(int grow_stage)
        {
            if (data != null && growth_stage >= 0 && growth_stage < nb_stages)
            {
                SowedPlantData sdata = PlayerData.Get().GetSowedPlant(GetUID());
                if (sdata == null)
                {
                    //Remove this plant and create a new one (this one probably was already in the scene)
                    if (!was_spawned)
                        PlayerData.Get().RemoveObject(GetUID()); //Remove Unique id
                    sdata = PlayerData.Get().AddPlant(data.id, SceneNav.GetCurrentScene(), transform.position, transform.rotation, grow_stage);
                }
                else
                {
                    //Grow current plant from data
                    PlayerData.Get().GrowPlant(GetUID(), grow_stage);
                }

                ResetGrowTime();
                RemoveWater();
                plant_list.Remove(this); //Remove from list so spawn works!

                Spawn(sdata.uid);
                Destroy(gameObject);
            }
        }

        public void GrowFruit()
        {
            if (fruit != null && !has_fruit)
            {
                has_fruit = true;
                PlayerData.Get().SetCustomInt(GetSubUID("fruit"), 1);
                RemoveWater();
                RefreshFruitModel();
            }
        }

        public void Harvest(PlayerCharacter character)
        {
            if (fruit != null && has_fruit && character.Inventory.CanTakeItem(fruit, 1))
            {
                GameObject source = fruit_model != null ? fruit_model.gameObject : gameObject;
                character.Inventory.GainItem(fruit, 1, source.transform.position);

                RemoveFruit();

                if (death_on_harvest && destruct != null)
                    destruct.Kill();

                TheAudio.Get().PlaySFX("plant", gather_audio);

                if (gather_fx != null)
                    Instantiate(gather_fx, transform.position, Quaternion.identity);
            }
        }

        public void RemoveFruit()
        {
            if (has_fruit)
            {
                has_fruit = false;
                ResetGrowTime();
                PlayerData.Get().SetCustomInt(GetSubUID("fruit"), 0);
                RefreshFruitModel();
            }
        }

        public void Water()
        {
            if (!HasWater())
            {
                if (soil != null)
                    soil.Water();
                PlayerData.Get().SetCustomInt(GetSubUID("water"), 1);
                ResetGrowTime();
            }
        }

        public void RemoveWater()
        {
            if (HasWater())
            {
                PlayerData.Get().SetCustomInt(GetSubUID("water"), 0);
                if (soil != null)
                    soil.RemoveWater();
            }
        }

        private void RefreshFruitModel()
        {
            if (fruit_model != null && has_fruit != fruit_model.gameObject.activeSelf)
                fruit_model.gameObject.SetActive(has_fruit);
        }

        private void ResetGrowTime()
        {
            if(time_type == TimeType.GameDays)
                PlayerData.Get().SetCustomFloat(GetSubUID("grow_time"), PlayerData.Get().day);
            if(time_type == TimeType.GameHours)
                PlayerData.Get().SetCustomFloat(GetSubUID("grow_time"), PlayerData.Get().GetTotalTime());
        }

        private bool GrowTimeFinished()
        {
            float last_grow_time = PlayerData.Get().GetCustomFloat(GetSubUID("grow_time"));
            if (time_type == TimeType.GameDays && HasUID())
                return PlayerData.Get().day >= Mathf.RoundToInt(last_grow_time + grow_time);
            if (time_type == TimeType.GameHours && HasUID())
                return PlayerData.Get().GetTotalTime() > last_grow_time + grow_time;
            return false;
        }

        private bool FruitGrowTimeFinished()
        {
            float last_grow_time = PlayerData.Get().GetCustomFloat(GetSubUID("grow_time"));
            if (time_type == TimeType.GameDays && HasUID())
                return PlayerData.Get().day >= Mathf.RoundToInt(last_grow_time + fruit_grow_time);
            if (time_type == TimeType.GameHours && HasUID())
                return PlayerData.Get().GetTotalTime() > last_grow_time + fruit_grow_time;
            return false;
        }

        public void Kill()
        {
            destruct.Kill();
        }

        public void KillNoLoot()
        {
            destruct.KillNoLoot(); //Such as when being eaten, dont spawn loot
        }

        private void OnBuild()
        {
            if (data != null)
            {
                SowedPlantData splant = PlayerData.Get().AddPlant(data.id, SceneNav.GetCurrentScene(), transform.position, transform.rotation, growth_stage);
                unique_id.unique_id = splant.uid;
                soil = Soil.GetNearest(transform.position, soil_range);
                ResetGrowTime();
            }
        }

        private void OnDeath()
        {
            if (data != null)
            {
                foreach (PlayerCharacter character in PlayerCharacter.GetAll())
                    character.SaveData.AddKillCount(data.id); //Add kill count
            }

            PlayerData.Get().RemovePlant(GetUID());
            if (!was_spawned)
                PlayerData.Get().RemoveObject(GetUID());

            if (HasFruit())
                Item.Create(fruit, transform.position, 1);

            if (data != null && regrow_on_death)
            {
                SowedPlantData sdata = PlayerData.Get().GetSowedPlant(GetUID());
                Create(data, transform.position, transform.rotation, 0);
            }
        }

        public bool HasFruit()
        {
            return has_fruit;
        }

        public bool HasWater()
        {
            bool wplant = PlayerData.Get().GetCustomInt(GetSubUID("water")) > 0;
            bool wsoil = soil != null ? soil.IsWatered() : false;
            return wplant || wsoil;
        }

        public bool IsFullyGrown()
        {
            return (growth_stage + 1) >= nb_stages;
        }

        public bool IsBuilt()
        {
            return !IsDead() && !buildable.IsBuilding();
        }

        public bool IsDead()
        {
            return destruct.IsDead();
        }

        public float GetGrowTime()
        {
            return PlayerData.Get().GetCustomFloat(GetSubUID("grow_time"));
        }

        public bool HasUID()
        {
            return !string.IsNullOrEmpty(unique_id.unique_id);
        }

        public string GetUID()
        {
            return unique_id.unique_id;
        }

        public string GetSubUID(string tag)
        {
            return unique_id.GetSubUID(tag);
        }

        public bool HasGroup(GroupData group)
        {
            if (data != null)
                return data.HasGroup(group) || selectable.HasGroup(group);
            return selectable.HasGroup(group);
        }

        public Selectable GetSelectable()
        {
            return selectable;
        }

        public Destructible GetDestructible()
        {
            return destruct;
        }

        public Buildable GetBuildable()
        {
            return buildable;
        }

        public SowedPlantData SaveData
        {
            get { return PlayerData.Get().GetSowedPlant(GetUID()); }  //Can be null if not sowed or spawned
        }

        public static new Plant GetNearest(Vector3 pos, float range = 999f)
        {
            Plant nearest = null;
            float min_dist = range;
            foreach (Plant plant in plant_list)
            {
                float dist = (plant.transform.position - pos).magnitude;
                if (dist < min_dist && plant.IsBuilt())
                {
                    min_dist = dist;
                    nearest = plant;
                }
            }
            return nearest;
        }

        public static int CountInRange(Vector3 pos, float range)
        {
            int count = 0;
            foreach (Plant plant in GetAll())
            {
                float dist = (plant.transform.position - pos).magnitude;
                if (dist < range && plant.IsBuilt())
                    count++;
            }
            return count;
        }

        public static int CountInRange(PlantData data, Vector3 pos, float range)
        {
            int count = 0;
            foreach (Plant plant in GetAll())
            {
                if (plant.data == data && plant.IsBuilt())
                {
                    float dist = (plant.transform.position - pos).magnitude;
                    if (dist < range)
                        count++;
                }
            }
            return count;
        }

        public static Plant GetByUID(string uid)
        {
            if (!string.IsNullOrEmpty(uid))
            {
                foreach (Plant plant in plant_list)
                {
                    if (plant.GetUID() == uid)
                        return plant;
                }
            }
            return null;
        }

        public static List<Plant> GetAllOf(PlantData data)
        {
            List<Plant> valid_list = new List<Plant>();
            foreach (Plant plant in plant_list)
            {
                if (plant.data == data)
                    valid_list.Add(plant);
            }
            return valid_list;
        }

        public static new List<Plant> GetAll()
        {
            return plant_list;
        }

        //Spawn an existing one in the save file (such as after loading)
        public static Plant Spawn(string uid, Transform parent = null)
        {
            SowedPlantData sdata = PlayerData.Get().GetSowedPlant(uid);
            if (sdata != null && sdata.scene == SceneNav.GetCurrentScene())
            {
                PlantData pdata = PlantData.Get(sdata.plant_id);
                if (pdata != null)
                {
                    GameObject prefab = pdata.GetStagePrefab(sdata.growth_stage);
                    GameObject build = Instantiate(prefab, sdata.pos, sdata.rot);
                    build.transform.parent = parent;

                    Plant plant = build.GetComponent<Plant>();
                    plant.data = pdata;
                    plant.growth_stage = sdata.growth_stage;
                    plant.was_spawned = true;
                    plant.unique_id.unique_id = uid;
                    return plant;
                }
            }
            return null;
        }

        //Create a totally new one, in build mode for player to place, will be saved after FinishBuild() is called, -1 = max stage
        public static Plant CreateBuildMode(PlantData data, Vector3 pos, int stage)
        {
            GameObject prefab = data.GetStagePrefab(stage);
            GameObject build = Instantiate(prefab, pos, prefab.transform.rotation);
            Plant plant = build.GetComponent<Plant>();
            plant.data = data;
            plant.was_spawned = true;

            if(stage >= 0 && stage < data.growth_stage_prefabs.Length)
                plant.growth_stage = stage;
            
            return plant;
        }

        //Create a totally new one that will be added to save file, already placed
        public static Plant Create(PlantData data, Vector3 pos, int stage)
        {
            Plant plant = CreateBuildMode(data, pos, stage);
            plant.buildable.FinishBuild();
            return plant;
        }

        public static Plant Create(PlantData data, Vector3 pos, Quaternion rot, int stage)
        {
            Plant plant = CreateBuildMode(data, pos, stage);
            plant.transform.rotation = rot;
            plant.buildable.FinishBuild();
            return plant;
        }
    }

}