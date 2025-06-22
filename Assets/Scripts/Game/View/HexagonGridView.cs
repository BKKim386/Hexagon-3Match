using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using System.Linq;

namespace Game
{
    public class HexagonGridView : MonoBehaviour
    {
        public static int HexagonRadius = 36;
        
        [SerializeField] private GameObject BackgroundPrefab;
        [SerializeField] private NormalBlockView NormalPrefab;

        private BlockViewFactory _blockViewFactory;
        private Dictionary<HexagonBlockData, BlockView> _blockViewMap = new Dictionary<HexagonBlockData, BlockView>();
        private List<Vector3> _centers = new List<Vector3>();

        public IReadOnlyDictionary<HexagonBlockData, BlockView> BlockMap => _blockViewMap;

        private void Awake()
        {
            _blockViewFactory = new BlockViewFactory(transform);
        }

        public void CreateMapBackground(List<Vector2Int> posList)
        {
            foreach (var pos in posList)
            {
                GameObject newInstance = InstantiateBackground();
                newInstance.transform.localPosition = Util.AxialToWorldPos(pos, HexagonRadius);

                _centers.Add(newInstance.transform.localPosition);
            }
        }

        private GameObject InstantiateBackground()
        {
            GameObject newInstance = Instantiate(BackgroundPrefab, transform);
            newInstance.transform.localScale = Vector3.one;
            newInstance.transform.localPosition = Vector3.zero;

            return newInstance;
        }

        public BlockView InstantiateBlock(HexagonBlockData data)
        {
            if(_blockViewMap.ContainsKey(data))
            {
                return _blockViewMap[data];
            }
            else
            {
                BlockView newInstance = _blockViewFactory.Create(data);
                newInstance.transform.localPosition = Util.AxialToWorldPos(data.Pos, HexagonRadius);

                _blockViewMap.Add(data, newInstance);

                return newInstance;
            }
        }

        public bool GetCloseBlock(Vector3 from, out Vector3? pos)
        {
            pos = null;
            from = new Vector3(from.x, from.y, 0);
            Vector3[] nearPoints = _centers.Where(v3 => Vector3.Distance(from, v3) <= HexagonRadius).OrderBy(v3 => Vector3.Distance(from, v3)).ToArray();

            if (nearPoints.Length > 0)
            {
                pos = nearPoints[0];
                return true;
            }
            
            return false;
        }

        public void MoveBlock(HexagonBlockData data, Vector2Int prev, List<Vector2Int> path)
        {
            if (_blockViewMap.TryGetValue(data, out var instance))
            {
                instance.Move(prev, path);
            }
        }

        public void RemoveBlock(HexagonBlockData data)
        {
            if(_blockViewMap.TryGetValue(data, out var instance))
            {
                _blockViewMap.Remove(data);
                instance.Remove();
            }
        }
    }
}