using UnityEditor;
using UnityEngine;
using System.Collections;
using JoonyleGameDevKit;

[CustomEditor(typeof(SortingChildren))]
public class SortingChildrenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Sort Children"))
        {
            SortingChildren t = target as SortingChildren;
            Transform parent = t.transform;

            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
            {
                children[i] = parent.GetChild(i);
            }

            // Array Ŭ������ public static void Sort<T>(T[] array, IComparer<T> comparer) �� ȣ��ȴ�
            System.Array.Sort(children, new ComparerByNumber());

            // Array Ŭ������ public static void Sort<T>(T[] array, Comparison<T> comparison) �� ȣ��ȴ�
            // System.Array.Sort(children, (x, y) => string.Compare(x.name, y.name));

            /*
            // �̸� �������� ���ĵ� Children ��ü�� �ٽ� �θ� ��ü�� �ڽ����� ����
            foreach (Transform child in children)
            {
                child.SetAsLastSibling();
            }
            */

            Debug.Log("Sorting Children");
        }
    }
}

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