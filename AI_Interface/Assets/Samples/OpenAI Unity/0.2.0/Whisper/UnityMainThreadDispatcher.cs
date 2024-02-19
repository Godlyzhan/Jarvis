using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;

    private Queue<Action> actionQueue = new Queue<Action>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        while (actionQueue.Count > 0)
        {
            actionQueue.Dequeue().Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        actionQueue.Enqueue(action);
    }

    public static UnityMainThreadDispatcher Instance()
    {
        return instance;
    }
}