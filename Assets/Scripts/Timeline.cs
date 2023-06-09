using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Event delegate for when a card should be said
public delegate void OnCardDisplayDelegate(string actorId, string cardSpriteId, bool isMissing);
// Event delegate for when an object is missed
public delegate void OnObjectMissedDelegate();
// Event delegate for when the timeline is finished
public delegate void OnTimelineFinishedDelegate();

public class Timeline : MonoBehaviour
{

  // Event for when an card should be said
  public event OnCardDisplayDelegate OnCardDisplay;

  // Event for when an object is missed
  public event OnObjectMissedDelegate OnObjectMissed;
  // Event for when the timeline is finished
  public event OnTimelineFinishedDelegate OnTimelineFinished;


  public Script script = null;
  bool paused = false; // whether the timeline is paused
  bool waitingForHint = false; // whether the player should give a hint
  int hintIndex = 0; // the index of the script item that needs a hint
  float hintTimer = 0.0f; // in seconds
  float elapsedTime = 0.0f; // in seconds, corresponds to the right edge of the timeline
  float timelineWidth;
  float timelinePositionX;
  int firstVisibleIndex = 0;
  float firstVisibleIndexTime = 0.0f; // in seconds
  int lastVisibleIndex = 0;
  float lastVisibleIndexTime = 0.0f; // in seconds
  public float cardOffset = 0.5f;

  // Dictionary from element ID to game object, to track which game objects are currently visible.
  Dictionary<int, Card> elementToGameObject = new Dictionary<int, Card>();

  public GameManager gameManager;
  public float timelineTotalDuration = 5.0f; // in seconds
  public float hintTimerDuration = 3.0f; // in seconds
  // script speed
  public float scriptSpeed = 1.0f;
  public int playableCardsCount;
  public int scriptLength = 20;
  public float missingProbability = 0.5f; // probability of a card missing
  // start time
  public float initialDelay = 0.0f; // in seconds

  public List<string> actorsIds = new List<string>();
  public List<string> cardIds = new List<string>();


  public float bubbleHeadsup = 1.0f;
  private Card cardPrefab;

  // Start is called before the first frame update
  void Start()
  {
    // Get the timeline width and position
    var sprite = GetComponent<SpriteRenderer>();
    timelineWidth = sprite.bounds.size.x;
    timelinePositionX = sprite.bounds.center.x - timelineWidth / 2.0f;
    cardPrefab = Resources.Load<Card>(ResourcePaths.Card);
    if (gameManager == null) {
      gameManager = FindObjectOfType<GameManager>();
    }
  }

  public void CreateScript()
  {
    // Create a test script with test items
    script = new Script();
    script.items = new List<ScriptItem>();
    playableCardsCount = Mathf.Min(playableCardsCount, cardIds.Count);
    Debug.Log("Creating script with " + cardIds.Count + " cards and " + actorsIds.Count + " actors, with a missing probability of " + missingProbability + ".");
    for (int i = 0; i < scriptLength; ++i)
    {
      // Get a random isMissing value with a probability of .3 of being true
      var isMissing = Random.Range(0.0f, 1.0f) < missingProbability;
      // Choose a random index between 0 and playableCardsCount - 1 if the card is missing, or between playableCardsCount and cardIds.Count - 1 otherwise
      var cardId = cardIds[Random.Range(isMissing ? 0 : playableCardsCount, isMissing ? playableCardsCount : cardIds.Count)];
      // Choose a random actor ID
      var actorId = actorsIds[Random.Range(0, actorsIds.Count)];
      // Add a new script item
      script.items.Add(new ScriptItem(cardId, actorId, isMissing));
    }

    elapsedTime = -initialDelay;

    // Reset the fields
    paused = true; // whether the timeline is paused
    waitingForHint = false; // whether the player should give a hint
    hintIndex = 0; // the index of the script item that needs a hint
    hintTimer = 0.0f; // in seconds
    elapsedTime = 0.0f; // in seconds, corresponds to the right edge of the timeline
    firstVisibleIndex = 0;
    firstVisibleIndexTime = 0.0f; // in seconds
    lastVisibleIndex = 0;
    lastVisibleIndexTime = 0.0f; // in seconds
  }

  public void StartScript()
  {
    CreateScript();
    paused = false;
  }

