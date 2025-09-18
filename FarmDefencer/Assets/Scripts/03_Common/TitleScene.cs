using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public sealed class TitleScene : MonoBehaviour
{
    private class MonsterData
    {
        public SkeletonGraphic SkeletonGraphic;
        public RectTransform RectTransform;
        public Vector2 DestinationAnchoredPosition;
        public bool IsLeft;
    }
    
    [Header("배경 배율")] [SerializeField] private float backgroundScale = 1.5f;
    [Header("배경 이동 거리(x)")] [SerializeField] private float backgroundMovement = 200.0f;
    [Header("타이틀 낮 길이(초)")] [SerializeField] private float dayLength = 0.5f;
    [Header("타이틀 밤 길이(초)")] [SerializeField] private float nightLength = 1.5f;
    [Header("몬스터 도착 시간(초)")] [SerializeField] private float monsterArriveTime = 2.0f;
    
    private Image _background;
    private Image _blur;
    private Button _gameStartButton;
    private float _elapsedTime;
    private float _monsterScreenOutsideDistance;

    private List<MonsterData> _monsterDatas;
    
    private void Awake()
    {
        _monsterDatas = new List<MonsterData>();
        _background = transform.Find("Background").GetComponent<Image>();
        _blur = transform.Find("Blur").GetComponent<Image>();
        _gameStartButton = transform.Find("GameStartButton").GetComponent<Button>();
        _gameStartButton.onClick.AddListener(OnGameStartButtonPressed);
    }

    private void Start()
    {
        _monsterScreenOutsideDistance = Screen.width * 2.0f;
        
        var backgroundSprite = _background.sprite;
        var backgroundSpriteAspect = backgroundSprite.rect.width / backgroundSprite.rect.height;
        var desiredBackgroundWidth = Mathf.Max(Screen.width, Screen.height * backgroundSpriteAspect) * backgroundScale;
        _background.preserveAspect = true;

        var newBackgroundSizeDelta = _background.rectTransform.sizeDelta;
        newBackgroundSizeDelta.x = desiredBackgroundWidth;
        _background.rectTransform.sizeDelta = newBackgroundSizeDelta;

        var monstersRootObject = transform.Find("Monsters").gameObject;
        for (var i = 0; i < monstersRootObject.transform.childCount; ++i)
        {
            var monsterObject = monstersRootObject.transform.GetChild(i).gameObject;
            var rectTransform = monsterObject.GetComponent<RectTransform>();
            var skeletonGraphic = monsterObject.GetComponent<SkeletonGraphic>();

            var idleAnimationName = skeletonGraphic.SkeletonData.Animations.Any(c => c.Name.Equals("idle_title"))
                ? "idle_title"
                : "idle";
            skeletonGraphic.AnimationState.AddAnimation(0, "title", false, monsterArriveTime);
            skeletonGraphic.AnimationState.AddAnimation(0, idleAnimationName, true, 0.0f);
            
            var monsterData = new MonsterData();
            monsterData.SkeletonGraphic = skeletonGraphic;
            monsterData.RectTransform = rectTransform;
            monsterData.DestinationAnchoredPosition = rectTransform.anchoredPosition;
            monsterData.IsLeft = skeletonGraphic.initialFlipX;
            _monsterDatas.Add(monsterData);
        }
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        var backgroundPosition = _background.rectTransform.anchoredPosition;
        backgroundPosition.x = Mathf.Lerp(backgroundMovement / 2.0f, -backgroundMovement / 2.0f,
            Mathf.Clamp01(_elapsedTime / (dayLength + nightLength)));
        _background.rectTransform.anchoredPosition = backgroundPosition;
        
        var colorDay = new Vector3(Color.white.r, Color.white.g, Color.white.b);
        var colorNight = new Vector3(Color.black.r, Color.black.g, Color.black.b);
        var alpha = 0.7f;
        var lerpedColor = Vector3.Lerp(colorDay, colorNight, Mathf.Atan((_elapsedTime - dayLength) * 2.0f));
        _blur.color = new Color(lerpedColor.x, lerpedColor.y, lerpedColor.z, alpha);
        
        foreach (var monsterData in _monsterDatas)
        {
            var beginX = monsterData.DestinationAnchoredPosition.x + _monsterScreenOutsideDistance * (monsterData.IsLeft ? 1.0f : -1.0f);
            var endX = monsterData.DestinationAnchoredPosition.x;
            var lerpedX = Mathf.Lerp(beginX, endX, _elapsedTime / monsterArriveTime);

            var anchoredPosition = monsterData.DestinationAnchoredPosition;
            anchoredPosition.x = lerpedX;
            monsterData.RectTransform.anchoredPosition = anchoredPosition;
        }
    }

    private void OnGameStartButtonPressed()
    {
        SoundManager.Instance.PlaySfx("SFX_C_ui", SoundManager.Instance.uiVolume);
        SceneChangeManager.Instance.ChangeScene(SceneType.Main);
    }
}
