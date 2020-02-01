using System;
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
        public float decisionsPerSec;
        public int shotsPerFireDecision;

        [Range(0.01f, 1f)]
        public float shootSkill;

        [Range(0.01f, 1f)]
        public float agroness;

        public Transform home;

        public float maxDistanceFromHome;
        public bool returnToHomeOnIdle;
        public float radiusOfAwareness;

        public bool canRepair;
        [Range(0f, 1f)]
        public float eagernessToSeekoutRepair;
    }

    public BehaviourSettings settings = new BehaviourSettings()
    {
        brainType = BrainType.NOP,
        decisionsPerSec = 1f,
        shootSkill = 1,
        agroness = 0.5f,
        shotsPerFireDecision = 1,
        maxDistanceFromHome = 2f,
        radiusOfAwareness = 5f,
        canRepair = true
    };

    public abstract class Action {
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
            //Debug.Log("Going left");
            mac.selfTransform.position = mac.selfTransform.position - new Vector3(mac.speed, 0, 0);
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

            mac.selfTransform.position = mac.selfTransform.position + new Vector3(mac.speed, 0, 0);
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
            var rb = mac.self.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.Log("Cannot jump don't have rigitbody");
            }
            else {
                rb.AddForce(Vector3.up);
            }
        }

        public override int getInitialTickLifetime()
        {
            return 1;
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
            return 1;
        }

    }

    private class ShootAction : Action
    {
        Vector3 direction;

        public ShootAction(Vector3 direction) {
            this.direction = direction;
        }

        public override void apply(MealyMachine mac)
        {
            //Debug.Log("Shooting");
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.position = mac.selfTransform.position + direction * 2;
            bullet.transform.localScale = Vector3.one * 0.1f;
            var rb = bullet.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            rb.isKinematic = false;
            rb.AddForce(direction * 100);
            Destroy(bullet, 100);
        }

        public override int getInitialTickLifetime()
        {
            return 1;
        }
    }

    private class Brain {
        protected readonly System.Random random = new System.Random();

        protected MealyMachine mac;

        float actionDecisionCoolDownTimeSec = 0f;
        List<Action> lastActions = new List<Action>();

        public Brain(MealyMachine machine) {
            Debug.Log("new brain");
            this.mac = machine;
        }

        public virtual void tick() {
            //Debug.Log(actionDecisionCoolDownTimeSec);

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

                this.actionDecisionCoolDownTimeSec = 1 / mac.settings.decisionsPerSec;
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

            if (transform.position.y > mac.self.transform.position.y + 2)
            {
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

            foreach (var a in moveToTarget(player.transform, 5, true))
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

        public float PosNegAngle(Vector3 a1, Vector3 a2, Vector3 normal)
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
        public List<GameObject> players = new List<GameObject>();

        public float speed = 0.01f;

        public MealyMachine(GameObject self, BehaviourSettings settings) {
            this.selfTransform = self.transform;
            this.self = self;
            brain = createBrainOfType(brainType);
            this.settings = settings;
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
        mealyMachine = new MealyMachine(gameObject, settings);
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
