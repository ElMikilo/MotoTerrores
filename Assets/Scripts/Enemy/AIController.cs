using UnityEngine;
using UnityEngine.Events;

public class AIController : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;
    public float speed = 3f;
    public float detectionRange = 10f;

    [Header("Player Animation")]
    public Animator playerAnimator;

    [Header("Events")]
    public UnityEvent onChaseStart;
    public UnityEvent onChaseEnd;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private Animator animator;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsChasing = Animator.StringToHash("isChasing");
    private static readonly int PlayerIsScared = Animator.StringToHash("IsScared");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private bool wasChasing = false;

    void FixedUpdate()
    {
        bool chasing = PlayerInRange();

        if (chasing && !wasChasing)
            onChaseStart?.Invoke();
        else if (!chasing && wasChasing)
            onChaseEnd?.Invoke();

        wasChasing = chasing;

        if (chasing)
            ChasePlayer();
        else
            Patrol();

        if (animator != null)
        {
            animator.SetBool(IsWalking, !chasing);
            animator.SetBool(IsChasing, chasing);
        }

        if (playerAnimator != null)
            playerAnimator.SetBool(PlayerIsScared, chasing);
    }

    bool PlayerInRange()
    {
        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatPlayer = new Vector3(player.position.x, 0, player.position.z);
        return Vector3.Distance(flatPos, flatPlayer) < detectionRange;
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        Vector3 lookTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(lookTarget);

        transform.Translate(direction * speed * Time.fixedDeltaTime, Space.World);

        Vector3 flatPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(target.position.x, 0, target.position.z);
        if (Vector3.Distance(flatPos, flatTarget) < 0.3f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookTarget);

        transform.Translate(direction * speed * Time.fixedDeltaTime, Space.World);
    }

    void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                    Gizmos.DrawSphere(waypoint.position, 0.3f);
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null && waypoints[(i + 1) % waypoints.Length] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[(i + 1) % waypoints.Length].position);
            }
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
