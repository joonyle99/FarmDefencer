using Com.LuisPedroFonseca.ProCamera2D;
using System;
using UnityEngine;

/// <summary>
/// ���� �����ϴ� ��� ������Ʈ���� �����ϴ� ���ؽ�Ʈ�Դϴ�.
/// </summary>
public class SceneContext : JoonyleGameDevKit.Singleton<SceneContext>
{
    public static SceneContext Current
    {
        get
        {
            if (Instance == null)
            {
                throw new NullReferenceException("The SceneContext Instance is null.");
            }

            return Instance;
        }
    }

    // comman reference
    public ProCamera2D ProCamera2D;

    protected override void Awake()
    {
        base.Awake();

        ProCamera2D = FindFirstObjectByType<ProCamera2D>();
    }
    protected virtual void Start()
    {

    }
}
