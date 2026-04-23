using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class NPC : MonoBehaviour, IInteractable
{
    public NpcDialog dialog;
    public GameObject dialogPanel;
    public TMPro.TextMeshProUGUI dialogText, nameText;
    public Image portraitImage;

    private int dialogIndex;
    private bool isTyping, isDialogActive;

    public bool CanInteract()
    {
        return !isDialogActive;
    }

    public void Interact()
    {
        if (dialog == null) return;

        if (isDialogActive)
            NextLine();
        else
            StartDialog();
    }

    public void StartDialog()
    {
        isDialogActive = true;
        dialogIndex = 0;

        nameText.SetText(dialog.npcName);
        portraitImage.sprite = dialog.npcPortrait;

        dialogPanel.SetActive(true);

        GameManager.Instance.GetComponent<PlayerInput>()
            .SwitchCurrentActionMap("UI");

        GameManager.Instance.FreezeSwitch();
        PauseManager.Instance.FreezeGameplay();
        StartCoroutine(TypeLine());
    }

    public void EndDialog()
    {
        StopAllCoroutines();
        isDialogActive = false;
        dialogText.SetText("");
        dialogPanel.SetActive(false);

        GameManager.Instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");

        GameManager.Instance.UnfreezeSwitch();
        PauseManager.Instance.UnfreezeGameplay();
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogText.SetText("");

        foreach (char c in dialog.dialogLines[dialogIndex])
        {
            dialogText.text += c;
            yield return new WaitForSecondsRealtime(dialog.typingSpeed);
        }

        isTyping = false;

        if (dialog.autoProgess.Length > dialogIndex && dialog.autoProgess[dialogIndex])
        {
            yield return new WaitForSecondsRealtime(dialog.autoProgressDelay);
            NextLine();
        }
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogText.SetText(dialog.dialogLines[dialogIndex]);
            isTyping = false;
            return;
        }

        dialogIndex++;
        if (dialogIndex < dialog.dialogLines.Length)
            StartCoroutine(TypeLine());
        else
            EndDialog();
    }
}