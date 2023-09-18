using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class WingController
{
    public Transform wingTransform;
    public bool leftWing;
    public bool innerWing;
    public KeyCode key;
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : IManagedController
{
    [Header("References")]
    [SerializeField] private StaminaMeterController staminaMeterController;
    [SerializeField] private LayerMask nestLayer;
    [SerializeField] private LayerMask wallsLayer;

    private class WingData
    {
        public static float InnerWingGroundTarget;
        public static float OuterWingGroundTarget;

        public Transform transform;
        public KeyCode key;
        public bool leftWing;
        public bool innerWing;

        public bool lifting;
        public float liftVal; // [0,1]
        public float liftValTarget;
        public float downTime;
    }
    [Header("Wings")]
    [SerializeField] private List<WingController> wingControllers;

    [Header("Wing Settings")]
    [SerializeField] [Range(0.0f, 60.0f)] private float innerWingUpRotationBound;
    [SerializeField] [Range(0.0f, 40.0f)] private float outerWingUpRotationBound;
    [SerializeField] [Range(0.0f, 50.0f)] private float innerWingDownRotationBound;
    [SerializeField] [Range(0.0f, 40.0f)] private float outerWingDownRotationBound;
    [SerializeField] [Range(0.1f, 4.0f)] private float wingUpSpeed = 0.1f;
    [SerializeField] [Range(0.1f, 4.0f)] private float wingDownSpeed = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float initInnerWingForce = 0.1f;
    [SerializeField] [Range(0.1f, 1.0f)] private float initOuterWingForce = 0.1f;
    [SerializeField] [Range(0.1f, 3.0f)] private float initLateralWingForce = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float lateralMomentumScalar;
    [SerializeField] [Range(0.0f, 0.3f)] private float wingForceScalarTime; // Time it takes to get to 100% of wing force
    [SerializeField] [Range(1, 5)] private int wingForceScalarPower = 1;
    [SerializeField] [Range(0.01f, 0.33f)] private float innerWingDragForce = 0.01f;
    [SerializeField] [Range(0.01f, 0.33f)] private float outerWingDragForce = 0.01f;

    [Header("Grip Settings")]
    [SerializeField] [Range(0.0f, 1.0f)] private float initGripVal;
    [SerializeField] [Range(0.1f, 2.0f)] private float initGripDrainScalar = 0.1f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripGainAmount = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCollisionDrainAmount = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCollisionSaveAmount = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCrackThreshold = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float gripCrackedWaitThreshold = 0.01f;
    [SerializeField] [Range(0.0f, 2.0f)] private float invulnerabilityInitTime;

    [Header("Player Settings")]
    [SerializeField] [Range(5.0f, 15.0f)] private float rotationScalar = 5.0f;
    [SerializeField] [Range(1.0f, 200.0f)] private float eggGrabForceScalar = 1.0f;
    [SerializeField] [Range(0.1f, 8.0f)] private float maxVelocityX = 0.1f;
    [SerializeField] [Range(0.01f, 1.0f)] private float introWingUpTime = 0.01f;
    [SerializeField] [Range(0.01f, 1.0f)] private float introWingDownTime = 0.01f;

    private Rigidbody2D rb2d;
    private List<PolygonCollider2D> eggColliders;

    private List<WingData> wingDatas;

    // Wing forces
    private float innerWingForce;
    private float outerWingForce;
    private float lateralWingForce;

    private bool canInput;
    private bool canGrip;

    private EggController eggController;
    private bool gripping;
    private float gripVal;
    private float gripDrainScalar;
    private bool invulnerable;
    private float invulnerabilityTime;
    private bool gripCracked;

    private float initGrav;

    // Trapped settings
    private bool isPlayerTrapped;
    private int currentPlayerMashValue;
    
    [Header("Player Trapped Settings")]
    [SerializeField] private int playerMashTotal;
    [SerializeField] [Range(1.0f, 2.0f)] private float gripAmplifyingFactor = 0.75f;
    [SerializeField] [Range(0.0f, 1.0f)] private float wingDampeningFactor = 0.5f;

    override protected void ManagedStart()
    {
        rb2d = GetComponent<Rigidbody2D>();

        // Init wing forces and grip drain
        innerWingForce = initInnerWingForce;
        outerWingForce = initOuterWingForce;
        lateralWingForce = initLateralWingForce;
        gripDrainScalar = initGripDrainScalar;

        wingDatas = new List<WingData>(wingControllers.Count);
        for (int i = 0; i < wingControllers.Count; i++)
        {
            WingData wingData = new WingData
            {
                transform = wingControllers[i].wingTransform,
                key = wingControllers[i].key,
                leftWing = wingControllers[i].leftWing,
                innerWing = wingControllers[i].innerWing,

                lifting = false,
                liftVal = 0,
                downTime = 0,
            };
            UpdateWingLiftValTarget(wingData);
            wingData.liftVal = wingData.liftValTarget;
            UpdateWingRot(wingData);
            wingDatas.Add(wingData);
        }
        WingData.InnerWingGroundTarget = innerWingDownRotationBound / (innerWingDownRotationBound + innerWingUpRotationBound);
        WingData.OuterWingGroundTarget = outerWingDownRotationBound / (outerWingDownRotationBound + outerWingUpRotationBound);

        initGrav = rb2d.gravityScale;
        if ((innerWingDragForce * 2) + (outerWingDragForce * 2) > initGrav)
        {
            Debug.LogError("Drag forces too high!!!");
        }

        eggColliders = new List<PolygonCollider2D>(GetComponents<PolygonCollider2D>());

        canInput = true;
        canGrip = true;

        isPlayerTrapped = false;
        currentPlayerMashValue = 0;
    }

    override public void OnStateChanged(PlayState oldState, PlayState newState)
    {
        rb2d.simulated = newState != PlayState.PAUSE;
        switch (newState)
        {
            case PlayState.WIN:
            {
                canGrip = false;
                if (gripping) DropEgg();
                break;
            }
            case PlayState.LOSE:
            {
                canInput = false;
                if (gripping) DropEgg();
                break;
            }
        }
    }

    override public void ManagedUpdate()
    {
        if (PlayController.Instance.State == PlayState.INTRO) return;

        CheckForWingInput();
        if (canGrip) HandleGrip();

        transform.localEulerAngles = new Vector3(0, 0, rb2d.velocity.x * -rotationScalar);
    }

    override public void ManagedFixedUpdate()
    {
        if (PlayController.Instance.State == PlayState.INTRO) return;

        foreach (WingData wingData in wingDatas)
        {
            UpdateWingLiftValTarget(wingData);
            UpdateWingData(wingData);
            UpdateWingRot(wingData);
            CheckApplyForce(wingData);
        }
        CheckForWingDrag();

        if (Mathf.Abs(rb2d.velocity.x) > maxVelocityX)
            rb2d.velocity = new Vector2(rb2d.velocity.x > 0 ? maxVelocityX : -maxVelocityX, rb2d.velocity.y);

        if (isPlayerTrapped)
        {
            CheckForPlayerMash();
        }
    }

    private bool GetPlayerInputKey(KeyCode key, bool keyDown=true)
    {
        if (!canInput) return false;
        return keyDown ? Input.GetKeyDown(key) : Input.GetKey(key);
    }

    private void CheckForWingInput()
    {
        for (int i = 0; i < wingDatas.Count; i++)
        {
            WingData wingData = wingDatas[i];
            if (!wingData.lifting && GetPlayerInputKey(wingData.key, false))
            {
                wingData.lifting = true;
            }
            else if (wingData.lifting && !GetPlayerInputKey(wingData.key, false))
            {
                wingData.lifting = false;
            }
        }
    }

    private void CheckForPlayerMash()
    {
        // Check if any key has been pressed and then any key released
        // TODO: not great as if, improvement will be to do wing keys
        // TODO: further improvement would be to detect flaps
        // TODO: further further improvment is to rely on time - faster flaps = shorter trap time
        if (Input.anyKeyDown)
        {
            currentPlayerMashValue += 1;
        }

        if (currentPlayerMashValue > playerMashTotal)
        {
            UntrapPlayer();
        }
    }

    private void HandleGrip()
    {
        if (staminaMeterController) staminaMeterController.UpdatePosition(transform.position);

        if (invulnerabilityTime > 0)
        {
            invulnerabilityTime -= Time.deltaTime;
            if (invulnerabilityTime <= 0)
            {
                invulnerable = false;
            }
        }

        if (gripping)
        {
            if (GetPlayerInputKey(KeyCode.LeftControl) || GetPlayerInputKey(KeyCode.RightControl))
            {
                DropEgg();
                return;
            }

            gripVal -= Time.deltaTime * gripDrainScalar;
            if (gripCracked)
            {
                if (gripVal < gripCrackedWaitThreshold) gripCracked = false;
            }
            else if (GetPlayerInputKey(KeyCode.Space))
            {
                // If the player tries gripping harder while they're past the gripCrackThreshold
                //  and the amount added will hit the top of the grip bar, take egg damage
                if (gripVal >= gripCrackThreshold && (gripVal + gripGainAmount) >= 1)
                {
                    PlayController.Instance.TakeEggDamage();
                    // Make sure they don't do it again by forcing the player to wait until they grip again
                    gripCracked = true;
                }
                gripVal += gripGainAmount;
                if (gripVal > 1) gripVal = 1;
            }
            staminaMeterController.SetStaminaPercentage(gripVal);

            if (gripVal <= 0)
            {
                DropEgg();
            }
        }
        else if (GetPlayerInputKey(KeyCode.Space))
        {
            GrabEgg(); // Sets gripping value in here
            if (gripping)
            {
                gripVal = initGripVal;
            }
        }
    }

    private void UpdateWingLiftValTarget(WingData wingData)
    {
        if (wingData.lifting)
        {
            wingData.liftValTarget = 1;
        }
        else
        {
            if (IsOnGround())
                wingData.liftValTarget = wingData.innerWing ? WingData.InnerWingGroundTarget : WingData.OuterWingGroundTarget;
            else
                wingData.liftValTarget = 0;
        }
    }

    private bool IsOnGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.9f, wallsLayer);
        return hit.collider != null;
    }

    private void UpdateWingData(WingData wingData)
    {
        float liftValDiff = wingData.liftValTarget - wingData.liftVal;
        float liftValDelta = (liftValDiff > 0 ? wingUpSpeed : -wingDownSpeed) * 100 * Time.fixedDeltaTime /
            (wingData.innerWing ? (innerWingDownRotationBound + innerWingUpRotationBound) : (outerWingDownRotationBound + outerWingUpRotationBound));
        if (Mathf.Abs(liftValDelta) > Mathf.Abs(liftValDiff))
            liftValDelta = liftValDiff;
        wingData.liftVal = Mathf.Clamp01(wingData.liftVal + liftValDelta);

        if (wingData.liftValTarget - wingData.liftVal < 0)
            wingData.downTime += Time.deltaTime;
        else if (wingData.downTime > 0)
            wingData.downTime = 0;
    }

    private void UpdateWingRot(WingData wingData)
    {
        float upperBound = wingData.innerWing ? innerWingUpRotationBound : outerWingUpRotationBound;
        float lowerBound = -(wingData.innerWing ? innerWingDownRotationBound : outerWingDownRotationBound);
        float wingRot = ((wingData.liftVal * (upperBound - lowerBound)) + lowerBound) * (wingData.leftWing ? -1 : 1);
        wingData.transform.localEulerAngles = new Vector3(0, 0, wingRot);
    }

    private void CheckApplyForce(WingData wingData)
    {
        if (wingData.liftValTarget - wingData.liftVal < 0)
        {
            float forceScalar = Mathf.Min(Mathf.Pow((wingData.downTime + wingData.liftValTarget) / wingForceScalarTime, wingForceScalarPower), 1);
            float lateralForce = lateralWingForce * (wingData.leftWing ? 1 : -1) + (rb2d.velocity.x * lateralMomentumScalar);
            Vector2 wingForceVec = new Vector2(lateralForce, 1) * (wingData.innerWing ? innerWingForce : outerWingForce) * forceScalar;
            rb2d.velocity += wingForceVec;
        }
    }

    private void CheckForWingDrag()
    {
        float gravOffset = 0;
        foreach (WingData wingData in wingDatas)
        {
            if (wingData.liftVal >= 0.5f && wingData.lifting)
            {
                gravOffset += wingData.innerWing ? innerWingDragForce : outerWingDragForce;
            }
        }
        rb2d.gravityScale = initGrav - gravOffset;
    }

    private void GrabEgg()
    {
        if (!eggController) return;
        gripping = true;

        eggController.GrabEgg(transform);
        transform.localEulerAngles = new Vector3(0, 0, 0);
        transform.position = eggController.transform.position;
        eggController.transform.localPosition = new Vector2(0, -0.5f);
        rb2d.velocity = Vector2.zero;
        Physics2D.IgnoreLayerCollision(gameObject.layer, Mathf.FloorToInt(Mathf.Log(nestLayer.value, 2)));

        // The build doesn't like it when we create colliders :(
        // List<PolygonCollider2D> colliders = eggController.GetEggColliders();
        // eggColliders = new List<PolygonCollider2D>();
        // foreach (PolygonCollider2D eggCollider in colliders)
        // {
        //     PolygonCollider2D newCollider = gameObject.AddComponent<PolygonCollider2D>().GetCopyOf(eggCollider);
        //     newCollider.isTrigger = eggCollider.isTrigger;
        //     newCollider.offset = new Vector2(0, newCollider.offset.y - 0.5f);
        //     eggColliders.Add(newCollider);
        // }
        foreach (PolygonCollider2D eggCollider in eggColliders)
        {
            eggCollider.enabled = true;
        }

        rb2d.AddForce(Vector2.up * eggGrabForceScalar);
        SetInvulnerable();

        staminaMeterController.Enter();
    }

    private void DropEgg()
    {
        if (gripping && eggController)
        {
            eggController.DropEgg();
            foreach (PolygonCollider2D collider in eggColliders)
            {
                // Destroy(collider);
                collider.enabled = false;
            }
            // eggColliders = null;
            eggController.SetEggOutlineVisible(false);
            eggController = null;
            staminaMeterController.Exit();
            Physics2D.IgnoreLayerCollision(gameObject.layer, Mathf.FloorToInt(Mathf.Log(nestLayer.value, 2)), false);

            gripping = false;
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayController.Instance.State != PlayState.RUNNING) return;

        if (other.gameObject.CompareTag("Egg"))
        {
            eggController = other.transform.parent.gameObject.GetComponent<EggController>();
            eggController.SetEggOutlineVisible(true);
        }
        else if (other.gameObject.CompareTag("Nest") && gripping && eggController)
        {
            // TODO Maybe when in nestshow sprite mask so wings don't clip out of nest?
            PlayController.Instance.WinLevel();
        }
        else if (other.gameObject.CompareTag("Token"))
        {
            TokenController tokenController = other.GetComponent<TokenController>();
            if (!tokenController.Collected)
            {
                PlayController.Instance.CollectToken();
                tokenController.CollectToken();
            }
        }

        // TODO: ideally we handle this better with a script we can attach to any game object separate from main logic
        // TODO: something like a damage emitter and damage accumulator script or whatever and that can handle checking and storing
        // TODO: damage on its own
        if (other.gameObject.CompareTag(Constants.DAMAGE_EMITTER_TAG))
        {
            // we lose if we touch something that can damage the player
            PlayController.Instance.LoseLevel();
        }
    }
    
    protected void OnTriggerExit2D(Collider2D other)
    {
        if (PlayController.Instance.State != PlayState.RUNNING) return;

        if (other.gameObject.CompareTag("Egg") && !gripping)
        {
            eggController.SetEggOutlineVisible(false);
            eggController = null;
        }
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (PlayController.Instance.State != PlayState.RUNNING) return;

        if ((collision.gameObject.CompareTag("Walls") || collision.gameObject.CompareTag(Constants.HAZARD_SOURCE_TAG)) && gripping && !invulnerable)
        {
            float drainAmount = gripCollisionDrainAmount;
            if (gripVal > gripCollisionSaveAmount)
            {
                drainAmount = Mathf.Min(gripCollisionDrainAmount, gripVal - gripCollisionSaveAmount);
            }
            gripVal -= drainAmount;
            staminaMeterController.SetStaminaPercentage(gripVal);
            staminaMeterController.OnCollision();
            SetInvulnerable();
        }
    }

    private void SetInvulnerable()
    {
        invulnerabilityTime = invulnerabilityInitTime;
        invulnerable = true;
    }

    public bool IsTrapped()
    {
        return isPlayerTrapped;
    }

    public void TrapPlayer()
    {
        isPlayerTrapped = true;
        
        gripDrainScalar = initGripDrainScalar * gripAmplifyingFactor;
        innerWingForce = initInnerWingForce * wingDampeningFactor;
        outerWingForce = initOuterWingForce * wingDampeningFactor;
        lateralWingForce = initLateralWingForce * wingDampeningFactor;

        currentPlayerMashValue = 0;
    }

    public void UntrapPlayer()
    {
        isPlayerTrapped = false;
        
        gripDrainScalar = initGripDrainScalar;
        innerWingForce = initInnerWingForce;
        outerWingForce = initOuterWingForce;
        lateralWingForce = initLateralWingForce;
    }

    public void RunIntroSequence()
    {
        Sequence seq = DOTween.Sequence();
        int wingCount = wingDatas.Count;
        for (int i = 0; i < (wingCount * 2); i++)
        {
            WingData wingData = wingDatas[i % wingCount];
            bool wingUp = i < wingCount;
            float wingFlapTo = wingUp ?
                wingData.innerWing ? innerWingUpRotationBound : outerWingUpRotationBound + innerWingUpRotationBound :
                wingData.innerWing ? -innerWingDownRotationBound : -outerWingDownRotationBound - innerWingDownRotationBound;
            wingFlapTo *= wingData.leftWing ? -1 : 1;
            var wingAnim = wingData.transform.DORotate(new Vector3(0, 0, wingFlapTo), wingUp ? introWingUpTime : introWingDownTime);
            wingAnim.SetEase(wingData.innerWing ? Ease.InOutQuad : Ease.InOutSine);
            if (i == 0 || i == wingCount)
                seq.Append(wingAnim);
            else
                seq.Join(wingAnim);
        }
        seq.AppendCallback(() => PlayController.Instance.FinishIntroSequence());
    }
}
