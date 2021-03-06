﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public enum BrainType
    {
        NOP,
        GROUND_UNIT
    }

    [Serializable]
    public class BehaviourSettings {
        public BrainType brainType;

        public GameObject bulletPrefab;

        [Range(0.1f, 1)]
        public float moveSpeed = 1f;
        public float jumpForce = 2f;

        [Range(0.1f, 2f)]
        public float decisionsPerSec;
        public int shotsPerFireDecision;

        [Range(0.01f, 1f)]
        public float shootSkill;

        [Range(0.01f, 1f)]
        public float agroness;

        public Transform home;
        public bool spawnIsHome = false;
        public bool respectHomeWhenAttackingPlayer = true;

        public float maxDistanceFromHome;
        public bool returnToHomeOnIdle;
        public float radiusOfAwareness;

        public bool canRepair;
        [Range(0f, 1f)]
        public float eagernessToSeekoutRepair;
    }

    public BehaviourSettings settings = new BehaviourSettings()
    {
        brainType = BrainType.GROUND_UNIT,
        moveSpeed = 0.01f,
        decisionsPerSec = 1f,
        shootSkill = 1,
        agroness = 0.5f,
        shotsPerFireDecision = 1,
        spawnIsHome = true,
        respectHomeWhenAttackingPlayer = true,
        maxDistanceFromHome = 2f,
        radiusOfAwareness = 5f,
        canRepair = true
    };

    public abstract class Action {
        public static float moveScaler = 1f / 10f;
        public readonly static Action NOP = new NopAction();
        public int remainingTickLifetime;

        public int delayAsTicks;

        public static Type[] ALL_ACTION_TYPES = new Type[] {
            typeof(LeftAction),
            typeof(RightAction)
        };

        public Action() {
            this.remainingTickLifetime = getInitialTickLifetime();
            setDelay(0);
        }

        abstract public int getInitialTickLifetime();

        public void setDelay(int newDelayAsTicks) {
            delayAsTicks = newDelayAsTicks;
        }

        /*static Action() {
            ALL_ACTIONS = assembly.GetTypes().Where(t => t.BaseType == typeof(baseType));
        }*/

        public virtual void apply(MealyMachine mac) { }
    }

    private class LeftAction : Action {
        public override void apply(MealyMachine mac) {
            float ms = mac.settings.moveSpeed * Action.moveScaler;
            //Debug.Log("Going left");
            mac.selfTransform.position = mac.selfTransform.position - new Vector3(ms, 0, 0);
        }

        public override int getInitialTickLifetime() {
            return 1000;
        }
    }

    private class RightAction : Action  
    {
        public override void apply(MealyMachine mac)
        {
            //Debug.Log("Going right");
            float ms = mac.settings.moveSpeed * Action.moveScaler;

            mac.selfTransform.position = mac.selfTransform.position + new Vector3(ms, 0, 0);
        }

        public override int getInitialTickLifetime()
        {
            return 1000;
        }
    }

    private class JumpAction : Action
    {
        public override void apply(MealyMachine mac)
        {
            var rb = mac.self.GetComponent<Rigidbody2D>();
            
            if (rb == null)
            {
                //Debug.Log("Cannot jump don't have rigitbody");
            }
            else {
                Debug.Log("applying jump force");
                rb.AddForce(new Vector2(0, mac.settings.jumpForce), ForceMode2D.Impulse);
                //rb.AddForce(Vector3.up);
            }
        }

        public override int getInitialTickLifetime()
        {
            return 0;
        }
    }

    private class NopAction : Action
    {
        public override void apply(MealyMachine mac)
        {
            //Debug.Log("Doing NOP");
        }

        public override int getInitialTickLifetime()
        {
            return 0;
        }

    }

    private class ShootAction : Action
    {
        Vector3 direction;

        public ShootAction(Vector3 direction) {
            this.direction = new Vector3(direction.x, direction.y, 0);
            //this.direction = direction;
        }

        public override void apply(MealyMachine mac)
        {
            var initialPosition = mac.selfTransform.position;
            var forceToApply = direction * 100;

            //Debug.Log("Shooting");
            if (mac.settings.bulletPrefab == null) {
                Debug.LogWarning("No bullet prefab for enemy using debug fallback");
                var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bullet.transform.position = initialPosition;
                bullet.transform.localScale = Vector3.one * 0.1f;
                var rb = bullet.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
                rb.isKinematic = false;
                rb.AddForce(forceToApply);
                Destroy(bullet, 100);
            }
            else {
                var fp = mac.self.transform.Find("Firepoint");
                //var angle = Vector3.Angle(Vector3.right, direction);
                var angle = Brain.PosNegAngle(Vector3.right, direction, Vector3.forward);
                fp.transform.rotation = Quaternion.Euler(0, 0, angle);
                //Debug.Log("Instantiate bullet");
                var bullet = Instantiate(mac.settings.bulletPrefab, initialPosition, fp.transform.rotation);
                //Debug.Log(bullet.transform.position.ToString());
                
                //var rb = bullet.GetComponent<Rigidbody2D>();
                //rb.AddForce(forceToApply);
            }

        }

        public override int getInitialTickLifetime()
        {
            return 0;
        }
    }

    private class Brain {
        protected readonly System.Random random = new System.Random();

        protected MealyMachine mac;

        float actionDecisionCoolDownTimeSec = 0f;
        List<Action> lastActions = new List<Action>();

        public Brain(MealyMachine machine) {
            //Debug.Log("new brain");
            this.mac = machine;
        }

        public void modifyBehaviour() {
            if (mac.enemyController != null) {
                var currentHealth = mac.enemyController.GetCurrentHealth();
                if (currentHealth <= 50) {
                    mac.settings.moveSpeed = mac.initialMoveSpeed * 2;
                    mac.settings.respectHomeWhenAttackingPlayer = false;
                }
            }
        }

        public virtual void tick() {
            //Debug.Log(actionDecisionCoolDownTimeSec);

            modifyBehaviour();

            //Debug.Log("Diff: " + actionDecisionCoolDownTimeSec + " - " + Time.fixedDeltaTime);
            actionDecisionCoolDownTimeSec -= Time.fixedDeltaTime;
            if (actionDecisionCoolDownTimeSec < 0) {
                actionDecisionCoolDownTimeSec = 0;
            }

            foreach (Action la in lastActions) {
                la.delayAsTicks--;
                if (la.delayAsTicks < 0) {
                    la.delayAsTicks = 0;
                }
            }

            List<Action> newLastActions = new List<Action>();
            foreach (Action la in lastActions)
            {
                if (la.remainingTickLifetime > 0)
                {
                    newLastActions.Add(la);
                }
            }

            this.lastActions = newLastActions;

            foreach (Action la in lastActions) {
                if (la.delayAsTicks > 0) {
                    continue;
                }

                la.remainingTickLifetime--;
            }
        }

        public List<Action> getActions() {
            List<Action> ret = new List<Action>();
            foreach (Action la in lastActions)
            {
                if (la.delayAsTicks <= 0) {
                    ret.Add(la);
                }
            }

            return ret;
        }

        public virtual void setActions(List<Action> newActions) {
            if (actionDecisionCoolDownTimeSec <= 0) {
                this.lastActions = newActions;
                // when changing in editor, short time becomes NaN, ai unresponsive, use default in that case
                var decisionsPerSec = mac.settings.decisionsPerSec;
                if (decisionsPerSec < 0.1f) {
                    decisionsPerSec = 1f;
                }

                this.actionDecisionCoolDownTimeSec = 1 / decisionsPerSec;
            }
            else {
                Debug.LogWarning("Tried to set actions before cooldown finished");
            }
        }

        public List<Action> moveToTarget(Transform transform, float keepDistance, bool respectHome) {
            var ret = new List<Action>();

            var deltaToTarget = mac.selfTransform.position - transform.position;
            var distToTarget = deltaToTarget.magnitude;

            bool allowMoveToTarget = true;
            if (respectHome) {
                if (mac.settings.home == null) {
                    Debug.LogWarning("respect home was given but no home was provided");
                }
                else {
                    var deltaToHome = mac.selfTransform.position - mac.settings.home.position;
                    var distToTHome = deltaToHome.magnitude;

                    if (distToTHome > mac.settings.maxDistanceFromHome) {
                        var distOfTargetToHome = (transform.position - mac.settings.home.transform.position).magnitude;
                        if (distOfTargetToHome < distToTHome) {
                            allowMoveToTarget = true;
                            keepDistance = 0; // HACKZZ
                        } else {
                            allowMoveToTarget = false;
                        }
                        
                    }
                }
            }

            var xDistToTarget = Mathf.Abs(transform.position.x - mac.selfTransform.position.x);

            var allowJump = distToTarget < mac.settings.radiusOfAwareness && allowMoveToTarget && xDistToTarget > 3;

            if (transform.position.y > mac.self.transform.position.y + 4 && allowJump)
            {
                Debug.Log("Attempting to jump");
                var dir = transform.position - mac.selfTransform.position;
                var acts = jumpInDirection(dir);
                foreach (var ac in acts) {
                    ret.Add(ac);
                }
            }


            if (distToTarget > keepDistance && allowMoveToTarget)
            {
                if (deltaToTarget.x > 0)
                {
                    ret.Add(new LeftAction());
                }
                else
                {
                    ret.Add(new RightAction());
                }
            } else {
                if (deltaToTarget.x < 0)
                {
                    ret.Add(new LeftAction());
                }
                else
                {
                    ret.Add(new RightAction());
                }
            }


            return ret;
        }

        public GameObject findClosestPlayer() {
            GameObject closestPlayer = null;
            float closestDistance = float.MaxValue;
            foreach (GameObject player in mac.players) {
                if (closestPlayer == null) {
                    float dist2 = Vector3.Distance(player.transform.position,  mac.selfTransform.position);
                    if (dist2 > mac.settings.radiusOfAwareness) {
                        continue;
                    }

                    closestPlayer = player;
                    closestDistance = dist2;
                    continue;
                }

                float dist = Vector3.Distance(player.transform.position, mac.selfTransform.position);

                if (dist > mac.settings.radiusOfAwareness) {
                    continue;
                }

                if (dist < closestDistance) {
                    closestDistance = dist;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }

        public List<Action> attackPlayer(GameObject player)
        {
            var ret = new List<Action>();

            foreach (var a in moveToTarget(player.transform, 3, mac.settings.respectHomeWhenAttackingPlayer))
            {
                ret.Add(a);
            }

            int shotCount = mac.settings.shotsPerFireDecision;
            float delayBetweenShots = mac.settings.decisionsPerSec / mac.settings.shotsPerFireDecision;


            var shots = createActionShoot(player, shotCount, delayBetweenShots);
            foreach (var sc in shots)
            {
                ret.Add(sc);
            }

            return ret;
        }

        public List<Action> getIdleActions() {
            var ret = new List<Action>();

            if (mac.settings.returnToHomeOnIdle) {
                foreach (var ac in moveToTarget(mac.settings.home, 0, false)) {
                    ret.Add(ac);
                }                
            } else {
                // TODO
            }

            return ret;
        }

        public List<ShootAction> createActionShoot(GameObject target, int shotCount, float delayBetweenShots) {
            var ret = new List<ShootAction>();
            for (int i = 0; i < shotCount; i++) {
                Vector3 shootPos = target.transform.position;
                float role = (float)random.NextDouble();
                if (role <= mac.settings.shootSkill)
                {
                    // success, no penalty
                }
                else
                {
                    // TODO: needs improvement
                    float yErr = (float)random.NextDouble() * (role * 10) - (role * 10 / 2);
                    shootPos = target.transform.position + new Vector3(0, yErr, 0);
                }

                var dir = (mac.selfTransform.position - shootPos).normalized;
                var sc = new ShootAction(-dir);
                var delay = delayBetweenShots * i;
                var delayInTicks = (int) (delay / Time.fixedDeltaTime);
                sc.setDelay(delayInTicks);
                ret.Add(sc);
            }

            return ret;
        }

        public static float PosNegAngle(Vector3 a1, Vector3 a2, Vector3 normal)
        {
            float angle = Vector3.Angle(a1, a2);
            float sign = Mathf.Sign(Vector3.Dot(normal, Vector3.Cross(a1, a2)));
            return angle * sign;
        }

        public List<Action> jumpInDirection(Vector3 dir)
        {
            var ret = new List<Action>();
            var ja = new JumpAction();

            var d = PosNegAngle(Vector3.up, dir, Vector3.forward);
            //var d = Vector3.Angle(Vector3.up, dir);
            //Debug.Log(d);

            // TODO: control strafe during jump

            if (d > 0)
            {
                var la = new LeftAction();
                ret.Add(la);
            }
            else
            {
                var ra = new RightAction();
                ret.Add(ra);
            }

            ret.Add(ja);
            return ret;
        }


        public bool shouldDecideOnNewAction() {
            return actionDecisionCoolDownTimeSec <= 0;
        }
    }

    // Select player and try to get close to him
    private class GroundBrain : Brain
    {
        private GameObject playerToStalk;


        public GroundBrain(MealyMachine machine) : base(machine)
        {
        }

        void updatePlayerToStalk() {
            if (mac.players.Count > 0)
            {
                playerToStalk = findClosestPlayer();
            }
        }




        public override void tick()
        {
            base.tick();
            if (!shouldDecideOnNewAction()) {
                return;
            }

            //var rot = Quaternion.AngleAxis(+45, Vector3.forward);
            //var lDirection = rot * Vector3.up;
//            var dir = mac.settings.home.transform.position - mac.selfTransform.position;
//            Debug.Log(lDirection.ToString());
//            setActions(jumpInDirection(dir));
 //           return;

            updatePlayerToStalk();

            var chosenActions = new List<Action>();

            if (playerToStalk != null) {
                var roleShouldShoot = random.NextDouble();
                if (roleShouldShoot <= mac.settings.agroness) {
                    //Debug.Log("Attacking player " + playerToStalk.name);
                    var moveAndAttackActions = attackPlayer(playerToStalk);
                    chosenActions = moveAndAttackActions;
                }
            }

            if (chosenActions.Count == 0) {
                chosenActions = getIdleActions();
            }

            setActions(chosenActions);
        }
    }

    public class MealyMachine {
        private BrainType brainType = BrainType.NOP;
        private Brain brain;

        public Transform selfTransform;
        public GameObject self;
        public BehaviourSettings settings;
        public EnemyController enemyController;
        public List<GameObject> players = new List<GameObject>();

        public float initialMoveSpeed;

        public MealyMachine(GameObject self, BehaviourSettings settings, EnemyController enemyController) {
            this.selfTransform = self.transform;
            this.self = self;
            brain = createBrainOfType(brainType);
            this.settings = settings;
            this.initialMoveSpeed = settings.moveSpeed;
            this.enemyController = enemyController;
        }


        public void tick() {
            // :-P
            if (players != null) {
                players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
            }

            brain.tick();
            var actions = brain.getActions();
            foreach (var action in actions) {
                action.apply(this);
            }

            //brain.postActionsApplied();
        }

        public void changeBrainIfNeeded(BrainType newBrainType) {
            if (!newBrainType.Equals(brainType)) {
                brain = createBrainOfType(newBrainType);
                brainType = newBrainType;
            }

            if (brain == null) {
                Debug.LogWarning("Brain type not set, using NOP");
                brain = createBrainOfType(BrainType.NOP);
            }
        }

        private Brain createBrainOfType(BrainType bt) {
            switch (bt) {
                case BrainType.NOP:
                    return new Brain(this);
                case BrainType.GROUND_UNIT:
                    return new GroundBrain(this);
                default:
                    Debug.LogError("Unimplemented brain type: " + brainType.ToString());
                    return new Brain(this);
            }
        }
    }

    MealyMachine mealyMachine;

    void Start() {
        if (settings.home == null && settings.spawnIsHome) {
            var go = new GameObject("Home of " + gameObject.name);
            settings.home = go.transform;
            settings.home.position = gameObject.transform.position;
        }

        var enemyController = GetComponent<EnemyController>();
        mealyMachine = new MealyMachine(gameObject, settings, enemyController);        
    }

    void FixedUpdate()
    {
        // for hot reload of unity
        if (mealyMachine == null) {
            return;
        }

        mealyMachine.changeBrainIfNeeded(settings.brainType);
        mealyMachine.tick();
    }
}
