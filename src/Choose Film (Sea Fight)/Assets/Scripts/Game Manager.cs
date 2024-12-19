using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private AsyncOperation _sceneReloadOperation;

    [Header("Экран выбора темы")] public Canvas topicSelectionCanvas;
    public Button topicButtonPrefab;
    public Vector2 randomCardRotationRangeZ = new(-5, 5);
    public TopicData[] topics;

    [Header("Бросок кубика и карты")] public Canvas diceRollCanvas;
    public TMP_Text infoText;
    public TMP_Text throwDiceText;
    public Transform coversParent;
    public Image[] diceImages;
    public Button rollButton;
    public Button coversButton;
    public Button backToTopicsButton;
    public Vector3 cardSelectedOffset = new(0, -30, 10);

    [Header("Информация о карте")] public Canvas cardInfoCanvas;
    public Button backToTopicSelectionButton;
    public Image cardImage;

    private void Start()
    {
        InitializeSceneReload();

        SetCanvasVisibility(topicSelectionCanvas, true);
        SetCanvasVisibility(diceRollCanvas, false);
        SetCanvasVisibility(cardInfoCanvas, false);

        PopulateTopicButtons();
    }

    private void InitializeSceneReload()
    {
        _sceneReloadOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        _sceneReloadOperation!.allowSceneActivation = false;
    }

    private void PopulateTopicButtons()
    {
        foreach (var topic in topics)
        {
            var button = Instantiate(topicButtonPrefab, topicButtonPrefab.transform.parent);
            button.onClick.AddListener(() => OnTopicSelected(topic));
            SetupButtonImages(button, topic.coverImage);
            button.gameObject.SetActive(true);
        }
    }

    private void SetupButtonImages(Button button, Texture2D coverImage)
    {
        var images = button.GetComponentsInChildren<Image>();
        for (var i = 0; i < images.Length; i++)
        {
            var rectTransform = images[i].GetComponent<RectTransform>();
            images[i].sprite = CreateSprite(coverImage);

            if (i > 0)
            {
                rectTransform.localRotation = Quaternion.Euler(0, 0,
                    Random.Range(randomCardRotationRangeZ.x, randomCardRotationRangeZ.y));
            }
        }
    }

    private void OnTopicSelected(TopicData topic)
    {
        SetCanvasVisibility(topicSelectionCanvas, false);
        SetCanvasVisibility(diceRollCanvas, true);
        infoText.enabled = false;
        throwDiceText.enabled = true;

        InitializeTopicCards(topic);
        rollButton.onClick.AddListener(() => StartCoroutine(RollDice(topic)));
        backToTopicsButton.onClick.AddListener(ReloadScene);
    }

    private void InitializeTopicCards(TopicData topic)
    {
        foreach (Transform child in coversParent)
        {
            var image = child.GetComponent<Image>();
            image.sprite = CreateSprite(topic.coverImage);
        }
    }

    private IEnumerator RollDice(TopicData topic)
    {
        rollButton.interactable = false;
        var diceValue = 0;

        var coroutines = new List<Coroutine>();

        foreach (var image in diceImages)
        {
            coroutines.Add(StartCoroutine(Dice.RollTheDice(image, value => diceValue += value)));
            yield return new WaitForSeconds(0.5f);
        }

        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }

        OnCardSelected(topic, diceValue);
    }

    private void OnCardSelected(TopicData topic, int diceValue)
    {
        infoText.enabled = true;
        throwDiceText.enabled = false;
        var selectedCard = GetCardByDiceValue(diceValue);

        if (selectedCard == null) return;

        var rectTransform = selectedCard.GetComponent<RectTransform>();
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localPosition += cardSelectedOffset;

        coversButton.onClick.AddListener(() => ShowCardDetails(topic, diceValue));
    }

    private Image GetCardByDiceValue(int diceValue)
    {
        while (diceValue > coversParent.childCount)
        {
            diceValue -= coversParent.childCount;
        }
        
        return coversParent.GetChild(coversParent.childCount - diceValue).GetComponent<Image>();
    }

    private void ShowCardDetails(TopicData topic, int diceValue)
    {
        SetCanvasVisibility(diceRollCanvas, false);
        SetCanvasVisibility(cardInfoCanvas, true);

        cardImage.sprite = CreateSprite(topic.cardImages[diceValue - 2]);

        backToTopicSelectionButton.onClick.AddListener(ReloadScene);
    }

    private void ReloadScene() =>
        _sceneReloadOperation.allowSceneActivation = true;

    private static Sprite CreateSprite(Texture2D texture) =>
        Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

    private static void SetCanvasVisibility(Canvas canvas, bool isVisible) =>
        canvas.enabled = isVisible;
}

[Serializable]
public class TopicData
{
    public Texture2D coverImage;
    public Texture2D[] cardImages;
}

public class Dice
{
    private const float ROLL_DURATION = 0.075f;
    private const int ROLL_COUNT_MIN = 15;
    private const int ROLL_COUNT_MAX = 25;

    public static IEnumerator RollTheDice(Image image, Action<int> onDiceRolled)
    {
        var diceSides = Resources.LoadAll<Sprite>("DiceSides");
        var randomDiceSide = 0;

        var rollCount = Random.Range(ROLL_COUNT_MIN, ROLL_COUNT_MAX);

        for (var i = 0; i < rollCount; i++)
        {
            randomDiceSide = Random.Range(0, diceSides.Length);
            image.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(ROLL_DURATION - i * 0.0025f);
        }

        onDiceRolled?.Invoke(randomDiceSide + 1);
    }
}