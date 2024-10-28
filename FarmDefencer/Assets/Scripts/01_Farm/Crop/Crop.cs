using UnityEngine;

/// <summary>
/// �۹� �� ������ �ǹ��ϴ� �߻� Ŭ�����Դϴ�.
/// ��ġ�� ���� ������ �������� ����ϸ�, �ɱ�� ��Ȯ�ϱ��� ������ �׼��� �������� �ʿ�� �մϴ�.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Crop : MonoBehaviour
{
    /// <summary>
    /// �� Crop�� �ش� ��ǥ�� �ش��ϴ��� Ȯ���մϴ�.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

    /// <summary>
    /// �۹��� �ɰ� ���� �ֱ⿡ �����մϴ�.
    /// �۹� ������ ���� ���� �� �ִ� ������ �ٸ� �� �ֽ��ϴ�.
    /// </summary>
    /// <returns>��� ������ ������ �۹��� ���� ��� true, �̿��� ��� false</returns>
    public abstract bool TryPlant();

    /// <summary>
    /// �۹��� ��Ȯ�մϴ�.
    /// �۹� ������ ��Ȯ ������ �ٸ� �� �ֽ��ϴ�.
    /// </summary>
    /// <returns>��� ������ ������ �۹��� ��Ȯ�� ��� true, �̿��� ��� false</returns>
    public abstract bool TryHarvest();

    /// <summary>
    /// �۹��� �����ŵ�ϴ�.
    /// �۹� ������ ���� ���� ������ �ٸ� �� �ֽ��ϴ�.
    /// </summary>
    /// <returns>��� ������ ������ �۹��� �����Ų ��� true, �̿��� ��� false</returns>
    public abstract bool TryGrowUp();
}