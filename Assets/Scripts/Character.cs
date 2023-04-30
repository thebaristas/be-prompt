using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Colors
// 62A2CD
// DEE38C
public class Character : MonoBehaviour
{
    public string id;
    public Bubble bubble;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SubscribeToCardDisplayEvents(HandleCardDisplay);
    }

    public void OnDrop(GameObject gameObject) {
        Debug.Log($"Dropped on character {gameObject.name}");
    }

    public void HandleCardDisplay(string actorId, string? cardId) {
        if (actorId != id) {
            bubble.Hide();
        } else {
            if (cardId != null) {
                bubble.Display("Cards/" + cardId);
            } else {
                bubble.Display("questionMark");
            }
        }
    }
}
