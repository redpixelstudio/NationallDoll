using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Soil that can be watered or not
    /// </summary>

    [RequireComponent(typeof(UniqueID))]
    public class Soil : MonoBehaviour
    {
        public MeshRenderer mesh;
        public Material watered_mat;

        private UniqueID unique_id;
        private Material original_mat;
        private bool watered = false;
        private float update_timer = 0f;

        private static List<Soil> soil_list = new List<Soil>();

        void Awake()
        {
            soil_list.Add(this);
            unique_id = GetComponent<UniqueID>();
            if(mesh != null)
                original_mat = mesh.material;
        }

        private void OnDestroy()
        {
            soil_list.Remove(this);
        }

        private void Start()
        {
            
        }

        void Update()
        {
            bool now_watered = IsWatered();
            if (now_watered != watered && mesh != null && watered_mat != null)
            {
                mesh.material = now_watered ? watered_mat : original_mat;
            }
            watered = now_watered;

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            //Auto water
            if (!watered)
            {
                if (TheGame.Get().IsWeather(WeatherEffect.Rain))
                    Water();
                Sprinkler nearest = Sprinkler.GetNearestInRange(transform.position);
                if (nearest != null)
                    Water();
            }
        }

        //Water the soil
        public void Water()
        {
            PlayerData.Get().SetCustomInt(GetSubUID("water"), 1);
        }

        public void RemoveWater()
        {
            PlayerData.Get().SetCustomInt(GetSubUID("water"), 0);
        }

        public bool IsWatered()
        {
            return PlayerData.Get().GetCustomInt(GetSubUID("water")) > 0;
        }

        public string GetSubUID(string tag)
        {
            return unique_id.GetSubUID(tag);
        }

        public static Soil GetNearest(Vector3 pos, float range=999f)
        {
            float min_dist = range;
            Soil nearest = null;
            foreach (Soil soil in soil_list)
            {
                float dist = (pos - soil.transform.position).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = soil;
                }
            }
            return nearest;
        }

        public static List<Soil> GetAll(){
            return soil_list;
        }
    }

}
