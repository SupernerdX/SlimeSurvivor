using UnityEngine;
using System.Collections;
using UnityEngine.AI;
public class SlimeTrail : MonoBehaviour
{
    [SerializeField] private LayerMask _detectableLayers;
    [SerializeField] private float _slowdownFactor = 0.5f;
    [SerializeField] private float lifeTime = 1f;


    private float originalSpeed;
    private Material _material;
    void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }
    void Start()
    {
        StartCoroutine(Disapate(lifeTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        other.TryGetComponent(out NavMeshAgent enemy);
        if (enemy != null && enemy.speed > 0)
        {
            originalSpeed = enemy.speed;
            enemy.speed *= _slowdownFactor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        other.TryGetComponent(out NavMeshAgent enemy);
        if (enemy != null && enemy.speed > 0)
        {
            enemy.speed = originalSpeed;
        }
    }

    public IEnumerator Disapate(float duration)
    {
        yield return new WaitForSeconds(duration);
        float elapsedTime = 0f;
        float initialAlpha = _material.color.a;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(initialAlpha, 0f, elapsedTime / duration);
            _material.color = new Color(_material.color.r, _material.color.g, _material.color.b, alpha);
            yield return null;
        }
        _material.color = new Color(_material.color.r, _material.color.g, _material.color.b, 0f);
        Destroy(gameObject);
    }


}
