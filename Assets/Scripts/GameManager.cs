using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int numberOfActors = 2;
    public int numberOfCards = 8;
    public GameObject[] actorsPrefabs;
    public GameObject[] cardsPrefabs;
    public Transform[] actorsSpawnPositions;
    public float spacing;

    // Static reference to the instance of the singleton class
    private static GameManager instance;

    // Public getter for the singleton instance
    public static GameManager Instance
    {
        get
        {
            // If the instance hasn't been set yet, try to find it in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                // If the instance still hasn't been set, log an error message
                if (instance == null)
                {
                    Debug.LogError("No instance of GameManager found in the scene!");
                }
            }

            return instance;
        }
    }

    // Private constructor to prevent direct instantiation of the class
    private GameManager() { }

    void Awake()
    {
        // If there is already an instance of the class, destroy the new one
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, set the instance to this object
        instance = this;

        // Make the game object persist across scenes
        DontDestroyOnLoad(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfActors; i++) {
            GameObject prefabToSpawn = actorsPrefabs[i];
            int randomIndex = Random.Range(0, actorsSpawnPositions.Length);
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, actorsSpawnPositions[randomIndex].position, Quaternion.identity);
        }

        float startX = -(numberOfCards / 2) * spacing;
        if (numberOfCards % 2 == 0)
        {
            startX += spacing / 2;
        }
        for (int i = 0; i < numberOfCards; i++) {
            int randomIndex = Random.Range(0, cardsPrefabs.Length);
            GameObject prefabToSpawn = cardsPrefabs[randomIndex];
            float xPos = startX + i * spacing;
            Vector3 spawnPos = new Vector3(xPos, 0, 0);
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}