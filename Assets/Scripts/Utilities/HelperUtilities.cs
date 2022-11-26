using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        return string.IsNullOrEmpty(stringToCheck);
    }

    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName,
        IEnumerable enumerableObjectToCheck)
    {
        var error = false;
        var count = 0;

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log($"{fieldName} has null values in object {thisObject?.name}");
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count != 0) return error;

        Debug.Log(($"{fieldName} has no values in object {thisObject?.name}"));
        return true;
    }
}
