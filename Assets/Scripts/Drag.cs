using UnityEngine;

public class Drag : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 mOffset;
    private float mZCoord;

    void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
        initialPosition = transform.position;
    }

    void OnMouseUp() {
        transform.position = initialPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + mOffset;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            OnMouseDrag();
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            touchPos.z = transform.position.z;
            transform.position = touchPos;
        }
    }
}
