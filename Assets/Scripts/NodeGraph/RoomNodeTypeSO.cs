using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string RoomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion
    public bool DisplayNodeGraphEditor = true;

    #region Header
    [Header("One Type Should Be A Corridor")]
    #endregion
    public bool IsCorridor;

    #region Header
    [Header("One Type Should Be A CorridorNS")]
    #endregion
    public bool IsCorridorNS;

    #region Header
    [Header("One Type Should Be A CorridorEW")]
    #endregion
    public bool IsCorridorEW;

    #region Header
    [Header("One Type Should Be An Entrance")]
    #endregion
    public bool IsEntrance;

    #region Header
    [Header("One Type Should Be A Boss Room")]
    #endregion
    public bool IsBossRoom;

    #region Header
    [Header("One Type Should Be A None (Unassigned)")]
    #endregion
    public bool IsNone;

#if UNITY_EDITOR
    void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(RoomNodeTypeName), RoomNodeTypeName);
    }
#endif

}
