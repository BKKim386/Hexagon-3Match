using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{

    public class HexagonMatchProcessor
    {
        private IReadOnlyDictionary<Vector2Int, HexagonBlockData> _map;
        private LineMatchGroup _lineMatchGroup;
        private RectangleMathGroup _rectangleMathGroup;

        public HexagonMatchProcessor(IReadOnlyDictionary<Vector2Int, HexagonBlockData> map)
        {
            _map = map;
            _lineMatchGroup = new LineMatchGroup(_map);
            _rectangleMathGroup = new RectangleMathGroup(_map);
        }

        public List<MatchInfo> SearchMatches()
        {
            var matched = new List<MatchInfo>();

            matched.AddRange(_lineMatchGroup.Search());
            matched.AddRange(_rectangleMathGroup.Search());

            matched = JoinGroups(matched);

            return matched;
        }

        //겹치는 MatchInfo를 결합
        private List<MatchInfo> JoinGroups(List<MatchInfo> matchInfos)
        {
            List<MatchInfo> ret = new List<MatchInfo>();

            Dictionary<BlockColor, List<MatchInfo>> colorGroup = new Dictionary<BlockColor, List<MatchInfo>>()
            {
                { BlockColor.Blue, new List<MatchInfo>() },
                { BlockColor.Green, new List<MatchInfo>() },
                { BlockColor.Orange, new List<MatchInfo>() },
                { BlockColor.Purple, new List<MatchInfo>() },
                { BlockColor.Red, new List<MatchInfo>() },
                { BlockColor.Yellow, new List<MatchInfo>() },
            };

            for(int i = 0; i < matchInfos.Count; ++i)
            {
                colorGroup[matchInfos[i].GetColor()].Add(matchInfos[i]);
            }

            foreach(var group in colorGroup)
            {
                List<List<MatchInfo>> intersectGroups = new List<List<MatchInfo>>();
                List<MatchInfo> infoList = group.Value;

                //1. 겹치는 Match끼리 모음
                for (int i = 0; i < infoList.Count; ++i)
                {
                    List<Vector2Int> currentBlockPos = infoList[i].BlockList.Select(data => data.Pos).ToList();

                    List<MatchInfo> matchedList = intersectGroups.FirstOrDefault(g => g.Any(exist => exist.BlockList.Select(data => data.Pos).Intersect(currentBlockPos).Any()));

                    if(matchedList != null)
                    {
                        matchedList.Add(infoList[i]);
                    }
                    else
                    {
                        intersectGroups.Add(new List<MatchInfo>() { infoList[i] });
                    }
                }

                List<MatchInfo> retList = new List<MatchInfo>();

                //2. 하나의 Match로 통합
                for(int i = 0; i < intersectGroups.Count; ++i)
                {
                    List<HexagonBlockData> blockList = new List<HexagonBlockData>();
                    MatchType matchType = intersectGroups[i].Max(matchInfo => matchInfo.Type);

                    for(int j = 0; j < intersectGroups[i].Count; ++j)
                    {
                        blockList.AddRange(intersectGroups[i][j].BlockList);
                    }

                    blockList = blockList.Distinct().ToList();

                    MatchInfo joinedInfo = new MatchInfo(matchType, blockList);

                    ret.Add(joinedInfo);
                }
            }

            return ret;
        }
    }

    public abstract class MatchGroup
    {
        protected IReadOnlyDictionary<Vector2Int, HexagonBlockData> _map;
        protected List<MatchInfo> _matchList;

        public MatchGroup(IReadOnlyDictionary<Vector2Int, HexagonBlockData> map)
        {
            _map = map;
            _matchList = new List<MatchInfo>();
        }

        public abstract List<MatchInfo> Search();
        
        protected bool IsValidBlock(Vector2Int pos)
        {
            return _map.ContainsKey(pos) && _map[pos] != null;
        }
    }

    //일자 패턴
    public class LineMatchGroup : MatchGroup
    {

        public LineMatchGroup(IReadOnlyDictionary<Vector2Int, HexagonBlockData> map) : base(map)
        {
        }

        public override List<MatchInfo> Search()
        {
            _matchList.Clear();

            _matchList.AddRange(MatchLine(HexagonDirection.NorthEast, HexagonDirection.SouthWest));
            _matchList.AddRange(MatchLine(HexagonDirection.North, HexagonDirection.South));
            _matchList.AddRange(MatchLine(HexagonDirection.NorthWest, HexagonDirection.SouthEast));

            return _matchList;   
        }

        private List<MatchInfo> MatchLine(Vector2Int dirForward, Vector2Int dirBackward)
        {
            List<MatchInfo> matchInfoList = new List<MatchInfo>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach(var blockData in _map)
            {
                Vector2Int currentPos = blockData.Key;
                HexagonBlockData startData = blockData.Value;

                if (startData == null || visited.Contains(currentPos)) continue;

                //축 후방 검사 - 후방에 같은 블럭이 있으면 후방으로 처리를 미룸
                Vector2Int backPos = currentPos + dirBackward;
                if(IsValidBlock(backPos) && _map[backPos].Color == startData.Color)
                {
                    continue;
                }

                List<HexagonBlockData> matchBlockList = new List<HexagonBlockData>();
                matchBlockList.Add(startData);
                visited.Add(currentPos);

                while(matchBlockList.Count > 0)
                {
                    Vector2Int forPos = currentPos + dirForward;
                    if(_map.ContainsKey(forPos) && _map[forPos] != null && _map[forPos].Color == startData.Color)
                    {
                        matchBlockList.Add(_map[forPos]);
                        visited.Add(forPos);
                        currentPos = forPos;
                    }
                    else
                    {
                        break;
                    }
                }

                if (matchBlockList.Count == 3)
                {
                    matchInfoList.Add(new MatchInfo(MatchType.Line_Three, matchBlockList));
                }
                else if (matchBlockList.Count == 4)
                {
                    matchInfoList.Add(new MatchInfo(MatchType.Line_Four, matchBlockList));
                }
                else if (matchBlockList.Count >= 5)
                {
                    matchInfoList.Add(new MatchInfo(MatchType.Line_Five, matchBlockList));
                }
            }

            return matchInfoList;
        }
    }

    //사각형(4칸)
    public class RectangleMathGroup : MatchGroup
    {
        public RectangleMathGroup(IReadOnlyDictionary<Vector2Int, HexagonBlockData> map) : base(map)
        {

        }

        public override List<MatchInfo> Search()
        {
            _matchList.Clear();

            _matchList.AddRange(MatchRectangle(HexagonDirection.NorthEast, HexagonDirection.SouthEast));
            _matchList.AddRange(MatchRectangle(HexagonDirection.SouthEast, HexagonDirection.South));
            _matchList.AddRange(MatchRectangle(HexagonDirection.South, HexagonDirection.SouthWest));

            return _matchList;
        }

        private List<MatchInfo> MatchRectangle(Vector2Int dirA, Vector2Int dirB)
        {
            List<MatchInfo> matchInfoList = new List<MatchInfo>();

            foreach(var blockData in _map)
            {
                Vector2Int currentPos = blockData.Key;
                HexagonBlockData currentData = blockData.Value;

                if (!IsValidBlock(currentPos)) continue;

                Vector2Int forwardA = currentPos + dirA;
                Vector2Int forwardB = currentPos + dirB;
                Vector2Int forwardAB = currentPos + dirA + dirB;

                if(IsValidBlock(forwardA) && _map[forwardA].Color == currentData.Color &&
                    IsValidBlock(forwardB) && _map[forwardB].Color == currentData.Color &&
                    IsValidBlock(forwardAB) && _map[forwardAB].Color == currentData.Color)
                {
                    List<HexagonBlockData> matchList = new List<HexagonBlockData>()
                    {
                        currentData, _map[forwardA], _map[forwardB], _map[forwardAB]
                    };

                    if(!AlreadyContainsSameSet(matchInfoList, matchList))
                    {
                        matchInfoList.Add(new MatchInfo(MatchType.Rectangle, matchList));
                    }
                }
            }

            return matchInfoList;
        }

        private bool AlreadyContainsSameSet(List<MatchInfo> groups, List<HexagonBlockData> candidate)
        {
            var candSet = new HashSet<Vector2Int>(candidate.ConvertAll(t => t.Pos));
            foreach (var group in groups)
            {
                var grpSet = new HashSet<Vector2Int>(group.BlockList.ConvertAll(t => t.Pos));
                if (grpSet.SetEquals(candSet))
                    return true;
            }
            return false;
        }
    }
}
