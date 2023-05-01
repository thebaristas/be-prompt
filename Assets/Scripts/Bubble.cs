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
        initialScale = transform.localScale;
        transform.localScale = new Vector3(0,0,0);
    }


    // Update is called once per frame
    void Update()
    {
        float speed = 10.0f;
        if (display) {
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, speed * Time.deltaTime);
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0,0,0), speed * Time.deltaTime);
        }
    }

    public void Display(Sprite sprite) {
        spriteRenderer.sprite = sprite;
        display = true;
    }

    public void Display() {
        display = false;
    }
}
