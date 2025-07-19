using Spine.Unity;
using System.Collections;
using UnityEngine;

public class MonsterUI : MonoBehaviour
{
    public void ShowMonsterUI(EndingType endingType)
    {
        var currentMap = MapManager.Instance.CurrentMap;
        var monsters = currentMap.Monsters;

        foreach (var monster in monsters)
        {
            // 1. SkeletonDataAsset 로드
            var lowerName = monster.MonsterData.Name;
            var upperedName = char.ToUpper(lowerName[0]) + lowerName.Substring(1);
            var dataPath = $"Spine/Monster/Map_{MapManager.Instance.CurrentMapIndex}/{upperedName}/monster_{lowerName}_SkeletonData";
            var materialPath = $"Spine/Monster/Map_{MapManager.Instance.CurrentMapIndex}/{upperedName}/monster_{lowerName}_Material";
            var skeletonData = Resources.Load<SkeletonDataAsset>(dataPath);
            if (skeletonData == null)
            {
                Debug.LogError($"SkeletonData is null: {dataPath}");
                continue;
            }
            var skeletonMaterial = Resources.Load<Material>(materialPath);
            if (skeletonMaterial == null)
            {
                Debug.LogError($"Material is null: {materialPath}");
                continue;
            }

            // 2. SkeletonGraphic 컴포넌트 추가
            GameObject graphicObject = new GameObject($"{upperedName} graphic");
            var graphic = graphicObject.AddComponent<SkeletonGraphic>();
            var rectTransform = graphic.GetComponent<RectTransform>();
            rectTransform.SetParent(transform, false);
            rectTransform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            graphic.skeletonDataAsset = skeletonData;
            graphic.material = skeletonMaterial;
            graphic.UnscaledTime = true;
            graphic.color = new Color(1f, 1f, 1f, 0f);

            StartCoroutine(FadeInCo(graphic, 0.5f));

            // 3. 애니메이션 설정
            if (endingType == EndingType.Success)
            {
                graphic.startingAnimation = "disappear";
                graphic.startingLoop = false;
                graphic.timeScale = 0.5f; // 죽는 애니메이션 느리게 재생

                //var waitTime = graphic.SkeletonData.FindAnimation("disappear").Duration;
                //StartCoroutine(FadeOutCo(graphic, 2f, waitTime));
            }
            else if (endingType == EndingType.Failure)
            {
                // 초원
                // - 토끼: 당근
                // - 고양이: 감자
                // - 다람쥐: 옥수수
                // - 카피바라: 당근
                // - 코끼리: 감자

                // 해변
                // - 꽃게: 당근
                // - 오징어: 오이
                // - 복어: 감자
                // - 물개: 가지
                // - 거북이: 배추

                // 동굴
                // - 박쥐: 감자
                // - 거미: 가지
                // - 두더지: 오이
                // - 두꺼비: 고구마
                // - 뱀: 버섯

                switch (lowerName)
                {
                    // 초원
                    case "rabbit":
                        graphic.initialSkinName = "carrot";
                        break;
                    case "cat":
                        graphic.initialSkinName = "potato";
                        break;
                    case "squirrel":
                        graphic.initialSkinName = "corn";
                        break;
                    case "capybara":
                        graphic.initialSkinName = "carrot";
                        break;
                    case "elephant":
                        graphic.initialSkinName = "potato";
                        break;

                    // 해변
                    case "crab":
                        graphic.initialSkinName = "carrot";
                        break;
                    case "squid":
                        graphic.initialSkinName = "cucumber";
                        break;
                    case "blowfish":
                        graphic.initialSkinName = "potato";
                        break;
                    case "seal":
                        graphic.initialSkinName = "eggplant";
                        break;
                    case "turtle":
                        graphic.initialSkinName = "cabbage";
                        break;

                    // 동굴
                    case "bat":
                        graphic.initialSkinName = "potato";
                        break;
                    case "spider":
                        graphic.initialSkinName = "eggplant";
                        break;
                    case "mole":
                        graphic.initialSkinName = "cucumber";
                        break;
                    case "frog":
                        graphic.initialSkinName = "sweetpotato";
                        break;
                    case "snake":
                        graphic.initialSkinName = "mushroom";
                        break;
                }

                if (string.IsNullOrEmpty(graphic.initialSkinName))
                {
                    graphic.initialSkinName = currentMap.Crops[Random.Range(0, currentMap.Crops.Length)];
                }
                graphic.startingAnimation = "eating";
                graphic.startingLoop = true;
            }

            // 4. 초기화
            graphic.Initialize(true);
        }
    }
    private IEnumerator FadeInCo(SkeletonGraphic graphic, float duration, float waitTime = 0f)
    {
        yield return new WaitForSeconds(waitTime);

        var color = graphic.color;
        var eTime = 0f;
        while (eTime < duration)
        {
            var t = eTime / duration;
            color.a = Mathf.Lerp(0f, 1f, t);
            graphic.color = color;
            yield return null;
            eTime += Time.deltaTime;
        }
        color.a = 1f;
        graphic.color = color;
    }
    //private IEnumerator FadeOutCo(SkeletonGraphic graphic, float duration, float waitTime = 0f)
    //{
    //    yield return new WaitForSeconds(waitTime);

    //    var color = graphic.color;
    //    var eTime = 0f;
    //    while (eTime < duration)
    //    {
    //        var t = eTime / duration;
    //        color.a = Mathf.Lerp(1f, 0f, t);
    //        graphic.color = color;
    //        yield return null;
    //        eTime += Time.deltaTime;
    //    }
    //    color.a = 0f;
    //    graphic.color = color;
    //}
}
