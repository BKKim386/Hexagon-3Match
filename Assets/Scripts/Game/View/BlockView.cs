using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class BlockView : MonoBehaviour
    {
        [SerializeField]
        private int speed;
        
        public void Move(Vector2Int start, List<Vector2Int> path)
        {
            transform.localPosition = Util.AxialToWorldPos(start, HexagonGridView.HexagonRadius);

            StartCoroutine(MoveAsync(path));
        }

        private IEnumerator MoveAsync(List<Vector2Int> path)
        {
            for (int i = 0; i < path.Count; ++i)
            {
                var targetPos = Util.AxialToWorldPos(path[i], HexagonGridView.HexagonRadius);                

                while (Vector3.Distance(transform.localPosition, targetPos) > 0.0001f)
                {
                    transform.localPosition = Vector3.MoveTowards(
                        transform.localPosition,
                        targetPos,
                        speed * Time.deltaTime * HexagonGridView.HexagonRadius
                    );

                    yield return null;
                }

                transform.localPosition = targetPos;
            }
            
            yield break;
        }
    }
}