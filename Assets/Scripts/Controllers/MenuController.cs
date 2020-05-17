using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    private GameObject _instructionsPanelResource;
    private GameObject _instructionsPanel;
    private AudioSource _audioSource;
    private AudioClip _whooshClipResource;

    // Use this for initialization
    IEnumerator Start () {
        Screen.SetResolution(1125, 634, false);
        _instructionsPanelResource = Resources.Load<GameObject>("InstructionsPanel");
        _audioSource = GetComponent<AudioSource>();
        _whooshClipResource = Resources.Load<AudioClip>("whoosh");

        yield return StartCoroutine(WaitUntilFrameLoad());
        PlayWhooshClip();
    }
	
	// Update is called once per frame
	void Update () {

    }

    private IEnumerator WaitUntilFrameLoad()
    {
        yield return new WaitForEndOfFrame();
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("game", LoadSceneMode.Single);
    }

    public void OnInstructionsButtonClick()
    {
        _instructionsPanel = Instantiate(_instructionsPanelResource, transform) as GameObject;
        _instructionsPanel.transform.Find("CloseButton").GetComponent<Button>().onClick.AddListener(() => OnCloseButtonClick());
        PlayWhooshClip();
    }

    public void OnCloseButtonClick()
    {
        Destroy(_instructionsPanel);
        PlayWhooshClip();
    }

    private void PlayWhooshClip()
    {
        _audioSource.PlayOneShot(_whooshClipResource, 0.7f);
    }
}
