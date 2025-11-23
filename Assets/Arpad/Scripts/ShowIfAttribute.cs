using UnityEngine;

public class ShowIfAttribute  : PropertyAttribute
{
    public string enumFieldName;
    public int enumValue;

    public ShowIfAttribute(string enumFieldName, int enumValue)
    {
        this.enumFieldName = enumFieldName;
        this.enumValue = enumValue;
    }
}
