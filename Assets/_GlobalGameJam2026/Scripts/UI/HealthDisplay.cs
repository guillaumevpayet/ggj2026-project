using System.Collections.Generic;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartContainer;

    private void Awake()
    {
        if (player != null)
        {
            player.OnHealthChanged += UpdateHearts;
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHearts;
        }
    }

    private void UpdateHearts(int currentHealth)
    {
        // Clear existing hearts
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new hearts
        for (int i = 0; i < currentHealth; i++)
        {
            Instantiate(heartPrefab, heartContainer);
        }
    }
}