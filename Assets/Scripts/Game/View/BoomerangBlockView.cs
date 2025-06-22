using UnityEngine;

namespace Game
{
    class BoomerangBlockView : BlockView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public override void SetSprite(HexagonBlockData blockData)
        {
            _spriteRenderer.color = Util.HexagonBlockColor.GetColor(blockData.Color);
        }
    }
}
