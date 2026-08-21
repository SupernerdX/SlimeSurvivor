using System.Collections.Generic;
using Unity.VisualScripting;
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
    private MeshDeformation meshDeformation;
    private Bounds spawnerBounds;
    private Vector3 spawnPosition;

    private List<Slot> slots = new();
    public SuspendedItemsSet suspendedItemsSet;
   
    private const int MaxAttempts = 30;
    private int batchCount = 3;
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

    private void CreateSlots(int batchCount)
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
                    Debug.Log("Local candidate Y: " + spawnPosition.y);
                    break;
                }
                    
            }
            if(foundValidPostion)
            {
                Vector3 worldSpawnPosition = transform.TransformPoint(spawnPosition);
                Debug.Log("World spawn Y: " + worldSpawnPosition.y);
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
        Instantiate(randoPrefab, targetObject.transform.position,Quaternion.identity, targetObject.transform);
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

    private void OnDrawGizmos()
    {
        if(spawnerBounds.size == Vector3.zero) return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(spawnerBounds.center, spawnerBounds.size);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
