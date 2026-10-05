using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerHealthDisplay : MonoBehaviour
{
    [SerializeField]  private Slider _healthSlider;
    [SerializeField] private Health _health;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _healthSlider.value = _health.CurrentHealth / _health.MaxHealth;
    }

    
}

