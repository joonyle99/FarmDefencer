using Com.LuisPedroFonseca.ProCamera2D;
using System;

/// <summary>
/// 씬에 존재하는 쥬요 오브젝트들을 관리하는 컨텍스트입니다.
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
