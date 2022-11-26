using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources _instance;

    public static GameResources Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameResources>("GameResources");
            }

            return _instance;
        }
    }

    [Space(10)] 
    [Header("DUNGEON")] 
    [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
    public RoomNodeTypeListSO RoomNodeTypeList;
}
