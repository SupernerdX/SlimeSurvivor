using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(Health))] [RequireComponent(typeof(NavMeshAgent))]
public class BasicEnemy : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.0f;
    [SerializeField] private GameObject target;
    
    private NavMeshAgent _navMeshAgent;
    private Health _health;

    public NavMeshAgent NavMeshAgent => _navMeshAgent;
    public Health Health => _health;
    public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }

    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = _moveSpeed;
        _health = GetComponent<Health>();
    }
   
    void Start()
    {
        StartCoroutine(UpdateTargetPosition());
    }
    void OnEnable()
    {
        if(_health == null)
            return;
        _health.Died += HasDied;
    }
    
    void OnDisable()
    {
        if(_health == null)
            return;
        _health.Died -= HasDied;
    }
    
    private IEnumerator UpdateTargetPosition()
    {
        while (true)
        {
            if (target != null)
            {
                SetTargetPosition(target.transform.position);
            }
            yield return null;
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        var targetPosition = position;
        if(NavMeshAgent == null || !NavMeshAgent.enabled || !NavMeshAgent.isOnNavMesh)
            return;
        NavMeshAgent.SetDestination(targetPosition);
        
    }
    public void HasDied(Health health)
    {
        Destroy(gameObject);
    }
}
