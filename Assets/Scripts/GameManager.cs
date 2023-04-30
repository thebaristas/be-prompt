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
    public Transform cardsLeft;
    public Transform cardsRight;
    public bool cardSelected {get; set;}

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
        // Shuffle the spawn positions indices to prevent actors from spawning in the same position
        var randomIndices = getRandomIndices(actorsSpawnPositions.Length);

        for (int i = 0; i < numberOfActors; i++) {
            GameObject prefabToSpawn = actorsPrefabs[i];
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, actorsSpawnPositions[randomIndices[i]].position, Quaternion.identity);
        }

        float cardsSpacing = (cardsRight.position - cardsLeft.position).x / numberOfCards;
        float startX = -(numberOfCards / 2) * cardsSpacing;
        if (numberOfCards % 2 == 0)
        {
            startX += cardsSpacing / 2;
        }
        int order = 0;
        for (int i = 0; i < numberOfCards; i++) {
            int randomIndex = Random.Range(0, cardsPrefabs.Length);
            GameObject prefabToSpawn = cardsPrefabs[randomIndex];
            float xPos = startX + i * cardsSpacing;
            Vector3 spawnPos = new Vector3(xPos, cardsLeft.position.y, 0);
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            order = SetLayerRecursively(spawnedPrefab, "UI", order);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    int[] getRandomIndices(int count) {
        int[] indices = new int[count];
        for (int i = 0; i < count; i++) {
            indices[i] = i;
        }
        for (int i = 0; i < indices.Length; i++) {
            int temp = indices[i];
            int randomIndex = Random.Range(i, indices.Length);
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }
        return indices;
    }

    public static int SetLayerRecursively(GameObject obj, string layerName, int layerSortingOrder)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null) {
            renderer.sortingLayerName = layerName;
            renderer.sortingOrder = layerSortingOrder;
        }
        int depth = layerSortingOrder + 1;
        foreach (Transform child in obj.transform)
        {
            depth = SetLayerRecursively(child.gameObject, layerName, layerSortingOrder + 1);
        }
        return depth;
    }
}