  // Update is called once per frame
  void Update()
  {
    if (script == null)
    {
      return;
    }

    if (waitingForHint)
    {
      hintTimer -= Time.deltaTime;
      if (hintTimer <= 0.0f)
      {
        OnObjectMissed.Invoke();
        StopWaitingForHint();
      }
    }

    if (!paused && !waitingForHint)
    {
      // Update the elapsed time
      elapsedTime += scriptSpeed * Time.deltaTime;
    }
    // Update the first and last visible indices
    while (firstVisibleIndex < script.items.Count && firstVisibleIndexTime < elapsedTime - timelineTotalDuration)
    {
      firstVisibleIndexTime += script.items[firstVisibleIndex].delay;
      ++firstVisibleIndex;
      if (firstVisibleIndex >= script.items.Count)
      {
        OnTimelineFinished.Invoke();
      }
    }
    while (lastVisibleIndex < script.items.Count && lastVisibleIndexTime < elapsedTime)
    {
      lastVisibleIndexTime += script.items[lastVisibleIndex].delay;
      ++lastVisibleIndex;
    }

    // Update the visible game objects
    float elementTime = firstVisibleIndexTime;
    var gameObjectPrefab = Resources.Load<Card>(ResourcePaths.Card);
    for (int i = firstVisibleIndex; i < lastVisibleIndex; ++i)
    {
      var item = script.items[i];
      if (!elementToGameObject.ContainsKey(i))
      {
        // Create a new game object
        var sprite = Resources.Load<Sprite>($"{ResourcePaths.CardSprites}/{item.cardSpriteId}");
        var gameObject = Instantiate(gameObjectPrefab);
        var dnd = gameObject.GetComponent<DragAndDrop>();
        if (dnd != null) {
          Destroy(dnd);
        }

        Character actor = gameManager.actors[item.actorId];
        if (actor != null && actor.spriteRenderer != null && gameObject.drawingSpriteRenderer != null) {
          if (!item.isMissing) {
            gameObject.frameSpriteRenderer.color = Color.white;
          } else {
            gameObject.frameSpriteRenderer.color = actor.spriteRenderer.color;
          }
        }
        gameObject.drawingSpriteRenderer.sprite = sprite;
        // Set the name of the game object to the index and emoji code
        gameObject.name = i + "_" + item.cardSpriteId;
        // Make sr a child of this game object
        gameObject.transform.parent = transform;
        // Add the game object to the dictionary
        elementToGameObject[i] = gameObject;
        gameObject.transform.localScale /= 2.0f;
      }
      var obj = elementToGameObject[i];
      obj.transform.position = transform.position + new Vector3(GetTimelinePosition(elementTime), 0.0f, 0.0f) + new Vector3(0, cardOffset, 0);
      if (elementTime < elapsedTime - timelineTotalDuration / 2.0f + bubbleHeadsup && !item.displayed)
      {
        OnCardDisplay.Invoke(item.actorId, item.cardSpriteId, item.isMissing);
        item.displayed = true;
      }
      else if (!waitingForHint && item.isMissing && elementTime < elapsedTime - timelineTotalDuration / 2.0f)
      {
        hintIndex = i;
        waitingForHint = true;
        hintTimer = hintTimerDuration;
      }
      elementTime += item.delay;
    }

    // Destroy the game objects that are no longer visible
    var keysToRemove = new List<int>();
    foreach (var pair in elementToGameObject)
    {
      var i = pair.Key;
      if (i < firstVisibleIndex || i >= lastVisibleIndex)
      {
        keysToRemove.Add(i);
      }
    }

    foreach (var i in keysToRemove)
    {
      var spriteRenderer = elementToGameObject[i];
      var item = script.items[i];
      Destroy(spriteRenderer.gameObject);
      elementToGameObject.Remove(i);
    }
  }

  public void Pause()
  {
    paused = true;
  }

  public void Unpause()
  {
    paused = false;
  }

  public void StopWaitingForHint()
  {
    if (waitingForHint)
    {
      waitingForHint = false;
      script.items[hintIndex].isMissing = false;
    }
  }

  public string GetWaitingHintActorId()
  {
    if (!waitingForHint)
    {
      return "";
    }
    return script.items[hintIndex].actorId;
  }

  public string GetWaitingHintCardId()
  {
    if (!waitingForHint)
    {
      return "";
    }
    return script.items[hintIndex].cardSpriteId;
  }

  float GetTimelinePosition(float time)
  {
    // clamp time to 0 and timelineTotalDuration
    return timelinePositionX + timelineWidth * (1.0f - (-time + elapsedTime) / timelineTotalDuration);
  }
}
