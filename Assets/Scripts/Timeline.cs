using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline : MonoBehaviour
{
  // Event delegate for when an object is missed
  public delegate void OnObjectMissedDelegate();
  // Event delegate for when an object is guessed correctly
  public delegate void OnObjectGuessedDelegate();
  // Event delegate for when an object is guessed incorrectly
  public delegate void OnObjectGuessedIncorrectlyDelegate();

  // Event for when an object is missed
  public event OnObjectMissedDelegate OnObjectMissed;
  // Event for when an object is guessed correctly
  public event OnObjectGuessedDelegate OnObjectGuessed;
  // Event for when an object is guessed incorrectly
  public event OnObjectGuessedIncorrectlyDelegate OnObjectGuessedIncorrectly;


  Script script;
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
  Dictionary<int, SpriteRenderer> elementToGameObject = new Dictionary<int, SpriteRenderer>();

  public float timelineTotalDuration = 5.0f; // in seconds
  public float hintTimerDuration = 3.0f; // in seconds
  // script speed
  public float scriptSpeed = 1.0f;
  // start time
  public float initialDelay = 0.0f; // in seconds

  // Start is called before the first frame update
  void Start()
  {
    // Create a list with all the emoji codes
    var emojiCodes = new List<string>();
    emojiCodes.Add("smile");
    emojiCodes.Add("heart");
    emojiCodes.Add("cup");
    // Create a list with all player IDs
    var playerIDs = new List<string>();
    playerIDs.Add("player1");
    playerIDs.Add("player2");
    playerIDs.Add("player3");
    // Create a test script with test items
    script = new Script();
    script.items = new List<ScriptItem>();
    script.items.Add(new ScriptItem("smile", "player1"));
    script.items.Add(new ScriptItem("heart", "player2", true));
    script.items.Add(new ScriptItem("cup", "player1", false, .5f));
    script.items.Add(new ScriptItem("heart", "player2"));
    script.items.Add(new ScriptItem("cup", "player3"));
    for (int i = 0; i < 100; ++i)
    {
      // Choose a random emoji code and player ID
      var emojiCode = emojiCodes[Random.Range(0, emojiCodes.Count)];
      var playerID = playerIDs[Random.Range(0, playerIDs.Count)];
      // Get a random isMissing value with a probability of .3 of being true
      var isMissing = Random.Range(0.0f, 1.0f) < .2f;
      // Add a new script item
      script.items.Add(new ScriptItem(emojiCode, playerID, isMissing));
    }

    elapsedTime = -initialDelay;
    // Get the timeline width and position
    var sprite = GetComponent<SpriteRenderer>();
    timelineWidth = sprite.bounds.size.x;
    timelinePositionX = sprite.bounds.center.x - timelineWidth / 2.0f;
    OnObjectGuessed += () =>
    {
      Debug.Log("Object guessed");
    };
    OnObjectMissed += () =>
    {
      Debug.Log("Object missed");
    };
  }

  // Update is called once per frame
  void Update()
  {
    // Space bar pressed
    if (Input.GetKeyDown(KeyCode.Space))
    {
      // Toggle the paused state
      if (waitingForHint)
      {
        OnObjectGuessed.Invoke();
        paused = false;
        waitingForHint = false;
        script.items[hintIndex].isMissing = false;
      }
    }

    if (waitingForHint)
    {
      hintTimer -= Time.deltaTime;
      if (hintTimer <= 0.0f)
      {
        OnObjectMissed.Invoke();
        paused = false;
        waitingForHint = false;
        script.items[hintIndex].isMissing = false;
      }
    }

    if (!paused)
    {
      // Update the elapsed time
      elapsedTime += Time.deltaTime;
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
        var sprite = Resources.Load<Sprite>("emojis/" + item.emojiCode);
        var gameObject = new GameObject();
        // Set the name of the game object to the index and emoji code
        gameObject.name = i + "_" + item.emojiCode;
        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        // Make sr a child of this game object
        sr.transform.parent = transform;
        // Add the game object to the dictionary
        elementToGameObject[i] = sr;
      }
      var spriteRenderer = elementToGameObject[i];
      // Make missing items red
      if (item.isMissing)
      {
        spriteRenderer.color = Color.red;
      }
      else
      {
        spriteRenderer.color = Color.white;
      }
      spriteRenderer.transform.position = transform.position + new Vector3(GetTimelinePosition(elementTime), 0.0f, 0.0f);
      if (!waitingForHint && item.isMissing && elementTime < elapsedTime - timelineTotalDuration / 2.0f)
      {
        paused = true;
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

  string getWaitingHintEmojiCode()
  {
    if (!waitingForHint)
    {
      return "";
    }
    return script.items[hintIndex].emojiCode;
  }

  float GetTimelinePosition(float time)
  {
    // clamp time to 0 and timelineTotalDuration
    return timelinePositionX + timelineWidth * (1.0f - (-time + elapsedTime) / timelineTotalDuration);
  }
}
