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
/// Transform ��ü�� �̸� �������� �����ϱ� ���� ���� Ŭ����
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
/// Transform ��ü�� ��ȣ �������� �����ϱ� ���� ���� Ŭ����
/// </summary>
public class ComparerByNumber : IComparer
{
    /// <summary>
    /// -1, 0, 1 �� �ϳ��� ��ȯ�Ѵ�
    /// </summary>
    /// <remarks>
    /// -1: x�� y���� �۴�
    /// 0: x�� y�� ����
    /// 1: x�� y���� ũ��
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

            // Array Ŭ������ public static void Sort<T>(T[] array, IComparer<T> comparer) �� ȣ��ȴ�
            System.Array.Sort(children, new ComparerByNumber());

            // Array Ŭ������ public static void Sort<T>(T[] array, Comparison<T> comparison) �� ȣ��ȴ�
            // System.Array.Sort(children, (x, y) => string.Compare(x.name, y.name));

            // �̸� �������� ���ĵ� Children ��ü�� �ٽ� �θ� ��ü�� �ڽ����� ����
            foreach (Transform child in children)
            {
                child.SetAsLastSibling();
            }

            Debug.Log("Sorting Children");
        }
    }
}
#endif