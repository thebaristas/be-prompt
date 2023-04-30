using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public int id {get; set;}
    // Event handler for when a card is dropped on a character
    public delegate void OnDropDelegate(string cardId, string actorId);
    public static event OnDropDelegate OnDropEvent;

    private Vector3 mOffset;
    private float mZCoord;

    private bool isDragging = false;
    private Vector3 initialPosition;
    private int sortingOrder;

    void Start()
    {
        initialPosition = transform.position;
    }

    void OnMouseOver() {
        if (GameManager.Instance.cardSelected == -1) {
            transform.position = Vector3.Lerp(transform.position, initialPosition + new Vector3(0,1,0), 10.0f * Time.deltaTime);
        }
    }

    void OnMouseExit() {
        if (GameManager.Instance.cardSelected == -1) {
            transform.position = initialPosition;
        }
    }

    void OnMouseDown()
    {
        GameManager.Instance.cardSelected = id;
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
        isDragging = true;
        Renderer renderer = this.gameObject.GetComponent<Renderer>();
        if (renderer != null) {
            sortingOrder = renderer.sortingOrder;
        }
        GameManager.SetLayerRecursively(this.gameObject, "Top", sortingOrder);
        transform.position = transform.position + new Vector3(0,1,0);
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + mOffset;
        }
    }

    void OnMouseUp()
    {
        if (isDragging) {
            // Check if the object is colliding with another object
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f);
            foreach (Collider2D collider in colliders)
            {
                if (collider != gameObject.GetComponent<Collider2D>())
                {
                    // Call a method on the other object
                    Character character = collider.gameObject.GetComponent<Character>();
                    if (character != null) {
                        character.OnDrop(gameObject);
                    }
                }
            }
        }
        isDragging = false;
        transform.position = initialPosition;
        GameManager.SetLayerRecursively(this.gameObject, "UI", sortingOrder);
        GameManager.Instance.cardSelected = -1;
    }

    void Update()
    {
        // Check if there is a touch or a mouse hover
        bool hasTouchOrMouseHover = false;
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
                    if (hit.collider == GetComponent<Collider2D>())
                    {
                        hasTouchOrMouseHover = true;
                        break;
                    }
                }
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider == GetComponent<Collider2D>())
            {
                hasTouchOrMouseHover = true;
            }
        }

        // Handle drag if there is a touch or a mouse hover
        if (hasTouchOrMouseHover)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnMouseDown();
            }
            else if (Input.GetMouseButton(0))
            {
                OnMouseDrag();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnMouseUp();
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                touchPos.z = transform.position.z;
                transform.position = touchPos;
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
