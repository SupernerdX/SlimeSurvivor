using UnityEngine;
using UnityEngine.AI;

public class BasicEnemy : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.0f;
    
    private NavMeshAgent _navMeshAgent; 

    public NavMeshAgent NavMeshAgent => _navMeshAgent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
