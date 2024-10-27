using UnityEngine;
using System.Collections;
using JoonyleGameDevKit;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SortingChildren : MonoBehaviour
{
    /*
     * 
     * Generic Test
     * 
    private void Start()
    {
        // var obj = GetObject(this);

        // this: SortingChildren class
        // obj: SortingChildren class

        // var obj2 = GetObject2<GameObject>();
    }

    public T GetObject<T>(T obj)
    {
        return obj;
    }
    public T GetObject2<T>()
    {
        return default(T);
    }
    */
}

#if UNITY_EDITOR
/// <summary>
/// Transform 객체를 이름 기준으로 정렬하기 위한 비교자 클래스
/// </summary>
public class ComparerByName : IComparer
{
    public int Compare(object x, object y)
    {
        Transform transformX = x as Transform;
        Transform transformY = y as Transform;

        return string.Compare(transformX.name, transformY.name);
    }
}

/// <summary>
/// Transform 객체를 번호 기준으로 정렬하기 위한 비교자 클래스
/// </summary>
public class ComparerByNumber : IComparer
{
    /// <summary>
    /// -1, 0, 1 중 하나를 반환한다
    /// </summary>
    /// <remarks>
    /// -1: x가 y보다 작다
    /// 0: x와 y가 같다
    /// 1: x가 y보다 크다
    /// </remarks>
    public int Compare(object x, object y)
    {
        Transform gameObjectX = x as Transform;
        Transform gameObjectY = y as Transform;

        int numberX = gameObjectX.name.ExtractNumber();
        int numberY = gameObjectY.name.ExtractNumber();

        // return numberX.CompareTo(numberY);

        if (numberX < numberY)
            return -1;
        else if (numberX > numberY)
            return 1;

        return 0;
    }
}

[CustomEditor(typeof(SortingChildren))]
public class SortingChildrenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Sort Children"))
        {
            SortingChildren t = target as SortingChildren;
            Transform trans = t.transform;

            Transform[] children = new Transform[trans.childCount];
            for (int i = 0; i < trans.childCount; i++)
            {
                children[i] = trans.GetChild(i);
            }

            // Array 클래스의 public static void Sort<T>(T[] array, IComparer<T> comparer) 이 호출된다
            System.Array.Sort(children, new ComparerByNumber());

            // Array 클래스의 public static void Sort<T>(T[] array, Comparison<T> comparison) 이 호출된다
            // System.Array.Sort(children, (x, y) => string.Compare(x.name, y.name));

            // 이름 기준으로 정렬되 Children 객체를 다시 부모 객체의 자식으로 설정
            foreach (Transform child in children)
            {
                child.SetAsLastSibling();
            }

            Debug.Log("Sorting Children");
        }
    }
}
#endif