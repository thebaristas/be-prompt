using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeline : MonoBehaviour
{
  Script script;
  bool paused = false; // whether the timeline is paused
  bool waitingForHint = false; // whether the player should give a hint
  int hintIndex = 0; // the index of the script item that needs a hint
  // script speed
  // start time
  // script time (progress)

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
      // Add a new script item
      script.items.Add(new ScriptItem(emojiCode, playerID));
    }

    // Get the timeline width and position
    var sprite = GetComponent<SpriteRenderer>();
    timelineWidth = sprite.bounds.size.x;
    timelinePositionX = sprite.bounds.center.x - timelineWidth / 2.0f;
  }

  // Update is called once per frame
  void Update()
  {
    // Space bar pressed
    if (Input.GetKeyDown(KeyCode.Space))
    {
      // Toggle the paused state
      if (waitingForHint) {
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
        // Make missing items red
        if (item.isMissing)
        {
          sr.color = Color.red;
        }
        // Make sr a child of this game object
        sr.transform.parent = transform;
        // Add the game object to the dictionary
        elementToGameObject[i] = sr;
        // Print the creation (index and emoji code) of the game object
        // Debug.Log("Created " + i + " " + item.emojiCode);
      }
      var spriteRenderer = elementToGameObject[i];
      spriteRenderer.transform.position = transform.position + new Vector3(GetTimelinePosition(elementTime), 0.0f, 0.0f);
      if (item.isMissing && elementTime < elapsedTime - timelineTotalDuration / 2.0f)
      {
        paused = true;
        hintIndex = i;
        waitingForHint = true;
      }
      elementTime += item.delay;
      // Print the index, emoji code and position of the game object
      //   Debug.Log(i + " " + item.emojiCode + " " + spriteRenderer.transform.position);
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
      if (item.isMissing) {
        Debug.Log("Missed " + i + " " + item.emojiCode);
      }
      Destroy(spriteRenderer.gameObject);
    //   Debug.Log("Deleted " + i + " " + item.emojiCode);
      elementToGameObject.Remove(i);
    }
  }

  float GetTimelinePosition(float time)
  {
    // clamp time to 0 and timelineTotalDuration
    return timelinePositionX + timelineWidth * (1.0f - (-time + elapsedTime) / timelineTotalDuration);
  }
}