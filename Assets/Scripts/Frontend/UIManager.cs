// UIManager.cs

using Frontend.UIComponents;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject deleteButtonPrefab;
    private GameObject deleteNodeButton;
    
    
    public GameObject gameOverPanel; 
 
    

    void Awake()
    {
        Instance = this;
       
    }

    public void ShowGameOver(string reason)
    {
    
    }

    public DeleteButton SpawnDeleteButton(Vector3 position)
    {
        if (deleteNodeButton != null)
        {
            Destroy(deleteNodeButton);
        }
        deleteNodeButton = Instantiate(deleteButtonPrefab, position, Quaternion.identity);
        return deleteNodeButton.GetComponent<DeleteButton>();
    }
}