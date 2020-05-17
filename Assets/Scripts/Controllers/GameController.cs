using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    private const float LEVEL_MAX_TIME = 5f;
    private const int MIN_TABLETS = 2;

    private bool _isLoadingScene;
    private bool _isGameOver;
    private float _time;
    private int _score;
    private int _highscore;
    private int _allPossibleVectors {
        get
        {
            if (_innerCircle != null && _outerCircle != null)
                return _innerCircle.Count + _outerCircle.Count;
            return 0;
        }
    }
    private HCharacter _chosenCharacter;
    private List<HCharacter> _characters;
    private List<Vector2> _innerCircle;
    private List<Vector2> _outerCircle;
    private List<GameObject> _tabletObjs;
    private GameObject _splashScreenObj;

    private HCharacterJsonBase _jsonResource;
    private GameObject _endScreenResource;
    private GameObject _characterPanelResource;

    private AudioSource _audioSource;
    private AudioClip _whooshClipResource;
    private AudioClip _clickClipResource;

    // Use this for initialization
    IEnumerator Start () {
        _highscore = 0;
        _tabletObjs = new List<GameObject>();
        _audioSource = GetComponent<AudioSource>();

        LoadResources();
        AssignAllTabletPositions();
        GenerateCharacters();

        yield return StartCoroutine(
            YieldUntilSuccess(
                () => UpdateScore(0) && IsParentTransformAvailable(),
                () => StartGame()
            )
        );
    }

    private void LoadResources()
    {
        Debug.Log("Loading Resources...");
        _endScreenResource = Resources.Load<GameObject>("CharacterPanel");
        _characterPanelResource = Resources.Load<GameObject>("EndScreen");
        _jsonResource = JsonUtility.FromJson<HCharacterJsonBase>(Resources.Load<TextAsset>("hiragana").text);
        _whooshClipResource = Resources.Load<AudioClip>("whoosh");
        _clickClipResource = Resources.Load<AudioClip>("click");
        Debug.Log("Loading Completed!");
    }

    private void AssignAllTabletPositions()
    {
        var controllerMidHeight = (transform as RectTransform).rect.height / 2;
        _innerCircle = new List<Vector2> {
            new Vector2(controllerMidHeight * 1, controllerMidHeight * 1),
            new Vector2(controllerMidHeight * 1, controllerMidHeight * 0),
            new Vector2(controllerMidHeight * 1, controllerMidHeight * -1),
            new Vector2(controllerMidHeight * 0, controllerMidHeight * 1),
            new Vector2(controllerMidHeight * 0, controllerMidHeight * -1),
            new Vector2(controllerMidHeight * -1, controllerMidHeight * 1),
            new Vector2(controllerMidHeight * -1, controllerMidHeight * 0),
            new Vector2(controllerMidHeight * -1, controllerMidHeight * -1)
        };

        _outerCircle = new List<Vector2> {
            new Vector2(controllerMidHeight * 2, controllerMidHeight * 1),
            new Vector2(controllerMidHeight * 2, controllerMidHeight * 0),
            new Vector2(controllerMidHeight * 2, controllerMidHeight * -1),
            new Vector2(controllerMidHeight * -2, controllerMidHeight * 1),
            new Vector2(controllerMidHeight * -2, controllerMidHeight * 0),
            new Vector2(controllerMidHeight * -2, controllerMidHeight * -1)
        };
    }

    private void PlayWhooshClip()
    {
        _audioSource.PlayOneShot(_whooshClipResource, 0.7f);
    }

    private void PlayClickClip()
    {
        _audioSource.PlayOneShot(_clickClipResource, 0.7f);
    }

    private bool StartGame()
    {
        _isGameOver = false;
        _time = LEVEL_MAX_TIME;
        _score = 0;

        GenerateLevel(MIN_TABLETS);
        PlayWhooshClip();
        return true;
    }

    private IEnumerator YieldUntilSuccess(System.Func<bool> requiredProcess, System.Func<bool> endProcess)
    {
        _isLoadingScene = true;
        var result = true;
        try
        {
            requiredProcess();
        }
        catch(System.Exception e)
        {
            result = string.IsNullOrEmpty(e.Message);
        }

        if(result)
        {
            _isLoadingScene = false;
            endProcess();
        }

        Debug.Log("Yield result: " + result.ToString());
        yield return result;
    }

    private void GenerateCharacters()
    {
        _characters = new List<HCharacter>();

        Debug.Log("Loading Monographs...");
        LoadKanaGroup(_jsonResource.monographs);

        Debug.Log("Loading Diacritics...");
        LoadKanaGroup(_jsonResource.diacritics);

        Debug.Log("Loading Completed (" + _characters.Count + ")!");
    }

    private void LoadKanaGroup(HCharacterJsonKanaGroup kGroup)
    {
        for (int i = 0; i < kGroup.groups.Count; i++)
        {
            for (int j = 0; j < kGroup.groups[i].characters.Count; j++)
            {
                LoadCharacter(kGroup.groups[i].characters[j]);
            }
        }
        for (int i = 0; i < kGroup.wildcards.Count; i++)
        {
            LoadCharacter(kGroup.wildcards[i]);
        }
    }

    private void LoadCharacter(HCharacter hCharacter)
    {
        _characters.Add(hCharacter);
        Debug.Log("Loaded: " + hCharacter.id + " - " + hCharacter.roman + " (" + hCharacter.character + ")");
    }

    private bool UpdateScore(int newScore)
    {
        var scoreTextComponent = transform
            .Find("ScorePanel")
            .Find("ScoreText")
            .GetComponent<Text>();

        scoreTextComponent.text = "Score: " + newScore.ToString();
        return true;
    }

    private void GenerateLevel(int tabletQuantity)
    {
        UpdateScore(_score);

        ClearTablets();

        Debug.Log("Creating Tablets...");

        var innerCircleOccupated = new List<int>();
        var outerCircleOccupated = new List<int>();
        var finalVectors = new List<Vector2>();
        var totalOccupated = 0;

        if (tabletQuantity > _allPossibleVectors)
            tabletQuantity = _allPossibleVectors;

        while (totalOccupated < tabletQuantity)
        {
            if(innerCircleOccupated.Count < _innerCircle.Count)
            {
                var rngValue = Random.Range(0, _innerCircle.Count);
                if (!innerCircleOccupated.Contains(rngValue))
                {
                    innerCircleOccupated.Add(rngValue);
                    finalVectors.Add(_innerCircle.ElementAt(rngValue));
                }
            }
            else
            {
                var rngValue = Random.Range(0, _outerCircle.Count);
                if (!outerCircleOccupated.Contains(rngValue))
                {
                    outerCircleOccupated.Add(rngValue);
                    finalVectors.Add(_outerCircle.ElementAt(rngValue));
                }
            }
            totalOccupated = innerCircleOccupated.Count + outerCircleOccupated.Count;
        }

        var characterList = new List<HCharacter>();
        for (int i = 0; i < tabletQuantity; i++)
        {
            var newList = _characters.Where(
                    x => !characterList.Select(y => y.id).Contains(x.id)
                ).ToList();
            characterList.Add(newList.ElementAt(Random.Range(0, newList.Count)));

            _tabletObjs.Add(Instantiate(_endScreenResource, transform) as GameObject);
            _tabletObjs.ElementAt(i).transform.position = TranslateTablet(finalVectors.ElementAt(i));
            _tabletObjs.ElementAt(i).transform.Find("DisplayedCharacter").GetComponent<Text>().text = characterList.ElementAt(i).character;

            if(i > 0)
            {
                _tabletObjs.ElementAt(i).GetComponent<Button>().onClick.AddListener(() => OnTabletClick(false));
            }
            else
            {
                _tabletObjs.ElementAt(i).GetComponent<Button>().onClick.AddListener(() => OnTabletClick(true));
            }

            Debug.Log(
                "Tablet Created: "
                    + _tabletObjs.ElementAt(i).transform.Find("DisplayedCharacter").GetComponent<Text>().text,
                _tabletObjs.ElementAt(i)
            );
        }

        _chosenCharacter = characterList.First();
        transform.Find("TargetPanel").Find("TargetText").GetComponent<Text>().text = _chosenCharacter.roman;
        Debug.Log(
            "Character Chosen: "
                + _chosenCharacter.character
                + " - "
                + transform.Find("TargetPanel").Find("TargetText").GetComponent<Text>().text
                + " ("
                + _chosenCharacter.id
                + ")"
        );
    }

    private void ClearTablets()
    {
        Debug.Log("Clearing Tablets...");
        for (int i = 0; i < _tabletObjs.Count; i++)
        {
            Destroy(_tabletObjs.ElementAt(i));
        }
        _tabletObjs.Clear();
        Debug.Log("Tablets Cleared (" + _tabletObjs.Count + ")!");
    }

    private Vector2 TranslateTablet(Vector2 pos)
    {
        var tabletPos = Camera.main.ScreenToWorldPoint(
            new Vector3(
                (Screen.width / 2) + pos.x,
                (Screen.height / 2) + pos.y,
                Camera.main.transform.position.z
            )
        );
        return new Vector2(tabletPos.x, tabletPos.y);
    }

    // Update is called once per frame
    void Update () {
        UpdateTime();
    }

    private void UpdateTime()
    {
        if (!_isGameOver && !_isLoadingScene)
        {
            _time -= Time.deltaTime;
            var mod = _time % 1;
            var displayedTime = mod == 0
                ? _time
                : _time - mod + 1;

            var timePanel = transform.Find("TimePanel");
            if (timePanel != null)
            {
                var timeText = timePanel.Find("TimeText");
                var timeTextComponent = timeText.GetComponent<Text>();
                timeTextComponent.text = "Time: " + ((int)displayedTime).ToString();
            }

            if (_time <= 0)
            {
                EndGame(false);
                PlayWhooshClip();
            }
        }
    }

    public void OnTabletClick(bool isCorrectTablet)
    {
        if (!isCorrectTablet)
        {
            EndGame(true);
            PlayWhooshClip();
            return;
        }

        _time = LEVEL_MAX_TIME;
        _score++;
        var expectedTablets = _score + MIN_TABLETS;
        GenerateLevel(
            _allPossibleVectors < expectedTablets
                ? _allPossibleVectors
                : expectedTablets
        );
        PlayClickClip();
    }

    private bool IsParentTransformAvailable()
    {
        var parentTransform = transform.parent.transform;
        return transform.parent.transform != null;
    }

    private void EndGame(bool isIncorrectAnswer)
    {
        Debug.Log("Loading end screen");
        ClearTablets();

        _splashScreenObj = Instantiate(_characterPanelResource, transform.parent.transform) as GameObject;
        _splashScreenObj.transform.Find("Panel").Find("CorrectRomanText").GetComponent<Text>().text = _chosenCharacter.roman;
        _splashScreenObj.transform.Find("Panel").Find("CorrectCharacterText").GetComponent<Text>().text = _chosenCharacter.character;
        _splashScreenObj.transform.Find("Panel").Find("ScoreText").GetComponent<Text>().text = _score.ToString();
        _splashScreenObj.transform.Find("Panel").Find("RetryButton").GetComponent<Button>().onClick.AddListener(() => OnRetryButtonClick());

        _splashScreenObj.transform.Find("Panel").Find("Title").GetComponent<Text>().text = isIncorrectAnswer
            ? "Incorrect!"
            : "Time's Up!";

        if (_score > _highscore)
        {
            _highscore = _score;
            _splashScreenObj.transform.Find("Panel").Find("PersonalBest").GetComponent<Text>().text = "(Personal Best!)";
        }
        else
        {
            _splashScreenObj.transform.Find("Panel").Find("PersonalBest").GetComponent<Text>().text = "High Score: " + _highscore.ToString();
            _splashScreenObj.transform.Find("Panel").Find("PersonalBest").GetComponent<Text>().color = new Color(0, 0, 0);
        }

        _isGameOver = true;
    }

    public void OnRetryButtonClick()
    {
        StartGame();
        Destroy(_splashScreenObj);
    }
}
