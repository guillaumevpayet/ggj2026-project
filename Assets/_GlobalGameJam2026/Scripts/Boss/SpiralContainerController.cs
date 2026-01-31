using UnityEngine;

namespace Boss
{
    public class SpiralContainerController : MonoBehaviour
    {
        // Showing up in Unity
        
        [SerializeField] private float rotationSpeed;
        
        // -------------------
        
        private void Update()
        {
            transform.Rotate(-Vector3.up * (rotationSpeed * Time.deltaTime));
        }
    }
}
