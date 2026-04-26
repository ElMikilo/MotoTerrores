using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState { Patrolling, Chasing, Investigating, Stunned }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMonster : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrolling;

    [Header("Target")]
    [SerializeField] Transform target;
    [SerializeField] string playerTag = "Player";

    [Header("Patrulla")]
    [SerializeField] PatrolRoute patrolRoute;
    [SerializeField] float patrolSpeed = 2.5f;
    private int _currentPatrolIndex = 0;

    [Header("Movimiento")]
    [SerializeField] float stoppingDistance = 2f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float chaseSpeed = 4.5f;

    [Header("Animación")]
    [SerializeField] Animator animator;
    [SerializeField] string walkBool = "isWalking";
    [SerializeField] string stunnedBool = "isStunned";
    [SerializeField] string attackTrigger = "Attack";

    [Header("Detección de Visión")]
    [SerializeField] float visionRange = 18f;
    [SerializeField] float visionAngle = 110f;
    [SerializeField] LayerMask visionObstacles = ~0;
    [SerializeField] float visionCheckInterval = 0.15f;

    [Header("Detección de Sonido")]
    [SerializeField] float soundRange = 12f;
    [SerializeField] float soundThreshold = 0.15f;
    [SerializeField] float searchTimeAfterLost = 4f;

    [Header("Configuración de Ataque")]
    [SerializeField] float killDistance = 2f;
    [SerializeField] float attackCooldown = 2.5f;

    [Header("Aturdimiento por Luz")]
    [SerializeField] bool canBeStunnedByLight = true;
    [SerializeField] float stunSeconds = 3f;
    [SerializeField] AudioClip stunSfx;
    [SerializeField, Range(0f, 1f)] float stunSfxVolume = 1f;

    private NavMeshAgent _agent;
    private float _stunEndTime;
    private float _nextAttackTime;
    private Vector3 _investigationTarget;
    private bool _isPlayerDetected;
    private Coroutine _detectionCoroutine;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        
        GameObject playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (!target && playerGO) target = playerGO.transform;

        if (_agent)
        {
            _agent.stoppingDistance = stoppingDistance;
            _agent.autoBraking = true;
        }
    }

    void Start()
    {
        _detectionCoroutine = StartCoroutine(DetectionRoutine());
        ChangeState(EnemyState.Patrolling);
    }

    void OnEnable()
    {
        GameEvents.OnNoiseChanged += OnNoiseChanged;
    }

    void OnDisable()
    {
        GameEvents.OnNoiseChanged -= OnNoiseChanged;
        if (_detectionCoroutine != null) StopCoroutine(_detectionCoroutine);
    }

    private void OnNoiseChanged(float noiseLevel)
    {
        if (currentState == EnemyState.Stunned || _isPlayerDetected) return;

        if (noiseLevel > soundThreshold)
        {
            float distSq = (transform.position - target.position).sqrMagnitude;
            if (distSq <= soundRange * soundRange)
            {
                _investigationTarget = target.position;
                if (currentState != EnemyState.Chasing)
                {
                    Debug.Log("[Mikilo] Escuché un ruido sospechoso...");
                    ChangeState(EnemyState.Investigating);
                }
            }
        }
    }

    private IEnumerator DetectionRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(visionCheckInterval);
        while (true)
        {
            bool canSee = CanSeePlayer();
            
            if (canSee && !_isPlayerDetected)
            {
                Debug.Log("[Mikilo] ¡TE ENCONTRÉ!");
                ChangeState(EnemyState.Chasing);
            }
            
            _isPlayerDetected = canSee;

            if (!_isPlayerDetected && currentState == EnemyState.Chasing)
            {
                if (!_agent.pathPending && _agent.remainingDistance < 1f)
                {
                    yield return new WaitForSeconds(searchTimeAfterLost);
                    if (!_isPlayerDetected) 
                    {
                        Debug.Log("[Mikilo] Se me perdió el rastro... vuelvo a patrullar.");
                        ChangeState(EnemyState.Patrolling);
                    }
                }
            }

            yield return wait;
        }
    }

    void Update()
    {
        if (currentState == EnemyState.Stunned)
        {
            if (Time.time >= _stunEndTime) ChangeState(EnemyState.Patrolling);
            return;
        }

        if (!target || _agent == null || !_agent.isOnNavMesh) return;

        switch (currentState)
        {
            case EnemyState.Patrolling:
                UpdatePatrol();
                break;
            case EnemyState.Chasing:
                UpdateChase();
                break;
            case EnemyState.Investigating:
                UpdateInvestigate();
                break;
        }

        UpdateAnimation();
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;
        if (_agent == null || !_agent.isOnNavMesh) return;

        _agent.isStopped = false;
        
        switch (newState)
        {
            case EnemyState.Patrolling:
                _agent.speed = patrolSpeed;
                _agent.stoppingDistance = 0.5f;
                MoveToNextPatrolPoint();
                break;
            case EnemyState.Chasing:
                _agent.speed = chaseSpeed;
                _agent.stoppingDistance = stoppingDistance;
                break;
            case EnemyState.Investigating:
                _agent.speed = patrolSpeed;
                _agent.stoppingDistance = 0.5f;
                _agent.SetDestination(_investigationTarget);
                break;
            case EnemyState.Stunned:
                _agent.ResetPath();
                _agent.isStopped = true;
                if (animator) animator.SetBool(stunnedBool, true);
                break;
        }

        if (newState != EnemyState.Stunned && animator)
            animator.SetBool(stunnedBool, false);
    }

    private void UpdatePatrol()
    {
        if (patrolRoute == null || patrolRoute.Points.Length == 0) return;

        if (!_agent.pathPending && _agent.remainingDistance < 0.8f)
        {
            MoveToNextPatrolPoint();
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolRoute == null || patrolRoute.Points.Length == 0) return;
        _agent.SetDestination(patrolRoute.Points[_currentPatrolIndex].position);
        _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolRoute.Points.Length;
    }

    private void UpdateChase()
    {
        _agent.SetDestination(target.position);

        float distSq = (transform.position - target.position).sqrMagnitude;
        if (distSq < killDistance * killDistance)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < _nextAttackTime) return;

        _nextAttackTime = Time.time + attackCooldown;
        
        if (animator && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        if (LivesSystem.I != null)
        {
            Debug.Log("[Mikilo] ¡TE ATRAPÉ!");
            LivesSystem.I.LoseLife();
        }
    }

    private void UpdateInvestigate()
    {
        if (!_agent.pathPending && _agent.remainingDistance < 0.7f)
        {
            ChangeState(EnemyState.Patrolling);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null || string.IsNullOrEmpty(walkBool)) return;
        bool isMoving = _agent.velocity.sqrMagnitude > 0.1f;
        animator.SetBool(walkBool, isMoving);
    }

    bool CanSeePlayer()
    {
        if (target == null) return false;

        Vector3 from = transform.position + Vector3.up * 1.5f;
        Vector3 to = target.position + Vector3.up * 1f;
        Vector3 dir = (to - from).normalized;

        if ((to - from).sqrMagnitude > visionRange * visionRange) return false;

        if (Vector3.Angle(transform.forward, dir) > visionAngle * 0.5f) return false;

        if (Physics.Raycast(from, dir, out RaycastHit hit, visionRange, visionObstacles, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != target && !hit.transform.IsChildOf(target))
                return false;
        }

        return true;
    }

    public void ApplyLightStun(float customDuration)
    {
        if (!canBeStunnedByLight) return;
        _stunEndTime = Time.time + (customDuration > 0f ? customDuration : stunSeconds);
        ChangeState(EnemyState.Stunned);
        
        if (stunSfx)
            AudioSource.PlayClipAtPoint(stunSfx, transform.position, stunSfxVolume);
    }
}
