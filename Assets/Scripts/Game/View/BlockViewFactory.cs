using System;
using UnityEngine;

namespace Game
{
    class BlockViewFactory
    {
        private Transform _parents;
        private string[] _paths = new string[]
        {
            "Prefab/NormalBlock",
            "Prefab/RocketBlock",
            "Prefab/BoomerangBlock",
            "Prefab/BoomerangBlock",
        };

        public BlockViewFactory(Transform parents)
        {
            _parents = parents;
        }

        public BlockView Create(HexagonBlockData blockData)
        {
            GameObject prefab = Resources.Load<GameObject>(_paths[(int)blockData.Type]);
            var instance = UnityEngine.Object.Instantiate(prefab, _parents);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale= Vector3.one;
            instance.transform.localRotation = Quaternion.identity;

            if (instance.TryGetComponent<BlockView>(out var blockView))
            {
                blockView.SetSprite(blockData);
                return blockView;
            }
            else
            {
                throw new Exception($"{_paths[(int)blockData.Type]} not contains view components");
            }
        }
    }
}
