using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class SuspendedItemSpawner : MonoBehaviour
{
    private class Slot
    {   
    public GameObject slotObject;
    
    public bool isFilled;

    public Slot(GameObject slotObject, bool isFilled)
        {
            this.slotObject = slotObject;
            this.isFilled = isFilled;
        }
    }

    [SerializeField]private float safetyMargin = 0.8f; 
    
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private float minSlotDistance = 0.2f;

    [SerializeField] private Vector2 minMaxStiffnessSlider = new Vector2(50f, 150f);
    [SerializeField] private Vector2 minMaxDampingSlider = new Vector2(0.2f, 0.5f);
    [SerializeField] private int batchCount = 3;
    private MeshDeformation meshDeformation;
    private Bounds spawnerBounds;
    private Vector3 spawnPosition;

    private List<Slot> slots = new();
    public SuspendedItemsSet suspendedItemsSet;
   
    private const int MaxAttempts = 30;
    
    private GameObject slotObject;
   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshDeformation = GetComponent<MeshDeformation>();
        spawnerBounds = CollapseBounds(safetyMargin);
        CreateSlots(batchCount);
    }

    private Bounds CollapseBounds(float safetyMargin)
    {
        Bounds rawBounds = meshDeformation.GetBounds();
        Bounds spawnBounds = new Bounds(rawBounds.center, rawBounds.size * safetyMargin);
        return spawnBounds;
    }

    public void CreateSlots(int batchCount)
    {
        for(int i = 0; i < batchCount; i++)
        {
            bool foundValidPostion = false;

            for (int j = 0; j < MaxAttempts; j++)
            {
                spawnPosition = new Vector3((
                    Random.Range(spawnerBounds.min.x, spawnerBounds.max.x)), 
                    Random.Range(spawnerBounds.min.y, spawnerBounds.max.y),
                    Random.Range(spawnerBounds.min.z, spawnerBounds.max.z));
                if(CheckIsValidPosition(spawnPosition))
                {
                    foundValidPostion = true;
                    break;
                }
                    
            }
            if(foundValidPostion)
            {
                Vector3 worldSpawnPosition = transform.TransformPoint(spawnPosition);
                slotObject = Instantiate(slotPrefab, worldSpawnPosition, Quaternion.identity, transform);
                Slot newSlot = new Slot(slotObject, false);
                slots.Add(newSlot);  
                AttachObjectTo(slotObject);   
            }
             
        }
    }

    private void AttachObjectTo(GameObject targetObject)
    {
        GameObject randoPrefab = suspendedItemsSet.itemPrefabs[Random.Range(0, suspendedItemsSet.itemPrefabs.Length)];
        Instantiate(randoPrefab, targetObject.transform.position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), targetObject.transform);

        SuspendBone bone = targetObject.GetComponent<SuspendBone>();
        if (bone != null)
        {
            bone.OnModelAttached();
        }
        SpringFollow springFollow = targetObject.GetComponent<SpringFollow>();
        if (springFollow != null)
        {
            springFollow.RandomizeDragParameters(
                minMaxStiffnessSlider.x, minMaxStiffnessSlider.y, 
                minMaxDampingSlider.x, minMaxDampingSlider.y);
        }
    }

    private bool CheckIsValidPosition(Vector3 candidate)
    { 
        Vector3 worldcanidatePostion = transform.TransformPoint(candidate);
        foreach(var slot in slots)
        {
            if(slot.slotObject != null 
            && Vector3.Distance(worldcanidatePostion, slot.slotObject.transform.position) < minSlotDistance)
            {
                return false;
            }
        }

        return true;
    }

    public List<SuspendBone> GetSuspendedBones()
    {
        List<SuspendBone> bones = new List<SuspendBone>();

        foreach(var slot in slots)
        {
            if(slot.slotObject == null) continue;
            SuspendBone bone = slot.slotObject.GetComponent<SuspendBone>();
            if(bone != null)
            {
                bones.Add(bone);
            }
        }
        return bones;
    }

    public Bounds GetSpawnerBounds() => spawnerBounds;

    public void ResetSpawner()
    {
        foreach(var slot in slots)
        {
            if(slot.slotObject != null)
            {
                Destroy(slot.slotObject);
            }
        }
        slots.Clear();
        spawnerBounds = CollapseBounds(safetyMargin);
        CreateSlots(batchCount);
    }

    private void OnDrawGizmos()
    {
        if(spawnerBounds.size == Vector3.zero) return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(spawnerBounds.center, spawnerBounds.size);
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(SuspendedItemSpawner))]

public class SuspendedItemSpawnerEditor : Editor
{
     public override void OnInspectorGUI()
     {
        DrawPropertiesExcluding(serializedObject, "minMaxStiffnessSlider", "minMaxDampingSlider");
        serializedObject.Update();

        SerializedProperty stiffnessProp = serializedObject.FindProperty("minMaxStiffnessSlider");
        Vector2 stiffnessRange = stiffnessProp.vector2Value; 

        float stiffMinValue = stiffnessRange.x;
        float stiffMaxValue = stiffnessRange.y; 
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stiffness Range", GUILayout.Width(120));
        stiffMinValue = EditorGUILayout.FloatField(stiffMinValue, GUILayout.Width(40));
        EditorGUILayout.MinMaxSlider(ref stiffMinValue, ref stiffMaxValue, 0f, 200f);
        stiffMaxValue = EditorGUILayout.FloatField(stiffMaxValue, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();
        stiffnessProp.vector2Value = new Vector2(stiffMinValue, stiffMaxValue);

        SerializedProperty DampeningProp = serializedObject.FindProperty("minMaxDampingSlider");
        Vector2 dampeningRange = DampeningProp.vector2Value; 

        float dampMinValue = dampeningRange.x;
        float dampMaxValue = dampeningRange.y; 
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dampness Range", GUILayout.Width(120));
        dampMinValue = EditorGUILayout.FloatField(dampMinValue, GUILayout.Width(40));
        EditorGUILayout.MinMaxSlider(ref dampMinValue, ref dampMaxValue, 0f, 1f);
        dampMaxValue = EditorGUILayout.FloatField(dampMaxValue, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();
        DampeningProp.vector2Value = new Vector2(dampMinValue, dampMaxValue);

        serializedObject.ApplyModifiedProperties();
    }

}

#endif