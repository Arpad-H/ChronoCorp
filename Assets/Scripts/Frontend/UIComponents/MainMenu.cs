using UnityEngine;

namespace Frontend.UIComponents
{
    public class MainMenu: MonoBehaviour
    
    {
        public GameObject MainMenuPanel;
        public GameObject DialoguePanel;
        
       public void StartGame()
        {
            MainMenuPanel.SetActive(false);
            DialoguePanel.SetActive(true);
        }
        
    }
}