using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private Vector3 initialScale = new Vector3(1,1,1);
    private bool display = false;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
    }


    // Update is called once per frame
    void Update()
    {
        float speed = 1.0f;
        if (display) {
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, speed * Time.deltaTime);
            gameObject.SetActive(true);
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0,0,0), speed * Time.deltaTime);
            gameObject.SetActive(false);
        }
    }

    public void Display(string resourcePath) {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        SpriteRenderer cardSprite = prefab.GetComponent<SpriteRenderer>();
        if (cardSprite) {
            spriteRenderer.sprite = cardSprite.sprite;
        }
        display = true;
    }

    public void Hide() {
        display = false;
    }
}
