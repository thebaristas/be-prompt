using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;

    private bool isDragging = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
        isDragging = true;
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