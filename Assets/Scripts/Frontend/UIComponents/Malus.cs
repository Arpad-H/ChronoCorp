using Backend.Simulation.World;
using UnityEngine;
using UnityEngine.UI;

public class Malus : MonoBehaviour
{
    private bool isActive = false;
    public Image malusIcon;
    public Color activeColor = Color.red;
    public Color inactiveColor = Color.gray;
    public StabilityMalusType malusType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ToggleMalus(false);
    }
    
    public void ToggleMalus(bool newActive)
    {
        isActive = newActive;
        if (isActive)
        {
            malusIcon.color = activeColor;
        }
        else
        {
            malusIcon.color = inactiveColor;
        } 
    }
}