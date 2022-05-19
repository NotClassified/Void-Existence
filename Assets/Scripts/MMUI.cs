using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MMUI : MonoBehaviour
{
    private GameObject[] mmChildren;
    [SerializeField]
    string[] scenes;

    [SerializeField]
    GameObject[] levelButtons;

    private void Start()
    {
        //initailize the mmChildren array to the children of this transform
        mmChildren = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            mmChildren[i] = transform.GetChild(i).gameObject;
        }
        mmChildren[0].SetActive(false); //back
        mmChildren[1].SetActive(true);  //MM1
        mmChildren[2].SetActive(false);  //MM2

        foreach (GameObject button in levelButtons) //lock all levels before unlocking
            button.GetComponent<Button>().interactable = false;
        for (int i = 0; i < levelButtons.Length && i < GameProgress.levelCompleted + 1; i++) //unlock levels completed plus 1 extra level
            levelButtons[i].GetComponent<Button>().interactable = true;
        for (int i = GameProgress.levelCompleted + 1; i < levelButtons.Length; i++) //change UI of locked levels
        {
            levelButtons[i].transform.Find("#").gameObject.SetActive(false);
            levelButtons[i].transform.Find("Lock").gameObject.SetActive(true);
        }
    }
    //call when button is pressed, find which button was pressed through parameter _button
    public void Button(string _button)
    {
        if(_button == "back")
        {
            mmChildren[0].SetActive(false);
            mmChildren[1].SetActive(true);
            mmChildren[2].SetActive(false);
        }
        else if (_button == "play")
        {
            mmChildren[0].SetActive(true);
            mmChildren[1].SetActive(false);
            mmChildren[2].SetActive(true);
        }
        else if (_button == "tutorial")
        {
            SceneManager.LoadScene(scenes[1]);
        }
        else if (_button == "quit")
        {
            Application.Quit();
        }
        else if (_button == "1.1")
        {
            SceneManager.LoadScene(scenes[2]);
        }
        //else if (_button == "")
        //{

        //}
    }
}
