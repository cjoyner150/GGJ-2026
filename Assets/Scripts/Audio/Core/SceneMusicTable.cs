using System;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Scene Music Table")]
public class SceneMusicTable : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string sceneName;
        public EventReference musicEvent;
    }

    public Entry[] entries;

    public bool TryGet(string sceneName, out EventReference musicEvent)
    {
        if (entries != null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].sceneName == sceneName)
                {
                    musicEvent = entries[i].musicEvent;
                    return true;
                }
            }
        }

        musicEvent = default;
        return false;
    }
}
