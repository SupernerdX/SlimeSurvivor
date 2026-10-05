using System;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Health : MonoBehaviour, IDamageable
{
    [SerializeField, Min(0.01f)] private float _maxHealth = 3.0f;

    private float _currentHealth;
    private int _damageImmunityRequests;
    private float _damageTakenMultiplier = 1.0f;
    private float _maxHealthMultiplier = 1f;

    public float MaxHealth => _maxHealth * _maxHealthMultiplier;
    public float CurrentHealth => _currentHealth;
    public bool IsAlive => _currentHealth > 0.0f;
    public bool IsDamageImmune => _damageImmunityRequests > 0;
    public float DamageTakenMultiplier => _damageTakenMultiplier;

    public event Func<float, float> DamageAdjusted;
    public event Action<Health> Died;
    public event Action<Health> HealthChanged;
    public event Func<Health, float, bool> DamageIntercepted;
    

    private void Awake()
    {
        RestoreToFull();
    }

    private void OnValidate()
    {
        _maxHealth = Mathf.Max(0.01f, _maxHealth);
        if (Application.isPlaying)
            _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
        
    }
    public bool HasTakenDamage(float damage, Health source = null)
    {
        if(damage <= 0.0f || !IsAlive || IsDamageImmune)
            return false;
        if(DamageIntercepted != null)
        {
            Delegate[] interceptors = DamageIntercepted.GetInvocationList();
            for(int index = 0; index < interceptors.Length; index++)
            {
                if(((Func<Health, float, bool>)interceptors[index])(source, damage))
                    return false;
            }
        }
        float appliedDamage = damage * _damageTakenMultiplier;
        if(appliedDamage <= 0.0f)
            return false;
        
        if(DamageAdjusted != null)
            foreach(Func<float, float> adjust in DamageAdjusted.GetInvocationList()) 
                appliedDamage = adjust(appliedDamage);
        if(appliedDamage <= 0f) 
            return false;
        _currentHealth = Mathf.Max(0.0f, _currentHealth - appliedDamage);
        HealthChanged?.Invoke(this);

        if(!IsAlive)
            Died?.Invoke(this);

        return true;
    }

    void IDamageable.TakeDamage(float damage)
    {
        HasTakenDamage(damage, this);
    }

    public void SetDamageTakenMultiplier(float multiplier)
    {
        _damageTakenMultiplier =  Mathf.Max(0.0f, multiplier);
    }

    public void AquireDamageImmunity()
    {
        _damageImmunityRequests++;
    }

    public void ReleaseDamageImmunity()
    {
        _damageImmunityRequests = Mathf.Max(0, _damageImmunityRequests - 1);
    }

    public void RestoreToFull()
    {
        _currentHealth = MaxHealth;
        HealthChanged?.Invoke(this);
    }

    public void SetMaxHealthMultiplier(float multiplier)
    {
        float ratio = MaxHealth > 0f ? _currentHealth / MaxHealth : 1f;
        _maxHealthMultiplier = Mathf.Max(0.01f, multiplier);
        _currentHealth = Mathf.Clamp01(ratio) * MaxHealth;
        HealthChanged?.Invoke(this);
    }

    public void Heal(float amount)
    {
        if(amount <= 0f || !IsAlive || _currentHealth >= MaxHealth) return;
        _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
        HealthChanged?.Invoke(this);
    }
}

[CustomEditor (typeof(Health))]
public class HealthDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Health playerHealthDisplay = (Health)target;
        if(GUILayout.Button("Take Damage"))
        {
            playerHealthDisplay.HasTakenDamage(0.1f);
        }
    }
}
