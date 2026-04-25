using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMonster : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;
    [SerializeField] string playerTag = "Player";

    [Header("Movement")]
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float idleSpeed = 1.5f;
    [SerializeField] float chaseSpeed = 3.5f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string walkBool = "isWalking";
    [SerializeField] string stunnedBool = "isStunned";

    [Header("Detection")]
    [SerializeField] MonoBehaviour detectionStrategyComponent;

    [Header("Sanity Damage")]
    [SerializeField] float sanityDamagePerSecond = 0f;

    [Header("Patrol")]
    [SerializeField] bool patrolEnabled = false;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] float patrolSpeed = 2.5f;
    [SerializeField] float patrolWaitSeconds = 0.5f;
    [SerializeField] bool patrolPingPong = false;
    [SerializeField] bool patrolRandom = false;

    [Header("Strategy Guard")]
    [SerializeField] bool forceEnableStrategyOnStart = true;
    [SerializeField] int enableGuardFrames = 20;

    [Header("Light Stun")]
    [SerializeField] bool canBeStunnedByLight = true;
    [SerializeField] float stunSeconds = 2.5f;
    [SerializeField] bool freezeAgentOnStun = true;
    [SerializeField] AudioClip stunSfx;
    [SerializeField, Range(0f, 1f)] float stunSfxVolume = 1f;

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip detectionSfx;
    [SerializeField, Range(0f, 1f)] float detectionSfxVolume = 1f;
    [SerializeField] float detectionSfxRearmSeconds = 1.0f;

    [Header("Pinning Control")]
    [SerializeField] float pinStopDistance = 1.1f;
    [SerializeField] float pinResumeDistance = 1.6f;
    [SerializeField] float pinBackWallCheck = 0.6f;
    [SerializeField] float pinCheckHeight = 1.2f;
    [SerializeField] LayerMask environmentMask = ~0;

    private IDetectionStrategy _detection;
    private NavMeshAgent _agent;
    private bool _isChasing;
    private Vector3 _lastPerceivedTargetPos;
    private int _patrolIndex;
    private int _patrolDir = 1;
    private float _patrolWaitTimer;
    private int _guardCounter;
    private bool _isStunned;
    private float _stunEndTime;
    private float _suppressUntilTime;
    private bool _isPinning;
    private bool _detectionArmed = true;
    private float _lastNotDetectTime;
    private GameObject _playerGO;

    public bool CurrentlyDetecting { get; private set; }
    public bool IsStunned => _isStunned;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        _playerGO = GameObject.FindWithTag(playerTag);
        if (!target && _playerGO) target = _playerGO.transform;
        BindDetection();
        if (_agent)
        {
            _agent.speed = idleSpeed;
            _agent.stoppingDistance = stoppingDistance;
            _agent.autoBraking = true;
        }
        _guardCounter = enableGuardFrames;
        _detectionArmed = true;
        _lastNotDetectTime = Time.time;
    }

    void OnEnable()
    {
        if (forceEnableStrategyOnStart) EnableStrategy();
        _guardCounter = enableGuardFrames;
    }

    void Update()
    {
        if (_isStunned)
        {
            if (Time.time >= _stunEndTime) EndStun();
            else { HoldPosition(); return; }
        }

        if (Time.time < _suppressUntilTime)
        {
            PatrolUpdate(false);
            return;
        }

        if (!target || _agent == null || _detection == null) { PatrolUpdate(false); return; }
        if (!_agent.isOnNavMesh) { PatrolUpdate(false); return; }

        if (forceEnableStrategyOnStart && _guardCounter > 0) { EnableStrategy(); _guardCounter--; }

        bool prevDetect = CurrentlyDetecting;
        Vector3 perceivedPos;
        CurrentlyDetecting = _detection.Detect(target, out perceivedPos);

        float dist = Vector3.Distance(transform.position, target.position);
        bool pinNow = CheckPinning(dist);
        if (pinNow)
        {
            _isPinning = true;
            HoldPosition();
            Face(target.position);
            return;
        }
        else if (_isPinning && dist > pinResumeDistance)
        {
            _isPinning = false;
        }

        if (CurrentlyDetecting && !prevDetect && _detectionArmed)
        {
            if (detectionSfx)
            {
                if (sfxSource) sfxSource.PlayOneShot(detectionSfx, detectionSfxVolume);
                else AudioSource.PlayClipAtPoint(detectionSfx, transform.position, detectionSfxVolume);
            }
            _detectionArmed = false;
        }

        if (CurrentlyDetecting)
        {
            _isChasing = true;
            _lastPerceivedTargetPos = perceivedPos;
            Chase(perceivedPos);
        }
        else
        {
            if (!_detectionArmed && Time.time - _lastNotDetectTime >= detectionSfxRearmSeconds) _detectionArmed = true;
            _lastNotDetectTime = Time.time;

            if (_isChasing)
            {
                float d = Vector3.Distance(transform.position, _lastPerceivedTargetPos);
                if (d > stoppingDistance * 1.1f) Chase(_lastPerceivedTargetPos);
                else { _isChasing = false; PatrolUpdate(true); }
            }
            else
            {
                PatrolUpdate(false);
            }
        }
    }

    bool CheckPinning(float distToPlayer)
    {
        if (distToPlayer > pinStopDistance) return false;
        Vector3 center = target.position + Vector3.up * pinCheckHeight;
        Vector3 toEnemy = (transform.position - target.position);
        toEnemy.y = 0f;
        if (toEnemy.sqrMagnitude < 0.0001f) return false;
        Vector3 backDir = -toEnemy.normalized;
        if (Physics.SphereCast(center, 0.3f, backDir, out var hit, pinBackWallCheck, environmentMask, QueryTriggerInteraction.Ignore))
            return true;
        return false;
    }

    void PatrolUpdate(bool justLostTarget)
    {
        if (!patrolEnabled || patrolPoints == null || patrolPoints.Length == 0)
        {
            HoldPosition();
            return;
        }

        if (_patrolWaitTimer > 0f)
        {
            _patrolWaitTimer -= Time.deltaTime;
            HoldPosition();
            return;
        }

        Transform wp = patrolPoints[Mathf.Clamp(_patrolIndex, 0, patrolPoints.Length - 1)];

        if (wp == null)
        {
            HoldPosition();
            return;
        }


        if (_agent.isOnNavMesh)
        {
            _agent.speed = patrolSpeed;
            if (!_agent.pathPending)
            {
                float d = Vector3.Distance(transform.position, wp.position);
                if (d <= Mathf.Max(stoppingDistance, _agent.stoppingDistance) + 0.1f)
                {
                    NextPatrolIndex();
                    _patrolWaitTimer = patrolWaitSeconds;
                    return;
                }
            }
            _agent.isStopped = false;
            _agent.SetDestination(wp.position);
        }
        else
        {
            HoldPosition();
        }


        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
    }

    void NextPatrolIndex()
    {
        if (!patrolPingPong)
        {
            if (patrolRandom && patrolPoints.Length > 1)
            {
                int next;
                do { next = Random.Range(0, patrolPoints.Length); } while (next == _patrolIndex);
                _patrolIndex = next;
            }
            else
            {
                _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        _patrolIndex += _patrolDir;
        if (_patrolIndex >= patrolPoints.Length) { _patrolIndex = patrolPoints.Length - 2; _patrolDir = -1; }
        else if (_patrolIndex < 0) { _patrolIndex = 1; _patrolDir = 1; }
    }

    void Chase(Vector3 pos)
    {
        if (_agent.isOnNavMesh)
        {
            _agent.speed = chaseSpeed;
            _agent.isStopped = false;
            _agent.SetDestination(pos);
        }
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, true);
        Face(pos);
    }

    void HoldPosition()
    {
        if (_agent.isOnNavMesh)
        {
            _agent.ResetPath();
            _agent.isStopped = true;
        }
        if (animator && !string.IsNullOrEmpty(walkBool)) animator.SetBool(walkBool, false);
    }

    void Face(Vector3 pos)
    {
        Vector3 p = pos; p.y = transform.position.y;
        Vector3 dir = p - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationSpeed);
    }

    void OnTriggerStay(Collider other)
    {
        if (sanityDamagePerSecond <= 0f) return;
        if (!other.CompareTag(playerTag)) return;
        if (!CurrentlyDetecting) return;
        var s = other.GetComponent<SanitySystem>();
        if (s != null) s.TakeDamage(sanityDamagePerSecond * Time.deltaTime);
    }

    void BindDetection()
    {
        _detection = null;
        if (detectionStrategyComponent is IDetectionStrategy ds)
        {
            _detection = ds;
            EnableStrategy();
            _detection.Initialize(this);
            return;
        }
        var comps = GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] is IDetectionStrategy s)
            {
                _detection = s;
                detectionStrategyComponent = (MonoBehaviour)s;
                EnableStrategy();
                _detection.Initialize(this);
                return;
            }
        }
        var children = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] is IDetectionStrategy s2)
            {
                _detection = s2;
                detectionStrategyComponent = (MonoBehaviour)s2;
                EnableStrategy();
                _detection.Initialize(this);
                return;
            }
        }
    }

    void EnableStrategy()
    {
        var beh = detectionStrategyComponent as Behaviour;
        if (beh && !beh.enabled) beh.enabled = true;
    }

    public void ApplyLightStun(float customDuration)
    {
        if (!canBeStunnedByLight) return;
        float d = customDuration > 0f ? customDuration : stunSeconds;
        _isStunned = true;
        _stunEndTime = Time.time + d;
        if (_agent) { _agent.ResetPath(); if (freezeAgentOnStun) _agent.isStopped = true; }
        CurrentlyDetecting = false;
        _isChasing = false;
        if (animator && !string.IsNullOrEmpty(stunnedBool)) animator.SetBool(stunnedBool, true);
        if (stunSfx)
        {
            if (sfxSource) sfxSource.PlayOneShot(stunSfx, stunSfxVolume);
            else AudioSource.PlayClipAtPoint(stunSfx, transform.position, stunSfxVolume);
        }
    }

    void EndStun()
    {
        _isStunned = false;
        if (_agent) _agent.isStopped = false;
        if (animator && !string.IsNullOrEmpty(stunnedBool)) animator.SetBool(stunnedBool, false);
    }

    public void UsePatrolRoute(PatrolRoute route)
    {
        if (route == null) { patrolEnabled = false; patrolPoints = null; return; }
        patrolPoints = route.Points;
        patrolEnabled = patrolPoints != null && patrolPoints.Length > 0;
        _patrolIndex = 0;
    }

    public void SetPatrolPoints(Transform[] pts)
    {
        patrolPoints = pts;
        patrolEnabled = patrolPoints != null && patrolPoints.Length > 0;
        _patrolIndex = 0;
    }

    public void SuppressFor(float seconds)
    {
        _suppressUntilTime = Mathf.Max(_suppressUntilTime, Time.time + Mathf.Max(0f, seconds));
        _isChasing = false;
        CurrentlyDetecting = false;
        if (_agent) { _agent.ResetPath(); _agent.isStopped = false; }
        _detectionArmed = true;
        _lastNotDetectTime = Time.time;
    }

    public void WarpAwayFrom(Vector3 origin, float minDistance)
    {
        if (_agent == null) return;
        Vector3 dir = transform.position - origin;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        dir.Normalize();
        Vector3 targetPos = origin + dir * Mathf.Max(0.1f, minDistance);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, minDistance + 2f, NavMesh.AllAreas)) _agent.Warp(hit.position);
        else _agent.Warp(targetPos);
        _isChasing = false;
        CurrentlyDetecting = false;
        _agent.ResetPath();
        _detectionArmed = true;
        _lastNotDetectTime = Time.time;
    }

    public Transform Target => target;
    public NavMeshAgent Agent => _agent;
}
