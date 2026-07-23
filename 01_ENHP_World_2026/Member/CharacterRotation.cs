using UnityEngine;

public class CharacterRotation : MonoBehaviour
{
    public float rotationSpeed = 5f;
    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private bool isCharacterSelected = false;

    void Update()
    {
        // 마우스 버튼을 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            CheckCharacterSelection();
            if (isCharacterSelected)
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        // 마우스를 드래그할 때
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        // 마우스 버튼을 뗐을 때
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isCharacterSelected = false;
        }
    }

    private void CheckCharacterSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isCharacterSelected = true;
            }
        }
    }
}
