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

    public void HandleCardDisplay(string actorId, string cardSpriteId, bool isMissing) {
        if (actorId != id) {
            bubble.Hide();
        } else {
            string resourcePath = $"{ResourcePaths.CardSprites}/{cardSpriteId}";
            if (isMissing) {
                resourcePath = ResourcePaths.QuestionMarkSprite;
            }
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite) {
                bubble.Display(sprite);
            }
        }
    }
}
