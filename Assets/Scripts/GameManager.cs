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

  public LevelManager levelManager { get; private set; }
  public int successfulScore = 0;
  public int missedScore = 0;
  public int performanceScore = 0;
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
    // Lerp the anchor position of the newspaper to the 0, 0
    var newspaper = GameObject.Find("Newspaper").GetComponent<RectTransform>();
    newspaper.anchoredPosition = Vector2.Lerp(newspaper.anchoredPosition, Vector2.zero, 0.1f);
  }

  public void ResetGame()
  {
    successfulScore = 0;
    missedScore = 0;
    performanceScore = 0;
    scoreText.text = successfulScore.ToString();
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
      timeline.scriptLength = lp.scriptLength;
      numberOfActors = lp.actorsCount;
      numberOfCards = lp.cardsCount;
    }
    else
    {
      Debug.LogError("No level manager found");
    }

    timeline.OnTimelineFinished += () =>
    {
      Debug.Log("Timeline finished");
      ShowGameOverPanel();
    };

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
    gameOverPanel.SetActive(false);
  }

  void ChangeScore(int scoreChange)
  {
    performanceScore = Mathf.Max(Mathf.Min(performanceScore + scoreChange, 3), -3);
    if (scoreChange > 0)
    {
      successfulScore += scoreChange;
      scoreText.text = successfulScore.ToString();
    }
    else
    {
      missedScore += scoreChange;
    }
    if (performanceScore <= -3)
    {
      Debug.Log("Game over!");
      ShowGameOverPanel();
    }
  }

  void ShowGameOverPanel()
  {
    timeline.Pause();
    gameOverPanel.SetActive(true);
    var resultLevel = getLevel();
    GameObject.Find("CriticTitle").GetComponent<TMPro.TMP_Text>().text = $"{getTitle(resultLevel)}";
    GameObject.Find("CriticReview").GetComponent<TMPro.TMP_Text>().text = $"{getCriticReview(resultLevel)}\n- {randomCriticName()}";
    GameObject.Find("Stats").GetComponent<TMPro.TMP_Text>().text = $"Correct hints: {successfulScore} / {successfulScore - missedScore} ({100.0f * successfulScore / (successfulScore - missedScore)}%)";
    GameObject.Find("Newspaper").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1000);
  }

  string randomCriticName() {
    var names = new List<string>();
    names.Add("The Drama Lama");
    names.Add("The Stage Sage");
    names.Add("The Bard Basher");
    names.Add("The Curtain Crusader");
    names.Add("The Performance Predator");
    names.Add("The Theater Terrorizer");
    names.Add("The Play Pundit");
    names.Add("The Comedy Censor");
    names.Add("The Tragic Takedown");
    names.Add("The Satirical Sniper");
    names.Add("The Musical Mauler");
    names.Add("The Shakespeare Shredder");
    names.Add("The Improv Inspector");
    names.Add("The Act Analysis Addict");
    names.Add("The Showdown Showoff");
    return names[Random.Range(0, names.Count)];
  }

  string getTitle(int score)
  {
    if (score == 0) return "Terrible";
    if (score == 1) return "Bad";
    if (score == 2) return "Poor";
    if (score == 3) return "Fair";
    if (score == 4) return "Good";
    if (score == 5) return "Great";
    return "Excellent";
  }

  string getCriticReview(int score)
  {
    var lines = new List<string>();
    lines.Add("The performance was worse than the Black Plague. Avoid at all costs!");
    lines.Add("The actors seemed like they would rather be jousting. Not a good show.");
    lines.Add("The stage design was the only thing that wasn't medieval about this performance. A complete miss.");
    lines.Add("The plot was more convoluted than the feudal system. A bit of a headache.");
    lines.Add("The performance had more humor than a court jester. A fun evening out.");
    lines.Add("The actors brought more passion than a crusade. A very enjoyable show.");
    lines.Add("The performance was better than the discovery of the printing press. A must-see!");
    return lines[score];
  }

  int getLevel()
  {
    if (performanceScore <= -3) return 0;
    var percentage = 1.0f * successfulScore / (successfulScore - missedScore);
    if (percentage < 0.4) return 0;
    if (percentage < 0.5) return 1;
    if (percentage < 0.6) return 2;
    if (percentage < 0.7) return 3;
    if (percentage < 0.8) return 4;
    if (percentage < 0.9) return 5;
    return 6;
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
      ChangeScore(1);
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
