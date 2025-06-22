using UnityEngine;

namespace Game
{
    class RocketBlockView : BlockView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public override void SetSprite(HexagonBlockData blockData)
        {
            _spriteRenderer.color = Util.HexagonBlockColor.GetColor(blockData.Color);

            if (blockData is RocketBlockData rocketData)
            {
                if(rocketData.Direction == HexagonDirection.NorthEast || rocketData.Direction == HexagonDirection.SouthWest)
                {
                    transform.rotation = Quaternion.Euler(0, 0, -60);
                }
                else if (rocketData.Direction == HexagonDirection.North || rocketData.Direction == HexagonDirection.South)
                {
                    transform.rotation = Quaternion.identity;
                }
                else if (rocketData.Direction == HexagonDirection.NorthWest || rocketData.Direction == HexagonDirection.SouthEast)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 60);
                }
            }
            else
            {
                Debug.LogError("잘못된 Data");
            }
        }
    }
}
