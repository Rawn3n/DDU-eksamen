using UnityEngine;

public abstract class Task : MonoBehaviour
{
    public bool IsCompleted { get; protected set; }

    protected void CompleteTask()
    {
        IsCompleted = true;
        Debug.Log($"task completed!");
    }
}