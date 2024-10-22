using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Crop : MonoBehaviour
{
	public Sprite Sprite;

	private SpriteRenderer _spriteRenderer;

	/// <summary>
	/// �� Crop�� �ش� ��ǥ�� �ش��ϴ��� Ȯ���մϴ�.
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_spriteRenderer.sprite = Sprite;
	}
}