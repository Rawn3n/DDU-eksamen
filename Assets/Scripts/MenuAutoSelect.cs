using UnityEngine;
using UnityEngine.EventSystems;

public class MenuAutoSelect : MonoBehaviour
{
    [SerializeField] private GameObject firstSelectedButton;

    void OnEnable()
    {
        StartCoroutine(SelectDelay());
    }

    private System.Collections.IEnumerator SelectDelay()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}