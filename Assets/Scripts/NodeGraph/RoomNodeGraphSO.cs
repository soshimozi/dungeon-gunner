using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> RoomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    private void LoadRoomNodeDictionary()
    {
        RoomNodeDictionary.Clear();

        foreach (var node in roomNodeList)
        {
            RoomNodeDictionary[node.id] = node;
        }
    }

    public RoomNodeSO GetRoomNodeById(string roomNodeId)
    {
        if(RoomNodeDictionary.TryGetValue(roomNodeId, out var roomNode))
        {
            return roomNode;
        }

        return null;
    }

    #region Editor Code

#if UNITY_EDITOR
    // if we are drawing a line
    [HideInInspector] public RoomNodeSO RoomNodeFrom = null;
    [HideInInspector] public Vector2 LinePosition;


    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void ConnectRoomNode(RoomNodeSO roomNode, Vector2 position)
    {
        RoomNodeFrom = roomNode;
        LinePosition = position;
    }

#endif

    #endregion

}
