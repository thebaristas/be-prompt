using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Event delegate for when a card should be said
public delegate void OnCardDisplayDelegate(string actorId, string cardSpriteId, bool isMissing);
// Event delegate for when an object is missed
public delegate void OnObjectMissedDelegate();

public class Timeline : MonoBehaviour
{

  // Event for when an card should be said
  public event OnCardDisplayDelegate OnCardDisplay;

  // Event for when an object is missed
  public event OnObjectMissedDelegate OnObjectMissed;


  public Script script;
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

  // Dictionary from element ID to game object, to track which game objects are currently visible.
  Dictionary<int, Card> elementToGameObject = new Dictionary<int, Card>();

  public float timelineTotalDuration = 5.0f; // in seconds
  public float hintTimerDuration = 3.0f; // in seconds
  // script speed
  public float scriptSpeed = 1.0f;
  // start time
  public float initialDelay = 0.0f; // in seconds

  // TODO: Extract list of emoji codes and player IDs from the scrip
  public string[] cardIds = { "0", "1", "2" };

  // Start is called before the first frame update
  void Start()
  {
    // Create a list with all the emoji codes
    // Create a list with all actors Ids
    var actorsIds = new List<string>();
    actorsIds.Add("Character_0");
    actorsIds.Add("Character_1");
    actorsIds.Add("Character_2");
    // Create a test script with test items
    script = new Script();
    script.items = new List<ScriptItem>();
    script.items.Add(new ScriptItem("1", "Character_1"));
    script.items.Add(new ScriptItem("2", "Character_2", true));
    script.items.Add(new ScriptItem("0", "Character_0", false, .5f));
    script.items.Add(new ScriptItem("1", "Character_1"));
    script.items.Add(new ScriptItem("2", "Character_2"));
    for (int i = 0; i < 100; ++i)
    {
      // Choose a random emoji code and player ID
      var cardId = cardIds[Random.Range(0, cardIds.Length)];
      var actorId = actorsIds[Random.Range(0, actorsIds.Count)];
      // Get a random isMissing value with a probability of .3 of being true
      var isMissing = Random.Range(0.0f, 1.0f) < .2f;
      // Add a new script item
      script.items.Add(new ScriptItem(cardId, actorId, isMissing));
    }

    elapsedTime = -initialDelay;
    // Get the timeline width and position
    var sprite = GetComponent<SpriteRenderer>();
    timelineWidth = sprite.bounds.size.x;
    timelinePositionX = sprite.bounds.center.x - timelineWidth / 2.0f;
  }

  // Update is called once per frame
  void Update()
  {
    if (waitingForHint)
    {
      hintTimer -= Time.deltaTime;
      if (hintTimer <= 0.0f)
      {
        OnObjectMissed.Invoke();
        StopWaitingForHint();
      }
    }

    if (!paused)
    {
      // Update the elapsed time
      elapsedTime += scriptSpeed * Time.deltaTime;
    }
    // Update the first and last visible indices
    while (firstVisibleIndex < script.items.Count && firstVisibleIndexTime < elapsedTime - timelineTotalDuration)
    {
      firstVisibleIndexTime += script.items[firstVisibleIndex].delay;
      ++firstVisibleIndex;
    }
    while (lastVisibleIndex < script.items.Count && lastVisibleIndexTime < elapsedTime)
    {
      lastVisibleIndexTime += script.items[lastVisibleIndex].delay;
      ++lastVisibleIndex;
    }

    // Update the visible game objects
    float elementTime = firstVisibleIndexTime;
    for (int i = firstVisibleIndex; i < lastVisibleIndex; ++i)
    {
      var item = script.items[i];
      if (!elementToGameObject.ContainsKey(i))
      {
        // Create a new game object
        var sprite = Resources.Load<Sprite>($"{ResourcePaths.CardSprites}/{item.cardSpriteId}");
        var gameObjectPrefab = Resources.Load<Card>(ResourcePaths.Card);
        var gameObject = Instantiate(gameObjectPrefab);
        gameObject.spriteRenderer.sprite = sprite;
        // Set the name of the game object to the index and emoji code
        gameObject.name = i + "_" + item.cardSpriteId;
        // Make sr a child of this game object
        gameObject.transform.parent = transform;
        // Add the game object to the dictionary
        elementToGameObject[i] = gameObject;
        gameObject.transform.localScale /= 2.0f;
      }
      var obj = elementToGameObject[i];
      obj.transform.position = transform.position + new Vector3(GetTimelinePosition(elementTime), 0.0f, 0.0f);
      if (elementTime < elapsedTime - timelineTotalDuration / 2.0f)
      {
        if (!item.displayed) {
          OnCardDisplay.Invoke(item.actorId, item.cardSpriteId, item.isMissing);
          item.displayed = true;
        }
        if (!waitingForHint && item.isMissing) {
          paused = true;
          hintIndex = i;
          waitingForHint = true;
          hintTimer = hintTimerDuration;
        }
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
      paused = false;
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
