using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject Settings;

    [Header("Tutorials")]
    public int TutorialIndex = -1;
    public GameObject[] tutorialImages;
    public Animator tutorialAnimator;
    // Start is called before the first frame update
    void Start()
    {
        ShowTutorial(TutorialIndex);
        Settings.transform.GetChild(2).GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Settings.transform.GetChild(3).GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("SFX", 1) == 1;
        Settings.transform.GetChild(4).GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("Music", 1) == 1;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetButtonDown("Space") || Input.GetMouseButtonDown(0)) && TutorialIndex > -1)
        {
            IncrementTutorial();
        }
    }

    public void PlayHoverSound()
    {
        transform.GetChild(0).GetComponent<AudioSource>().Play();
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
    public void SetSettings(bool value)
    {
        Settings.SetActive(value);
    }
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void SetFullScreen(bool value)
    {
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        Screen.fullScreen = value;
    }

    public void SetSFX(bool value)
    {
        PlayerPrefs.SetInt("SFX", value ? 1 : 0);
    }

    public void SetMusic(bool value)
    {
        PlayerPrefs.SetInt("Music", value ? 1 : 0);
    }
    public void ShowTutorial(int n)
    {
        foreach (GameObject x in tutorialImages)
        {
            x.SetActive(n != -1);
        }
        if (n != -1) tutorialImages[n].transform.SetAsLastSibling();

        tutorialAnimator.SetInteger("State", n);
    }
    public void IncrementTutorial()
    {
        TutorialIndex++;
        if (TutorialIndex == 3) TutorialIndex = -1;
        ShowTutorial(TutorialIndex);
    }
}
