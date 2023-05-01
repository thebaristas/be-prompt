using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Bubble bubble;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public float animatorBlend;
    private GameManager gameManager;

    void Awake()
    {
        SpriteRenderer spriteRen = GetComponent<SpriteRenderer>();
        spriteRen.material.SetFloat("_OutlineEnabled", 0);
        if (spriteRen) {
            spriteRenderer = spriteRen;
        } else {
            Debug.LogWarning("No sprite renderer for Character");
        }
        Animator anim = GetComponent<Animator>();
        if (anim) {
            animator = anim;
            animator.SetFloat(AnimatorParamsName.Blend, animatorBlend);
        } else {
            Debug.LogWarning("No animator for Character");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (gameManager == null) {
            gameManager = FindAnyObjectByType<GameManager>();
        }
        gameManager.SubscribeToCardDisplayEvents(HandleCardDisplay);
    }

    public void HandleCardDrop(bool correct, string cardSpriteId) {
        string trigger;
        string resourcePath;
        if (correct) {
            resourcePath = $"{ResourcePaths.CardSprites}/{cardSpriteId}";
            trigger = AnimatorParamsName.Talk;
        } else {
            resourcePath = ResourcePaths.RedMarkSprite;
            trigger = AnimatorParamsName.Idle;
        }
        Animate(trigger, resourcePath);
    }

    public void HandleCardDisplay(string actorId, string cardSpriteId, bool isMissing) {
        if (actorId != name) {
            animator.SetTrigger(AnimatorParamsName.Idle);
            bubble.Display();
        } else {
            string resourcePath = $"{ResourcePaths.CardSprites}/{cardSpriteId}";
            string trigger = AnimatorParamsName.Talk;
            if (isMissing) {
                resourcePath = ResourcePaths.QuestionMarkSprite;
                trigger = AnimatorParamsName.Embarrassed;
            }
            Animate(trigger, resourcePath);
        }
    }

    private void Animate(string animatorTrigger, string cardSpriteResourcePath) {
        Sprite sprite = Resources.Load<Sprite>(cardSpriteResourcePath);
        if (sprite) {
            bubble.Display(sprite);
            animator.SetTrigger(animatorTrigger);
        }
    }
}
