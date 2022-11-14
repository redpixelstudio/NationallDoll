using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FarmingEngine
{

    /// <summary>
    /// Add to your player character for HOE feature
    /// </summary>

    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerCharacterHoe : MonoBehaviour
    {
        public GroupData hoe_item;
        public ConstructionData hoe_soil;
        public float hoe_range = 1f;
        public float hoe_build_radius = 0.5f;
        public int hoe_energy = 1;

        private PlayerCharacter character;
        private 

        void Awake()
        {
            character = GetComponent<PlayerCharacter>();
        }

        private void OnDestroy()
        {
            
        }

        private void Start()
        {
            
        }

        void FixedUpdate()
        {
            
        }

        private void Update()
        {
            //Auto hoe
            if (character.IsAutoMove())
            {
                HoeGroundAuto(character.GetAutoMoveTarget());
            }
        }

        public void HoeGround(Vector3 pos)
        {
            if (!CanHoe())
                return;

            character.StopMove();
            character.Attributes.AddAttribute(AttributeType.Energy, -hoe_energy);

            character.TriggerAnim(character.Animation ? character.Animation.hoe_anim : "", pos);
            character.TriggerProgressBusy(0.8f, () =>
            {
                Construction prev = Construction.GetNearest(pos, hoe_build_radius);
                Plant plant = Plant.GetNearest(pos, hoe_build_radius);
                if (prev != null && plant == null && prev.data == hoe_soil)
                {
                    prev.Destroy(); //Destroy previous, if no plant on it
                    return;
                }

                Construction construct = Construction.CreateBuildMode(hoe_soil, pos);
                construct.GetBuildable().StartBuild(character);
                construct.GetBuildable().SetBuildPositionTemporary(pos);
                if (construct.GetBuildable().CheckIfCanBuild())
                {
                    construct.GetBuildable().FinishBuild();
                }
                else
                {
                    Destroy(construct.gameObject);
                }
            });

        }

        public bool CanHoe()
        {
            bool has_energy = character.Attributes.GetAttributeValue(AttributeType.Energy) >= hoe_energy;
            InventoryItemData ivdata = character.EquipData.GetEquippedItem(EquipSlot.Hand);
            ItemData idata = ItemData.Get(ivdata?.item_id);
            return has_energy && idata != null && idata.HasGroup(hoe_item);
        }

        public void HoeGroundAuto(Vector3 pos)
        {
            Vector3 dir = pos - transform.position;
            if (character.IsBusy() || character.Crafting.ClickedBuild() || dir.magnitude > hoe_range
                || character.GetAutoSelectTarget() != null || character.GetAutoDropInventory() != null)
                return;

            InventoryItemData ivdata = character.EquipData.GetEquippedItem(EquipSlot.Hand);
            if (ivdata != null && CanHoe())
            {
                HoeGround(pos);

                if (ivdata != null)
                    ivdata.durability -= 1;
            }
        }
    }

}