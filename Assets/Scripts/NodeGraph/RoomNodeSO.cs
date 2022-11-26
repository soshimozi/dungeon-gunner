using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO RoomNodeType;
    [HideInInspector] public RoomNodeTypeListSO RoomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Rect Rectangle;
    [HideInInspector] public bool IsLeftClickDragging = false;
    [HideInInspector] public bool IsSelected = false;

    public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO nodeType)
    {
        Rectangle = rect;
        id = Guid.NewGuid().ToString();
        name = "RoomNode";
        roomNodeGraph = nodeGraph;
        RoomNodeType = nodeType;

        // load room node type lsit
        RoomNodeTypeList = GameResources.Instance.RoomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(Rectangle, nodeStyle);

        EditorGUI.BeginChangeCheck();

        if (parentRoomNodeIDList.Count > 0 || RoomNodeType.IsEntrance)
        {
            EditorGUILayout.LabelField(RoomNodeType.RoomNodeTypeName);
        }
        else
        {
            var selected = RoomNodeTypeList.List.FindIndex(x => x == RoomNodeType);
            var selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            RoomNodeType = RoomNodeTypeList.List[selection];
            var selectedRoomNodeType = RoomNodeTypeList.List[selected];

            if(selectedRoomNodeType.IsCorridor && !RoomNodeType.IsCorridor ||
                !selectedRoomNodeType.IsCorridor && RoomNodeType.IsCorridor ||
                !selectedRoomNodeType.IsBossRoom && RoomNodeType.IsBossRoom)
            {
                if(childRoomNodeIDList.Count > 0)
                {
                    for(int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        var childRoomNode = roomNodeGraph.GetRoomNodeById(childRoomNodeIDList[i]);

                        if(childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }    
                    }    
                }
            }

        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();

    }

    public string[] GetRoomNodeTypesToDisplay()
    {
        var roomArray = new string[RoomNodeTypeList.List.Count];
        for (var i = 0; i < RoomNodeTypeList.List.Count; i++)
        {
            if (RoomNodeTypeList.List[i].DisplayNodeGraphEditor)
            {
                roomArray[i] = RoomNodeTypeList.List[i].RoomNodeTypeName;
            }
        }

        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent(currentEvent);
        } else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.ConnectRoomNode(this, currentEvent.mousePosition);
    }

    private void ProcessLeftClickDownEvent(Event currentEvent)
    {
        Selection.activeObject = this;

        IsSelected = !IsSelected;
    }

    private void ProcessLeftClickUpEvent()
    {
        Selection.activeObject = this;

        if (IsLeftClickDragging)
        {
            IsLeftClickDragging = false;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        IsLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    private void DragNode(Vector2 delta)
    {
        Rectangle.position += delta;
        EditorUtility.SetDirty(this);
    }

    public bool AddChild(string childID)
    {
        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    private bool IsChildRoomValid(string childId)
    {
        bool IsConnectedBossNodeAlready = false;
        foreach (var roomNode in roomNodeGraph.roomNodeList)
        {
            if (roomNode.RoomNodeType.IsBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                IsConnectedBossNodeAlready = true;
                break;
            }
        }

        if (roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsBossRoom && IsConnectedBossNodeAlready)
            return false;

        if (roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsNone)
            return false;

        if (childRoomNodeIDList.Contains(childId))
            return false;

        if (id == childId)
            return false;

        if (parentRoomNodeIDList.Contains(childId))
            return false;

        if (roomNodeGraph.GetRoomNodeById(childId).parentRoomNodeIDList.Count > 0)
            return false;

        if (roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsCorridor && RoomNodeType.IsCorridor)
            return false;

        if (!roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsCorridor && !RoomNodeType.IsCorridor)
            return false;

        if (roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsCorridor &&
            childRoomNodeIDList.Count >= Settings.MaxChildCorridors)
            return false;

        if (roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsEntrance)
            return false;

        if (!roomNodeGraph.GetRoomNodeById(childId).RoomNodeType.IsCorridor &&
            childRoomNodeIDList.Count > 0)
            return false;
            
        return true;
    }

    public bool AddParent(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }
        
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        if(parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }

        return false;
    }
        
#endif

    #endregion
}
