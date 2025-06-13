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

        private List<Vector3> _centers = new List<Vector3>();

        //private List<SpriteRenderer> _backgroundSprites = new List<SpriteRenderer>();


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

        public NormalBlockView InstantiateBlock(HexagonBlockData data)
        {
            NormalBlockView newInstance = Instantiate(NormalPrefab, transform);
            newInstance.transform.localScale = Vector3.one;
            newInstance.transform.localPosition = Util.AxialToWorldPos(data.Pos, HexagonRadius);

            newInstance.SetSprite(data.Color);

            return newInstance;
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
    }
}