using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle _roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeList;

    private RoomNodeSO currentRoomNode = null;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 5.5f;


    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");

    }
    private void OnEnable()
    {
        Selection.selectionChanged += SelectionChanged;
        _roomNodeStyle = new GUIStyle();
        _roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        _roomNodeStyle.normal.textColor = Color.white;
        _roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        _roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeTypeList = GameResources.Instance.RoomNodeTypeList;


    }

    private void OnDisable()
    {
        Selection.selectionChanged -= SelectionChanged;
    }

    private void SelectionChanged()
    {
        var graph = Selection.activeObject as RoomNodeGraphSO;

        if (graph != null)
        {
            currentRoomNodeGraph = graph;
            GUI.changed = true;
        }
    }

    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        var graph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (graph == null) return false;

        OpenWindow();
        currentRoomNodeGraph = graph;
        return true;
    }

    
    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            DrawDraggedLine();

            ProcessEvents(Event.current);

            DrawRoomConnections();

            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void DrawRoomConnections()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (var childRoomId in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.RoomNodeDictionary.ContainsKey(childRoomId))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.RoomNodeDictionary[childRoomId]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO fromNode, RoomNodeSO toNode)
    {
        var startPositon = fromNode.Rectangle.center;
        var endPosition = toNode.Rectangle.center;

        var midPosition = (endPosition + startPositon) / 2f;

        var direction = endPosition - startPositon;

        var arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        var arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        var arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPositon, endPosition,
            startPositon, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.LinePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.RoomNodeFrom.Rectangle.center, currentRoomNodeGraph.LinePosition,
                currentRoomNodeGraph.RoomNodeFrom.Rectangle.center, currentRoomNodeGraph.LinePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        if (currentRoomNode == null || !currentRoomNode.IsLeftClickDragging)
        {
            TryGetRoomNodeMouseIsOver(currentEvent.mousePosition, out currentRoomNode);
        }
        
        // if mouse isn't over a room node or we are currently dragging a line from the room node, then process graph events
        if (currentRoomNode == null || currentRoomNodeGraph.RoomNodeFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    private bool TryGetRoomNodeMouseIsOver(Vector2 mousePosition, out RoomNodeSO roomNode)
    {
        for (var i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (!currentRoomNodeGraph.roomNodeList[i].Rectangle.Contains(mousePosition)) continue;

            roomNode = currentRoomNodeGraph.roomNodeList[i];
            return true;
        }

        roomNode = null;
        return false;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.RoomNodeFrom != null)
        {
            if (TryGetRoomNodeMouseIsOver(currentEvent.mousePosition, out var roomNode))
            {
                if (currentRoomNodeGraph.RoomNodeFrom.AddChild(roomNode.id))
                {
                    roomNode.AddParent(currentRoomNodeGraph.RoomNodeFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        currentRoomNodeGraph.RoomNodeFrom = null;
        currentRoomNodeGraph.LinePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.RoomNodeFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.LinePosition += delta;
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        } else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.IsSelected)
            {
                roomNode.IsSelected = false;
                GUI.changed = true;
            }
        }

    }

    private void ShowContextMenu(Vector2 position)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, position);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeletedSelctedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeletedSelctedRoomNodes);

        menu.ShowAsContext();
    }

    private void DeletedSelctedRoomNodes()
    {
        var roomNodeToDelete = new Queue<RoomNodeSO>();

        foreach(var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.IsSelected && !roomNode.RoomNodeType.IsEntrance)
            {
                roomNodeToDelete.Enqueue(roomNode);

                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    var childRoomNode = currentRoomNodeGraph.GetRoomNodeById(childRoomNodeID);
                    if(childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                foreach(var parentRoomID in roomNode.parentRoomNodeIDList)
                {
                    var parentRoomNode = currentRoomNodeGraph.GetRoomNodeById(parentRoomID);
                    if(parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        while(roomNodeToDelete.Count > 0)
        {
            var roomNode = roomNodeToDelete.Dequeue();
            currentRoomNodeGraph.RoomNodeDictionary.Remove(roomNode.id);

            currentRoomNodeGraph.roomNodeList.Remove(roomNode);
            DestroyImmediate(roomNode, true);

            AssetDatabase.SaveAssets();
        }
    }

    private void DeletedSelctedRoomNodeLinks()
    {
        foreach(var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.IsSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for(var i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    var childRoomNode = currentRoomNodeGraph.GetRoomNodeById(roomNode.childRoomNodeIDList[i]);

                    if(childRoomNode != null && childRoomNode.IsSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }

    private void SelectAllRoomNodes()
    {
        foreach(var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.IsSelected = true;
        }
        
        GUI.changed = true;
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.List.Find(x => x.IsEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.List.Find(x => x.IsNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        var mousePosition = (Vector2)mousePositionObject;

        var roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

        AssetDatabase.SaveAssets();

        currentRoomNodeGraph.OnValidate();
    }

    private void DrawRoomNodes()
    {
        foreach (var roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNode.IsSelected ? roomNodeSelectedStyle : _roomNodeStyle);
        }

        GUI.changed = true;
    }
}

