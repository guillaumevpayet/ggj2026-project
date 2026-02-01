using UnityEngine;

public class LightPingPongX : MonoBehaviour
{
    public float distance = 0.3f;
    public float speed = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * speed) * distance;
        transform.localPosition = startPos + new Vector3(xOffset, 0f, 0f);
    }
}