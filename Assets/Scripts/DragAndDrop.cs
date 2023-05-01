using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
  public int id { get; set; }
  // Event handler for when a card is dropped on a character
  public delegate void OnDropDelegate(GameObject dropped, Character actor);
  public event OnDropDelegate OnDropEvent;
  public GameManager gameManager;

  private Vector3 mOffset;
  private float mZCoord;

  private bool isDragging = false;
  private Vector3 initialPosition;
  private int sortingOrder;
  private Character hoveredCharacter = null;

  void Start()
  {
    if (gameManager == null)
    {
      gameManager = FindObjectOfType<GameManager>();
    }
    initialPosition = transform.position;
  }

  void OnMouseOver()
  {
    if (gameManager.cardSelected == -1)
    {
      transform.position = Vector3.Lerp(transform.position, initialPosition + new Vector3(0, 1, 0), 10.0f * Time.deltaTime);
    }
  }

  void OnMouseExit()
  {
    if (gameManager.cardSelected == -1)
    {
      transform.position = initialPosition;
    }
  }

  void OnMouseDown()
  {
    gameManager.cardSelected = id;
    mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
    mOffset = gameObject.transform.position - GetMouseWorldPos();
    isDragging = true;
    Renderer renderer = this.gameObject.GetComponent<Renderer>();
    if (renderer != null)
    {
      sortingOrder = renderer.sortingOrder;
    }
    GameManager.SetLayerRecursively(this.gameObject, "Top", sortingOrder);
    transform.position = transform.position + new Vector3(0, 1, 0);
  }

  void OnMouseDrag()
  {
    if (isDragging)
    {
      transform.position = GetMouseWorldPos() + mOffset;
      var newHoveredCharacter = GetHoveredCharacter();
      if (newHoveredCharacter != hoveredCharacter)
      {
        if (hoveredCharacter != null)
        {
          // Set the outline enabled shader property of the sprite material
          hoveredCharacter.GetComponent<SpriteRenderer>().material.SetFloat("_OutlineEnabled", 0);

        }
        hoveredCharacter = newHoveredCharacter;
        if (hoveredCharacter != null)
        {
          hoveredCharacter.GetComponent<SpriteRenderer>().material.SetFloat("_OutlineEnabled", 1);
        }
      }
    }
  }

  void OnMouseUp()
  {
    if (isDragging)
    {
      // Check if the object is colliding with another object
      Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f);
      foreach (Collider2D collider in colliders)
      {
        if (collider != gameObject.GetComponent<Collider2D>())
        {
          // Call a method on the other object
          Character character = collider.gameObject.GetComponent<Character>();
          if (character != null)
          {
            // Take the part of the gameObject name until the first underscore
            OnDropEvent(gameObject, character);
            if (hoveredCharacter != null)
            {
              // Set the outline enabled shader property of the sprite material
              hoveredCharacter.GetComponent<SpriteRenderer>().material.SetFloat("_OutlineEnabled", 0);

            }
            hoveredCharacter = null;
          }
        }
      }
    }
    isDragging = false;
    transform.position = initialPosition;
    GameManager.SetLayerRecursively(this.gameObject, "UI", sortingOrder);
    gameManager.cardSelected = -1;
  }

  Character GetHoveredCharacter()
  {
    Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0f);
    foreach (Collider2D collider in colliders)
    {
      if (collider != gameObject.GetComponent<Collider2D>())
      {
        // Call a method on the other object
        Character character = collider.gameObject.GetComponent<Character>();
        if (character != null)
        {
          return character;
        }
      }
    }
    return null;
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
