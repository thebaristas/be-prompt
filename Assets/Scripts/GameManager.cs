using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
// 44 38 2b

public class GameManager : MonoBehaviour
{
  public int numberOfActors = 2;
  public int numberOfCards = 8;
  public Transform[] actorsSpawnPositions;
  public Transform cardsLeft;
  public Transform cardsRight;
  public int cardSelected { get; set; } = -1;

  public Timeline timeline;
  public TMPro.TMP_Text scoreText;

  // Static reference to the instance of the singleton class
  private static GameManager instance;
  private int score = 0;

  Dictionary<string, GameObject> actorsPrefabs;
  Dictionary<string, GameObject> cardsPrefabs;

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
    timeline.OnObjectMissed += () =>
    {
      Debug.Log("Hint missed!");
      ChangeScore(-1);
    };
    actorsPrefabs = new Dictionary<string, GameObject>();
    // Get the contents of the Resources/Actors folder
    foreach (var file in new DirectoryInfo("Assets/Prefabs/Resources/Actors").GetFiles("*.prefab"))
    {
      // Load the actor prefab
      var actorPrefab = Resources.Load<GameObject>("Actors/" + file.Name.Replace(".prefab", ""));
      // Add the actor prefab to the list of actors prefabs
      actorsPrefabs.Add(file.Name.Replace(".prefab", ""), actorPrefab);
    }

    // Same for the cards
    cardsPrefabs = new Dictionary<string, GameObject>();
    foreach (var file in new DirectoryInfo("Assets/Prefabs/Resources/Cards").GetFiles("*.prefab"))
    {
      var cardPrefab = Resources.Load<GameObject>("Cards/" + file.Name.Replace(".prefab", ""));
      cardsPrefabs.Add(file.Name.Replace(".prefab", ""), cardPrefab);
    }

    // Shuffle the spawn positions indices to prevent actors from spawning in the same position
    var randomIndices = getRandomIndices(actorsSpawnPositions.Length);

    for (int i = 0; i < numberOfActors; i++)
    {
      GameObject prefabToSpawn = actorsPrefabs["Character"];
      GameObject spawnedPrefab = Instantiate(prefabToSpawn, actorsSpawnPositions[randomIndices[i]].position, Quaternion.identity);
      string id = $"Character_{i}";
      spawnedPrefab.name = id;
      Character character = spawnedPrefab.GetComponent<Character>();
      if (character) {
        character.id = id;
      } else {
        Debug.LogError($"No character component for {id}");
      }
    }

    float cardsSpacing = (cardsRight.position - cardsLeft.position).x / numberOfCards;
    float startX = -(numberOfCards / 2) * cardsSpacing;
    if (numberOfCards % 2 == 0)
    {
      startX += cardsSpacing / 2;
    }
    int order = 0;
    for (int i = 0; i < numberOfCards; i++)
    {
      // Get a random prefab from the cards prefabs
      GameObject prefabToSpawn = cardsPrefabs["Card"];
      float xPos = startX + i * cardsSpacing;
      Vector3 spawnPos = new Vector3(xPos, cardsLeft.position.y, 0);
      GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
      spawnedPrefab.name = $"Card_{i}";
      spawnedPrefab.GetComponent<DragAndDrop>().OnDropEvent += OnCardDrop;
      order = SetLayerRecursively(spawnedPrefab, "UI", order);
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  void ChangeScore(int scoreChange)
  {
    score += scoreChange;
    scoreText.text = score.ToString();
    scoreText.GetComponent<Animator>().Play("ScorePop");
    if (score == -3)
    {
      Debug.Log("Game over!");
      timeline.Pause();
    }
  }

  void OnCardDrop(string cardName, string actorName)
  {
    Debug.Log($"Dropped card {cardName} on actor {actorName}");
    // Print expected hint card and actor
    Debug.Log($"Expected card: {timeline.GetWaitingHintCardId()}");
    Debug.Log($"Expected actor: {timeline.GetWaitingHintActorId()}");
    // Check if we need to drop a card on the timeline
    if (timeline.GetWaitingHintCardId() == cardName &&
            timeline.GetWaitingHintActorId() == actorName)
    {
      Debug.Log("Correct guess!");
      if (score < 3)
      {
        ChangeScore(1);
      }
    }
    else
    {
      Debug.Log("Wrong guess!");
      ChangeScore(-1);
    }
    timeline.StopWaitingForHint();
  }

  int[] getRandomIndices(int count)
  {
    int[] indices = new int[count];
    for (int i = 0; i < count; i++)
    {
      indices[i] = i;
    }
    for (int i = 0; i < indices.Length; i++)
    {
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
    if (renderer != null)
    {
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

  public void SubscribeToCardDisplayEvents(OnCardDisplayDelegate callback)
  {
    timeline.OnCardDisplay += callback;
  }
}
