using System.Linq;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    [SerializeField] bool useChildrenAsPoints = true;
    [SerializeField] Transform[] points;

    public Transform[] Points => points;

    void OnValidate()
    {
        if (useChildrenAsPoints)
            points = GetComponentsInChildren<Transform>(true)
                     .Where(t => t != transform).ToArray();
    }
}
