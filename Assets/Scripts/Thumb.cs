using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thumb : MonoBehaviour
{
    private GameManager gameManager;
    public float angleIncrement = 30f; // the amount to rotate by each time
    public float defaultAngle = -90f;

    // Start is called before the first frame update
    void Start()
    {
        if (gameManager == null) {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager != null) {
            // calculate the target rotation based on the score
            float targetRotation = gameManager.performanceScore * angleIncrement;
            // rotate the object towards the target rotation with a Lerp
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, defaultAngle + targetRotation), Time.deltaTime * 5f);
        }
    }
}
