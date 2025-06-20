using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class HexagonGameManager : MonoBehaviour
    {
        [SerializeField] private HexagonGridView _gridView;
        [SerializeField] private int Radius;
        
        private HexagonGrid _grid;
        private HexagonMatchProcessor _matchProcessor;

        private Camera _camera;

        private void Start()
        {
            _grid = new HexagonGrid(_gridView);
            _grid.CreateMap(Radius);
            _gridView.CreateMapBackground(_grid.Map.Keys.ToList());

            _matchProcessor = new HexagonMatchProcessor(_grid.Map);

            _camera = Camera.main;

            StartCoroutine(CoGameSequnce());
        }

        private IEnumerator CoGameSequnce()
        {
            while(true)
            {
                yield return StartCoroutine(CoFillGrid());

                var matches = _matchProcessor.SearchMatches();

                if (matches.Count > 0)
                {
                    _grid.RemoveMatchedBlockes(matches);
                    continue;
                }
                else
                {
                    bool attractInput = false;
                    Vector3? start = null;

                    while(true)
                    {
                        if(Input.GetMouseButtonDown(0) && attractInput == false)
                        {
                            if(_gridView.GetCloseBlock(_camera.ScreenToWorldPoint(Input.mousePosition), out start))
                            {
                                attractInput = true;
                            }
                        }

                        if(!Input.GetMouseButton(0))
                        {
                            start = Vector3.zero;
                            attractInput = false;
                        }
                        
                        if(attractInput)
                        {
                            if(_gridView.GetCloseBlock(_camera.ScreenToWorldPoint(Input.mousePosition), out var current))
                            {
                                //mousePosition이 확 튈 때를 방지
                                if(Vector3.Distance(start.Value, current.Value) > HexagonGridView.HexagonRadius * 2f)
                                {
                                    var dir = (current.Value - start.Value).normalized;

                                    _gridView.GetCloseBlock(start.Value + dir * HexagonGridView.HexagonRadius * 1.8f, out current);
                                }

                                //스왑
                                if(current != start)
                                {
                                    var startAxial = Util.WorldToAxialPos(start.Value, HexagonGridView.HexagonRadius);
                                    var currentAxial = Util.WorldToAxialPos(current.Value, HexagonGridView.HexagonRadius);

                                    _grid.Swap(startAxial, currentAxial);

                                    yield return YieldInstance.WaitForSeconds(0.2f);

                                    var swapMatches = _matchProcessor.SearchMatches();

                                    if(swapMatches.Count > 0)
                                    {
                                        _grid.RemoveMatchedBlockes(matches);
                                        break;
                                    }
                                    else
                                    {
                                        _grid.Swap(startAxial, currentAxial);
                                        yield return YieldInstance.WaitForSeconds(0.2f);
                                    }

                                    start = null;
                                    attractInput = false;
                                }
                            }
                        }
                        
                        yield return null;
                    }
                }
            }
        }

        private IEnumerator CoFillGrid()
        {
            while (_grid.IsEmptyBlockExist())
            {
                _grid.SpawnNewBlock(UnityEngine.Random.Range(1, 7));
                _grid.ProcessGravityOnce();
                yield return YieldInstance.WaitForSeconds(0.2f);
            }

            yield break;
        }
    }
}