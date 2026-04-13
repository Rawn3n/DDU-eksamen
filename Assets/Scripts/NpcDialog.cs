using UnityEngine;

[CreateAssetMenu(fileName = "NyNpcDialog", menuName = "NPC Dialog")]
public class NpcDialog : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public float typingSpeed = 0.05f;
    public string[] dialogLines;

    public bool[] autoProgess;
    public float autoProgressDelay = 1.5f;

    //public AudioClip voiceSound;
    //public float voicePitch = 1.0f;

}
