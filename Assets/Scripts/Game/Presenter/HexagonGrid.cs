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

    public static class HexagonDirection
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
        public readonly static Vector2Int NorthEast = _dir[(int)Direction.NorthEast];
        public readonly static Vector2Int North = _dir[(int)Direction.North];
        public readonly static Vector2Int NorthWest = _dir[(int)Direction.NorthWest];
        public readonly static Vector2Int SouthWest = _dir[(int)Direction.SouthWest];
        public readonly static Vector2Int South = _dir[(int)Direction.South];
        public readonly static Vector2Int SouthEast = _dir[(int)Direction.SouthEast];
    }
    
    public class HexagonGrid
    {
        private HexagonGridView _gridView;
        private HexagonBlockDataFactory _blockDataFactory;

        private Dictionary<Vector2Int, HexagonBlockData> _map;  //좌표, 블럭데이터 맵
        private Queue<Vector2Int> _emptySearchQueue;    //빈공간 탐색용 Queue
        private int _radius;
        private Vector2Int _spawnPos;
        private Vector2Int[] _swapRecord;


        public HexagonGrid(HexagonGridView view)
        {
            _gridView = view;

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

        public HexagonBlockData SpawnNewBlock(int id)
        {
            if (_map[_spawnPos] != null) return null;

            HexagonBlockData newBlock = _blockDataFactory.Create(id, _spawnPos);
            _map[_spawnPos] = newBlock;

            _gridView.InstantiateBlock(newBlock);

            return newBlock;
        }

        public HexagonBlockData SpawnAtMatched(Vector2Int pos, int id, MatchInfo matchInfo)
        {
            if (_map[pos] != null) return null;

            HexagonBlockData newBlock = _blockDataFactory.Create(id, pos, matchInfo);
            _map[pos] = newBlock;

            _gridView.InstantiateBlock(newBlock);

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
                var current = root + (HexagonDirection.North * i);

                if (!_map.ContainsKey(current)) return;

                //거리순으로 저장
                int distance = Mathf.Min(_radius, _radius + current.y);

                for (int j = distance; j > 0; --j)
                {
                    var left = current + HexagonDirection.SouthWest * j; //좌하단
                    if (!_map.ContainsKey(left)) throw new Exception($"out of map range {left}");

                    _emptySearchQueue.Enqueue(left);
                }
                
                for (int j = distance; j > 0; --j)
                {
                    var right = current + HexagonDirection.SouthEast * j; //우하단
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
                if (_map.ContainsKey(current + HexagonDirection.North))
                {
                    upper = current + HexagonDirection.North; 
                    return true;
                }

                if (_map.ContainsKey(current + HexagonDirection.NorthWest))
                {
                    upper = current + HexagonDirection.NorthWest;
                    return true;
                }

                if (_map.ContainsKey(current + HexagonDirection.NorthEast))
                {
                    upper = current + HexagonDirection.NorthEast;
                    return true;
                }

                upper = Vector2Int.zero;
                return false;
            }
            
            Queue<Vector2Int> searchQueue = new Queue<Vector2Int>(_emptySearchQueue);

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

                    MoveBlock(changeTarget, current, path);
                    
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
                var south = current + HexagonDirection.South;
                if (_map.ContainsKey(south) && _map[south] == null)
                {
                    path.Add(south);
                    current = south;
                    continue;
                }
                else if (start.x > goal.x)
                {
                    var southWest = current + HexagonDirection.SouthWest;
                    if (_map.ContainsKey(southWest) && _map[southWest] == null)
                    {
                        path.Add(southWest);
                        current = southWest;
                    }
                    else
                    {
                        Debug.LogError($"Path not found: Start: {start} Goal: {goal}");
                        break;
                    }
                }
                else if (start.x < goal.x)
                {
                    var southEast = current + HexagonDirection.SouthEast;
                    if (_map.ContainsKey(southEast) && _map[southEast] == null)
                    {
                        path.Add(southEast);
                        current = southEast;
                    }
                    else
                    {
                        Debug.LogError($"Path not found: Start: {start} Goal: {goal}");
                        break;
                    }
                }
                else
                {
                    Debug.LogError($"Path not found: Start: {start} Goal: {goal}");
                    break;
                }
            }

            return path;
        }

        public void RemoveBlockes(List<HexagonBlockData> blockList)
        {
            for(int i = 0; i < blockList.Count; ++i)
            {
                var data = blockList[i];
                blockList[i] = null;

                RemoveBlock(data);
            }
        }

        public void CreateItemBlock(List<MatchInfo> matchInfos)
        {
            Vector2Int GetSpawnPos(MatchInfo info)
            {
                if (_swapRecord != null)
                {
                    //스왑으로 움직인 블럭부터 체크
                    var intersect = info.BlockList.Select(data => data.Pos).Intersect(_swapRecord).ToArray();

                    if (intersect.Length > 0)
                    {
                        return intersect[0];
                    }

                    return info.GetCenter();
                }
                else
                {
                    return info.GetCenter();
                }
            }


            for (int i = 0; i < matchInfos.Count; ++i)
            {
                MatchInfo info = matchInfos[i];
                if (info.Type <= MatchType.Line_Three) continue;

                Vector2Int pos = GetSpawnPos(info);

                int id = (int)info.GetColor();

                switch (info.Type)
                {
                    case MatchType.Rectangle:
                        id += (int)BlockType.ItemBoomerang * 10;
                        break;
                    case MatchType.Line_Four:
                    case MatchType.Line_Five:
                        id += (int)BlockType.ItemRocket * 10;
                        break;
                }

                SpawnAtMatched(pos, id, matchInfos[i]);
            }
        }

        public List<HexagonBlockData> GetItemEffectedBlocks(List<HexagonBlockData> removeList)
        {
            List<HexagonBlockData> effectedBlocks = new List<HexagonBlockData>();
            List<IItemBlock> itemBlocks = new List<IItemBlock>();

            for(int i = 0; i < removeList.Count; ++i)
            {
                itemBlocks.AddRange(removeList.Where(data => data is IItemBlock).Select(itemBlockData => itemBlockData as IItemBlock));
            }

            for (int i = 0; i < itemBlocks.Count; ++i)
            {
                effectedBlocks.AddRange(itemBlocks[i].ProcessItem(_map));
            }

            effectedBlocks = effectedBlocks.Distinct().ToList();

            return effectedBlocks;
        }


        public void Swap(Vector2Int chosen, Vector2Int target)
        {
            HexagonBlockData chosenData = _map[chosen];
            HexagonBlockData targetData = _map[target];

            _map[chosen] = targetData;
            _map[target] = chosenData;

            MoveBlock(targetData, chosen, new List<Vector2Int> { chosen });
            MoveBlock(chosenData, target, new List<Vector2Int> { target });

            _swapRecord = new Vector2Int[] { chosen, target };
        }

        private void MoveBlock(HexagonBlockData data, Vector2Int goal, List<Vector2Int> path)
        {
            Vector2Int prev = data.Pos;

            data.Move(goal);
            _gridView.MoveBlock(data, prev, path);
        }

        private void RemoveBlock(HexagonBlockData data)
        {
            if(_map.ContainsKey(data.Pos))
            {
                _map[data.Pos] = null;
                _gridView.RemoveBlock(data);
            }
        }


        public void ClearSwapInfo()
        {
            _swapRecord = null;
        }
    }
}