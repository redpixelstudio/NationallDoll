using UnityEngine;
using UnityEngine.Events;

namespace FarmingEngine
{
    /// <summary>
    /// Animal that can eat and produce products
    /// </summary>

    public enum LivestockState
    {
        Wander=0,
        MoveTo=10,
        FindFood=20,
        Eat=25,
        Dead=50,
    }

    public enum LivestockProduceType
    {
        DropFloor=0,
        CollectAction=10
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Destructible))]
    [RequireComponent(typeof(Character))]
    public class AnimalLivestock : MonoBehaviour
    {
        [Header("Move/Wander")]
        public WanderBehavior wander = WanderBehavior.WanderNear;
        public float wander_range = 10f;
        public float wander_interval = 10f;
        public float detect_range = 5f;

        [Header("Time")]
        public TimeType time_type = TimeType.GameHours; //Time type (days or hours) for the produce/growth time

        [Header("Eat")]
        public bool eat = true;
        public GroupData eat_food_group; //Group of food the animal can eaet
        public float eat_range = 1f; //Distance it can eat from
        public float eat_interval_time = 12f; //In game-hours (or game-days), how long before eating again

        [Header("Produce")]
        public ItemData item_produce;
        public LivestockProduceType item_collect_type; //Is the item dropped on floor or need to be collected manually
        public int item_eat_count = 1; //How many time must eat to produce
        public float item_produce_time = 24f; //In game-hours (or game-days)
        public int item_max = 1; //Maximum number of produced items on floor at a time
        public float item_max_range = 10f; //How far are items counted as in maximum

        [Header("Growth")]
        public CharacterData grow_to;
        public int grow_eat_count = 4; //How many time must eat to grow
        public float grow_time = 48f; //In game-hours (or game-days)

        public UnityAction onAttack;
        public UnityAction onDamaged;
        public UnityAction onDeath;

        private LivestockState state;
        private Character character;
        private Selectable selectable;
        private Destructible destruct;
        private Animator animator;

        private Vector3 start_pos;

        private AnimalFood target_food;
        private Vector3 wander_target;

        private bool is_running = false;
        private bool is_active = false;

        private float state_timer = 0f;
        private float find_timer = 0f;
        private float update_timer = 0f;

        private float last_eat_time = 0f;
        private float last_grow_time = 0f;

        void Awake()
        {
            character = GetComponent<Character>();
            destruct = GetComponent<Destructible>();
            selectable = GetComponent<Selectable>();
            animator = GetComponentInChildren<Animator>();
            start_pos = transform.position;
            state_timer = 99f; //Find wander right away
            update_timer = Random.Range(-1f, 1f);

            if(wander != WanderBehavior.None)
                transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        void Start()
        {
            character.onAttack += OnAttack;
            destruct.onDamaged += OnDamaged;
            destruct.onDeath += OnDeath;

            last_eat_time = PlayerData.Get().GetCustomFloat(GetSubUID("eat_time"));
            last_grow_time = PlayerData.Get().GetCustomFloat(GetSubUID("grow_time"));

            if (last_eat_time < 0.01f)
                ResetEatTime();
            if (last_grow_time < 0.01f)
                ResetGrowTime();
        }

        void FixedUpdate()
        {
            if (TheGame.Get().IsPaused())
                return;

            //Optimization, dont run if too far
            float dist = (TheCamera.Get().GetTargetPos() - transform.position).magnitude;
            float active_range = Mathf.Max(detect_range * 2f, selectable.active_range * 0.8f);
            is_active = (state != LivestockState.Wander && state != LivestockState.Dead) || character.IsMoving() || dist < active_range;
        }

        private void Update()
        {
            //Animations
            bool paused = TheGame.Get().IsPaused();
            if (animator != null)
                animator.enabled = !paused;

            if (TheGame.Get().IsPaused())
                return;

            if (state == LivestockState.Dead || !is_active)
                return;

            state_timer += Time.deltaTime;
            find_timer += Time.deltaTime;

            //States
            if (state == LivestockState.Wander)
            {
                if (state_timer > wander_interval && wander != WanderBehavior.None)
                {
                    state_timer = Random.Range(-1f, 1f);
                    FindWanderTarget();
                    character.MoveTo(wander_target);
                }

                //Character stuck
                if (character.IsStuck())
                    character.Stop();
            }

            if (state == LivestockState.MoveTo)
            {
                if (character.HasReachedMoveTarget())
                    StopAction();
            }

            if (state == LivestockState.FindFood)
            {
                if (target_food == null)
                {
                    ChangeState(LivestockState.Wander);
                    return;
                }

                if(!character.IsTryMoving())
                    character.MoveTo(target_food.transform.position);

                float dist = (target_food.transform.position - transform.position).magnitude;
                if (dist < eat_range)
                {
                    StartEat();
                    return;
                }

                if (state_timer > 2f && character.IsStuck())
                    StopFindFood();

                if (state_timer > 10f)
                    StopFindFood();
            }

            if (state == LivestockState.Eat)
            {
                if (target_food == null)
                {
                    ChangeState(LivestockState.Wander);
                    return;
                }

                if (state_timer > 2f)
                {
                    FinishEat(target_food);
                }
            }

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = Random.Range(-0.1f, 0.1f);
                SlowUpdate(); //Optimization
            }

            //Animations
            if (animator != null && animator.enabled)
            {
                animator.SetBool("Move", IsMoving() && IsActive());
                animator.SetBool("Run", IsRunning() && IsActive());
            }
        }

        private void SlowUpdate()
        {
            if (state == LivestockState.Wander)
            {
                if (eat && IsHungry() && find_timer > 0f)
                {
                    target_food = AnimalFood.GetNearest(eat_food_group, transform.position, detect_range);
                    if (target_food != null)
                    {
                        ChangeState(LivestockState.FindFood);
                        character.Stop();
                    }
                }

                if (grow_to != null && GrowTimeFinished() && GetEatCount() >= grow_eat_count)
                {
                    Grow();
                }

                else if (item_produce != null && ProduceTimeFinished() && GetEatCount() >= item_eat_count)
                {
                    ProduceItem();
                }
            }
        }

        private void StartEat()
        {
            character.Stop();
            character.FaceTorward(target_food.transform.position);
            ChangeState(LivestockState.Eat);
            if(animator != null)
                animator.SetTrigger("Eat");
        }

        private void FinishEat(AnimalFood food)
        {
            if (food != null)
            {
                state_timer = 0f;
                food.EatFood();
                ResetEatTime();
                PlayerData.Get().SetCustomInt(GetSubUID("eat"), GetEatCount() + 1);
                ChangeState(LivestockState.Wander);
            }
        }

        private void StopFindFood()
        {
            find_timer = -5f; //Dont find try find food again for 5 secs
            ChangeState(LivestockState.Wander);
            FindWanderTarget();
            character.MoveTo(wander_target);
        }

        private void ProduceItem()
        {
            if (item_produce != null)
            {
                ResetGrowTime();
                PlayerData.Get().SetCustomInt(GetSubUID("eat"), 0);

                if (item_collect_type == LivestockProduceType.DropFloor)
                {
                    int count_animal = Character.CountSceneObjects(character.data, transform.position, item_max_range);
                    int count_item = Item.CountSceneObjects(item_produce, transform.position, item_max_range);

                    if (count_animal * item_max > count_item)
                    {
                        Item.Create(item_produce, transform.position);
                    }
                }


                if (item_collect_type == LivestockProduceType.CollectAction)
                {
                    int nb = GetProductCount();
                    if (nb < item_max)
                    {
                        PlayerData.Get().SetCustomInt(GetSubUID("product"), nb + 1);
                    }
                }
            }
        }

        private void Grow(){

            if (grow_to != null)
            {
                PlayerData.Get().RemoveCharacter(GetUID());
                PlayerData.Get().RemoveObject(GetUID());
                PlayerData.Get().SetCustomInt(GetSubUID("eat"), 0);
                ResetGrowTime();
                Destroy(gameObject);

                Character.Create(grow_to, transform.position, transform.rotation);
            }
        }

        private void FindWanderTarget()
        {
            float range = Random.Range(0f, wander_range);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 spos = wander == WanderBehavior.WanderFar ? transform.position : start_pos;
            Vector3 pos = spos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * range;
            wander_target = pos;
        }

        public void CollectProduct(PlayerCharacter player)
        {
            int nb = GetProductCount();
            if (nb > 0)
            {
                PlayerData.Get().SetCustomInt(GetSubUID("product"), nb - 1);
                player.Inventory.GainItem(item_produce, 1);
            }
        }

        public void MoveToTarget(Vector3 pos, bool run)
        {
            is_running = run;
            ChangeState(LivestockState.MoveTo);
            character.MoveTo(pos);
        }

        public void StopAction()
        {
            character.Stop();
            is_running = false;
            target_food = null;
            ChangeState(LivestockState.Wander);
        }

        public void ChangeState(LivestockState state)
        {
            this.state = state;
            state_timer = 0f;
        }

        public void Reset()
        {
            StopAction();
            character.Reset();
            animator.Rebind();
        }

        private void ResetEatTime()
        {
            if (time_type == TimeType.GameDays)
                PlayerData.Get().SetCustomFloat(GetSubUID("eat_time"), PlayerData.Get().day);
            if (time_type == TimeType.GameHours)
                PlayerData.Get().SetCustomFloat(GetSubUID("eat_time"), PlayerData.Get().GetTotalTime());
        }

        private void ResetGrowTime()
        {
            if (time_type == TimeType.GameDays)
                PlayerData.Get().SetCustomFloat(GetSubUID("grow_time"), PlayerData.Get().day);
            if (time_type == TimeType.GameHours)
                PlayerData.Get().SetCustomFloat(GetSubUID("grow_time"), Mathf.RoundToInt(PlayerData.Get().GetTotalTime()));
        }

        private bool EatTimeFinished()
        {
            float last_eat_time = PlayerData.Get().GetCustomFloat(GetSubUID("eat_time"));
            if (time_type == TimeType.GameDays && HasUID())
                return PlayerData.Get().day >= Mathf.RoundToInt(last_eat_time + eat_interval_time);
            if (time_type == TimeType.GameHours && HasUID())
                return PlayerData.Get().GetTotalTime() > last_eat_time + eat_interval_time;
            return false;
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

        private bool ProduceTimeFinished()
        {
            float last_grow_time = PlayerData.Get().GetCustomFloat(GetSubUID("grow_time"));
            if (time_type == TimeType.GameDays && HasUID())
                return PlayerData.Get().day >= Mathf.RoundToInt(last_grow_time + item_produce_time);
            if (time_type == TimeType.GameHours && HasUID())
                return PlayerData.Get().GetTotalTime() > last_grow_time + item_produce_time;
            return false;
        }

        private void OnDamaged()
        {
            if (IsDead())
                return;

            if (onDamaged != null)
                onDamaged.Invoke();

            if (animator != null)
                animator.SetTrigger("Damaged");
        }

        private void OnDeath()
        {
            state = LivestockState.Dead;

            if (onDeath != null)
                onDeath.Invoke();

            if (animator != null)
                animator.SetTrigger("Death");
        }

        void OnAttack()
        {
            if (animator != null)
                animator.SetTrigger("Attack");
        }

        public bool IsHungry()
        {
            return eat && EatTimeFinished();
        }

        public bool IsDead()
        {
            return character.IsDead();
        }

        public bool IsActive()
        {
            return is_active;
        }

        public bool IsMoving()
        {
            return character.IsMoving();
        }

        public bool IsRunning()
        {
            return character.IsMoving() && is_running;
        }

        public bool HasProduct()
        {
            return PlayerData.Get().GetCustomInt(GetSubUID("product")) > 0;
        }

        public int GetProductCount()
        {
            return PlayerData.Get().GetCustomInt(GetSubUID("product"));
        }

        public int GetEatCount()
        {
            return PlayerData.Get().GetCustomInt(GetSubUID("eat"));
        }

        public bool HasUID()
        {
            return character.HasUID();
        }

        public string GetUID()
        {
            return character.GetUID();
        }

        public string GetSubUID(string tag)
        {
            return character.GetSubUID(tag);
        }
    }

}
