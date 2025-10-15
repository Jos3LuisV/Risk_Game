using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    public static bool Exists()
    {
        return instance != null;
    }

    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null) return;

        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                Action action = executionQueue.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error ejecutando acción en hilo principal: {e.Message}");
                }
            }
        }
    }

    public static void Create()
    {
        if (instance != null) return;

        GameObject go = new GameObject("MainThreadDispatcher");
        go.AddComponent<MainThreadDispatcher>();
    }
}
