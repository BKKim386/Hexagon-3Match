using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public enum Direction
    {
        NorthEast = 0,
        North = 1,
        NorthWest = 2,
        SouthWest = 3,
        South = 4,
        SouthEast = 5,
    }

    public static class HexDirection
    {
        private static Vector2Int[] _dir = new Vector2Int[]
        {
            new Vector2Int(1,0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(1, -1)
        };

        public static Vector2Int[] All => _dir;
        public static Vector2Int NorthEast = _dir[(int)Direction.NorthEast];
        public static Vector2Int North = _dir[(int)Direction.North];
        public static Vector2Int NorthWest = _dir[(int)Direction.NorthWest];
        public static Vector2Int SouthWest = _dir[(int)Direction.SouthWest];
        public static Vector2Int South = _dir[(int)Direction.South];
        public static Vector2Int SouthEast = _dir[(int)Direction.SouthEast];
    }
    
    public class HexagonGrid
    {
        private HexagonGridView _gridView;
        private HexagonBlockDataFactory _blockDataFactory;

        private Dictionary<Vector2Int, HexagonBlockData> _map;  //좌표, 블럭데이터 맵
        private Queue<Vector2Int> _emptySearchQueue;    //빈공간 탐색용 Queue
        private int _radius;
        private Vector2Int _spawnPos;
        
        public HexagonGrid()
        {
            _blockDataFactory = new HexagonBlockDataFactory();

            _map = new Dictionary<Vector2Int, HexagonBlockData>();
            _emptySearchQueue = new Queue<Vector2Int>();
            
            _radius = 0;
        }

        public IReadOnlyDictionary<Vector2Int, HexagonBlockData> Map => _map;

        public void CreateMap(int radius)
        {
            _map.Clear();
            
            _radius = radius;
            
            for (int q = -radius; q <= radius; ++q)
            {
                int rMin = Mathf.Max(-radius, -q - radius);
                int rMax = Mathf.Min(radius, -q + radius);

                for (int r = rMin; r <= rMax; ++r)
                {
                    var pos = new Vector2Int(q, r);
                                
                    _map.Add(pos, null);
                }
            }

            _spawnPos = new Vector2Int(0, radius);
            
            CreateSearchQueue();
        }

        public bool IsEmptyBlockExist()
        {
            return _map.Any(data => data.Value == null);
        }

        public HexagonBlockData SpawnNewBlock()
        {
            if (_map[_spawnPos] != null) return null;

            HexagonBlockData newBlock = _blockDataFactory.Create(Random.Range(1, 7), _spawnPos);
            _map[_spawnPos] = newBlock;

            return newBlock;
        }
        
        //빈 블럭을 탐색하는 순서를 캐싱한다
        private void CreateSearchQueue()
        {
            _emptySearchQueue.Clear();
            Vector2Int root = new Vector2Int(0, -_radius);

            int height = _radius * 2;

            for (int i = 0; i <= height; ++i)
            {
                var current = root + (HexDirection.North * i);

                if (!_map.ContainsKey(current)) return;

                //거리순으로 저장
                int distance = Mathf.Min(_radius, _radius + current.y);

                for (int j = distance; j > 0; --j)
                {
                    var left = current + HexDirection.SouthWest * j; //좌하단
                    if (!_map.ContainsKey(left)) throw new Exception($"out of map range {left}");

                    _emptySearchQueue.Enqueue(left);
                }
                
                for (int j = distance; j > 0; --j)
                {
                    var right = current + HexDirection.SouthEast * j; //우하단
                    if (!_map.ContainsKey(right)) throw new Exception($"out of map range {right}");

                    _emptySearchQueue.Enqueue(right);
                }

                //중앙
                _emptySearchQueue.Enqueue(current);
            }
        }
        
        //중력 이동 1회
        public void ProcessGravityOnce()
        {
            bool SearchUpper(Vector2Int current, out Vector2Int upper)
            {
                //상단 -> 좌상단 -> 우상단 순으로 검사
                if (_map.ContainsKey(current + HexDirection.North))
                {
                    upper = current + HexDirection.North; 
                    return true;
                }

                if (_map.ContainsKey(current + HexDirection.NorthWest))
                {
                    upper = current + HexDirection.NorthWest;
                    return true;
                }

                if (_map.ContainsKey(current + HexDirection.NorthEast))
                {
                    upper = current + HexDirection.NorthEast;
                    return true;
                }

                upper = Vector2Int.zero;
                return false;
            }
            
            Queue<Vector2Int> searchQueue = new Queue<Vector2Int>(_emptySearchQueue);

            //탐색 횟수를 현재 블록 갯수로 제한해서 쓸데없는 반복을 줄임
            int totalBlock = _map.Count(mapData => mapData.Value != null);

            for (int i = 0; i < totalBlock; ++i)
            {
                Vector2Int current = searchQueue.Dequeue();

                if (!_map.TryGetValue(current, out var data)) throw new Exception($"out of map range {current}");
                if (data != null) continue;
                
                //빈블록의 위를 탐색
                Vector2Int upperSearch = current;
                while (SearchUpper(upperSearch, out var upper))
                {
                    if (_map[upper] == null)
                    {
                        upperSearch = upper;
                        continue;
                    }

                    var path = CalculateFallDownPath(upper, current);
                    
                    var changeTarget = _map[upper];
                    _map[current] = changeTarget;
                    _map[upper] = null;
                    
                    _map[current].Move(current, path);
                    
                    break;
                }
            }
        }

        //중력이동 경로탐색
        private List<Vector2Int> CalculateFallDownPath(Vector2Int start, Vector2Int goal)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int current = start;

            while (current != goal)
            {
                var south = current + HexDirection.South;
                if (_map.ContainsKey(south) && _map[south] == null)
                {
                    path.Add(south);
                    current = south;
                    continue;
                }

                if (start.x > goal.x)
                {
                    var southWest = current + HexDirection.SouthWest;
                    if (_map.ContainsKey(southWest) && _map[southWest] == null)
                    {
                        path.Add(southWest);
                        current = southWest;
                    }
                }
                else if (start.x < goal.x)
                {
                    var southEast = current + HexDirection.SouthEast;
                    if (_map.ContainsKey(southEast) && _map[southEast] == null)
                    {
                        path.Add(southEast);
                        current = southEast;
                    }
                }
            }

            return path;
        }

        public void RemoveMatchedBlockes(List<MatchInfo> matchInfos)
        {
            for(int i = 0; i < matchInfos.Count; ++i)
            {
                for(int j = 0; j < matchInfos[i].BlockList.Count; ++j)
                {
                    var target = matchInfos[i].BlockList[j];

                    if (_map[target.Pos] == null) continue; //이미 제거됨 
                    _map[target.Pos] = null;
                    target.Remove();
                }
            }
        }

        public void Swap(Vector2Int choice, Vector2Int target)
        {
            HexagonBlockData choiceData = _map[choice];
            HexagonBlockData targetData = _map[target];

            _map[choice] = targetData;
            _map[target] = choiceData;

            choiceData.Move(target, new List<Vector2Int> { target });
            targetData.Move(choice, new List<Vector2Int> { choice });
        }
    }
}