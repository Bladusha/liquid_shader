using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiquidShader.RuntimeData
{
    [Serializable]
    public sealed class GameState
    {
        public string version = "1";
        public string activeScene;
        public Vector3 playerPosition;
        public List<StateValue> values = new List<StateValue>();

        public string Get(string key, string fallback = "")
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].key == key)
                    return values[i].value;
            }

            return fallback;
        }

        public void Set(string key, string value)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].key == key)
                {
                    values[i].value = value;
                    return;
                }
            }

            values.Add(new StateValue { key = key, value = value });
        }
    }

    [Serializable]
    public sealed class StateValue
    {
        public string key;
        public string value;
    }
}
