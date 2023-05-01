using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
  public int numberOfActors = 2;
  public int numberOfCards = 1;
  public Transform[] actorsSpawnPositions;
  public Transform cardsLeft;
  public Transform cardsRight;
  public int cardSelected { get; set; } = -1;

  public Timeline timeline;
  public TMPro.TMP_Text scoreText;
  public Color[] actorColors = {
    new Color32(0xDE, 0xE3, 0x8C, 0xFF),
    new Color32(0xFF, 0x84, 0xED, 0xFF),
    new Color32(0x83, 0xB0, 0xFF, 0xFF),
    new Color32(0x91, 0xFF, 0x83, 0xFF)
  };

  private static LevelManager levelManager;
  private int score = 0;
  Card cardPrefab;
  Sprite[] cardSprites;
  Character[] actorsPrefabs;
  public GameObject gameOverPanel;

  public Dictionary<string, Character> actors { get; private set; }

  void Awake()
  {
    actors = new Dictionary<string, Character>();
  }


  // Start is called before the first frame update
  void Start()
  {
    Debug.Log("GameManager started");
    actorsPrefabs = Resources.LoadAll<Character>(ResourcePaths.Actors);
    cardPrefab = Resources.Load<Card>(ResourcePaths.Card);
    cardSprites = Resources.LoadAll<Sprite>(ResourcePaths.CardSprites);
    ResetGame();
    var lc = GameObject.Find("LeftCurtain");
    lc.GetComponent<Animator>().Play("OpenCurtains");
    GameObject.Find("RightCurtain").GetComponent<Animator>().Play("OpenRightCurtain");
    // Trigger event when animation is finished
    lc.GetComponent<CurtainListener>().OnCurtainStateChanged += (state) =>
    {
      if (state == "opened")
      {
        // Start the script
        Debug.Log("Starting script...");
        timeline.Unpause();
      }
    };
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void ResetGame()
  {
    score = 0;
    scoreText.text = score.ToString();
    Debug.Log("Resetting game...");
    if (gameOverPanel == null)
    {
      gameOverPanel = GameObject.Find("GameOverPanel");
      if (gameOverPanel == null)
      {
        Debug.LogError("No GameOverPanel found");
      }
    }

    if (timeline == null)
    {
      Debug.Log("Timeline is null, getting it from scene: " + GameObject.Find("Timeline"));
      timeline = GameObject.Find("Timeline").GetComponent<Timeline>();
    }

    if (scoreText == null)
    {
      scoreText = GameObject.Find("Score").GetComponent<TMPro.TMP_Text>();
    }

    // Fetch levelmanager FindObjectOfType<LevelManager>()
    // give levelparameters to timeline
    // create actors/cards based on levelmanager
    levelManager = FindObjectOfType<LevelManager>();
    if (levelManager)
    {
      var lp = levelManager.levelParams;
      timeline.scriptSpeed = lp.scriptSpeed;
      timeline.missingProbability = lp.missingProbability;
      timeline.hintTimerDuration = lp.hintTimerDuration;
      numberOfActors = lp.actorsCount;
      numberOfCards = lp.cardsCount;
    }
    else
    {
      Debug.LogError("No level manager found");
    }

    timeline.OnObjectMissed += () =>
    {
      Debug.Log("Hint missed!");
      ChangeScore(-1);
    };


    // Shuffle the spawn positions indices to prevent actors from spawning in the same position
    var positionRandomIndices = getRandomIndices(actorsSpawnPositions.Length);
    var colorRandomIndices = getRandomIndices(actorColors.Length);

    Debug.Log("number of actors: " + numberOfActors);
    numberOfActors = Mathf.Min(numberOfActors, actorsSpawnPositions.Length);
    Debug.Log("number of actors after: " + numberOfActors);
    for (int i = 0; i < numberOfActors; i++)
    {
      Character prefabToSpawn = actorsPrefabs[Random.Range(0, actorsPrefabs.Length)];
      Character character = Instantiate(prefabToSpawn, actorsSpawnPositions[positionRandomIndices[i]].position, Quaternion.identity);
      string id = $"Character_{i}";
      character.name = id;
      if (character)
      {
        character.spriteRenderer.color = actorColors[colorRandomIndices[i]];
        actors.Add(id, character);
      }
      else
      {
        Debug.LogError($"No character component for {id}");
        continue;
      }
    }
    timeline.actorsIds = new List<string>(actors.Keys);



    numberOfCards = Mathf.Min(numberOfCards, cardSprites.Length);

    float cardsSpacing = (cardsRight.position - cardsLeft.position).x / numberOfCards;
    float startX = -(numberOfCards / 2) * cardsSpacing;
    if (numberOfCards % 2 == 0)
    {
      startX += cardsSpacing / 2;
    }

    int order = 0;
    timeline.cardIds = new List<string>();
    for (int i = 0; i < numberOfCards; i++)
    {
      timeline.cardIds.Add(cardSprites[i].name);
      Sprite cardSprite = cardSprites[i];

      float xPos = startX + i * cardsSpacing;
      Vector3 spawnPos = new Vector3(xPos, cardsLeft.position.y, 0);
      Card spawnedPrefab = Instantiate(cardPrefab, spawnPos, Quaternion.identity);

      spawnedPrefab.name = cardSprite.name;
      spawnedPrefab.drawingSpriteRenderer.sprite = cardSprite;
      spawnedPrefab.GetComponent<DragAndDrop>().OnDropEvent += OnCardDrop;
      order = SetLayerRecursively(spawnedPrefab.gameObject, "UI", order);
    }
    print("Timeline card count: " + timeline.cardIds.Count);

    // Start the timeline
    timeline.gameObject.SetActive(true);
    timeline.CreateScript();
  }

  void ChangeScore(int scoreChange)
  {
    score += scoreChange;
    scoreText.text = score.ToString();
    scoreText.GetComponent<Animator>().Play("ScorePop");
    if (score <= -3)
    {
      Debug.Log("Game over!");
      gameOverPanel.SetActive(true);
      timeline.Pause();
    }
  }

  void OnCardDrop(GameObject dropped, Character actor)
  {
    string cardName = dropped.name;
    string actorName = actor.name;
    Debug.Log($"Dropped card {cardName} on actor {actorName}");
    // Print expected hint card and actor
    Debug.Log($"Expected card: {timeline.GetWaitingHintCardId()}");
    Debug.Log($"Expected actor: {timeline.GetWaitingHintActorId()}");
    // Check if we need to drop a card on the timeline
    bool correct = timeline.GetWaitingHintCardId() == cardName &&
            timeline.GetWaitingHintActorId() == actorName;
    if (correct)
    {
      Debug.Log("Correct guess!");
      string resourcePath = $"{ResourcePaths.CardSprites}/{cardName}";
      Sprite sprite = Resources.Load<Sprite>(resourcePath);
      if (sprite)
      {
        actor.bubble.Display(sprite);
      }
      if (score < 3)
      {
        ChangeScore(1);
      }
    }
    else
    {
      Sprite sprite = Resources.Load<Sprite>(ResourcePaths.RedMarkSprite);
      if (sprite)
      {
        actor.bubble.Display(sprite);
      }
      Debug.Log("Wrong guess!");
      ChangeScore(-1);
    }
    actor.HandleCardDrop(correct, cardName);
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
