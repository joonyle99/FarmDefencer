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
            var lowerName = monster.MonsterName;
            var upperedName = char.ToUpper(lowerName[0]) + lowerName.Substring(1);
            var dataPath = $"Spine/Monster/{upperedName}/monster_{lowerName}_SkeletonData";
            var materialPath = $"Spine/Monster/{upperedName}/monster_{lowerName}_Material";
            var skeletonData = Resources.Load<SkeletonDataAsset>(dataPath);
            var skeletonMaterial = Resources.Load<Material>(materialPath);

            // 2. SkeletonGraphic 컴포넌트 추가
            GameObject graphicObject = new GameObject($"{upperedName} graphic");
            var graphic = graphicObject.AddComponent<SkeletonGraphic>();
            var rectTransform = graphic.GetComponent<RectTransform>();
            rectTransform.SetParent(transform, false);
            rectTransform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            graphic.skeletonDataAsset = skeletonData;
            graphic.material = skeletonMaterial;

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
                graphic.initialSkinName = currentMap.Crops[Random.Range(0, currentMap.Crops.Length)];
                graphic.startingAnimation = "eating";
                graphic.startingLoop = true;
            }

            // 4. 초기화
            graphic.Initialize(true);
        }
    }
    private IEnumerator FadeOutCo(SkeletonGraphic graphic, float duration, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        var color = graphic.color;
        var eTime = 0f;
        while (eTime < duration)
        {
            var t = eTime / duration;
            color.a = Mathf.Lerp(1f, 0f, t);
            graphic.color = color;
            yield return null;
            eTime += Time.deltaTime;
        }
        color.a = 0f;
        graphic.color = color;
    }
}
