using UnityEngine;

public class ObjUICharacterBase : MonoBehaviour
{
    [SerializeField] private GameObject ObjCharacterParent;
    [SerializeField] private Transform TrsPos;

    public GameObject CharacterParent => ObjCharacterParent;

    public void SetPosition(Vector3 vecPos)
    {
        if (TrsPos != null)
        {
            TrsPos.localPosition = vecPos;
        }
    }
}
