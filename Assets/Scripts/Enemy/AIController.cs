using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class AIController : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;
    public float speed = 3f;
    public float detectionRange = 10f;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
        /*
        Patrol();
    */
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }


    bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.position) < detectionRange;
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * speed * Time.fixedDeltaTime);
    }

    // 3. Visualización en el editor (4:54)
    void OnDrawGizmos()
    {

        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.red;

            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawSphere(waypoint.position, 0.3f);
                }
            }


            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {

                if (waypoints[i] != null && waypoints[(i + 1) % waypoints.Length] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[(i + 1) % waypoints.Length].position);
                }
            }
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}