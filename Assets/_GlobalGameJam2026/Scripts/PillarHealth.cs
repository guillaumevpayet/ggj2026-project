using System.Collections.Generic;
using UnityEngine;

public class PillarHealth : MonoBehaviour
{
    // Showing up in Unity
    
    // -------------------
    
    private readonly Stack<GameObject> _parts = new();
    
    /// <summary>
    /// Applies damage to the pillar by removing the topmost part of its structure.
    /// </summary>
    /// <returns>True if all parts of the pillar have been destroyed, indicating the pillar is completely gone. False otherwise.</returns>
    public bool TakeDamage()
    {
        Destroy(_parts.Pop());
        return _parts.Count == 0;
    }

    private void Awake()
    {
        var children = GetComponentsInChildren<Transform>();
        
        foreach (var child in children)
        {
            if (child == transform)
            {
                continue;
            }
            
            _parts.Push(child.gameObject);
        }
    }
}
