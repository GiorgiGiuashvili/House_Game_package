using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    public float speed = 1f; 
    public float resetPositionX = -10f; 
    public float startPositionX = 10f; 

    private Vector2 startPosition;

    void Start()
    {
        startPosition = new Vector2(startPositionX, transform.position.y);
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        if (transform.position.x < resetPositionX)
        {
            transform.position = startPosition;
        }
    }
}
