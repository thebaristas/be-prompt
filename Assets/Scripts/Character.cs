using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string id;
    public Bubble bubble;
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        SpriteRenderer spriteRen = GetComponent<SpriteRenderer>();
        if (spriteRen) {
            spriteRenderer = spriteRen;
        } else {
            Debug.LogWarning("No sprite renderer for Character");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SubscribeToCardDisplayEvents(HandleCardDisplay);
    }

    public void OnDrop(GameObject gameObject) {
        Debug.Log($"Dropped on character {gameObject.name}");
    }

    public void HandleCardDisplay(string actorId, string cardSpriteId, bool isMissing) {
        if (actorId != id) {
            bubble.Hide();
        } else {
            if (isMissing) {
                bubble.Display(ResourcePaths.QuestionMarkSprite);
            } else {
                bubble.Display($"{ResourcePaths.CardSprites}/{cardSpriteId}");
            }
        }
    }
}
