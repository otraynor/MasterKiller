using UnityEngine;

public class ToggleInstructions : MonoBehaviour
{
    [SerializeField] private GameObject instructionsPop;

    private void Start()
    {
        if (instructionsPop != null)
        {
            instructionsPop.SetActive(false);
        }
    }
    
    public void ShowInstructions()
    {
        if (instructionsPop != null)
        {
            instructionsPop.SetActive(true);
        }
    }
    
    public void HideInstructions()
    {
        if (instructionsPop != null)
        {
            instructionsPop.SetActive(false);
        }
    }
}