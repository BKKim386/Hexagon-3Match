using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public enum MatchType   //매칭 아이템 생성 우선순위(내림차순)
    {
        None = 0,
        Line_Three = 1,
        Rectangle = 2,
        Line_Four = 3,
        Line_Five = 4
    }

    public class MatchInfo
    {
        public MatchType Type;
        public List<HexagonBlockData> BlockList;

        public MatchInfo(MatchType type, List<HexagonBlockData> blockList)
        {
            Type = type;
            BlockList = blockList;
        }

        public BlockColor GetColor()
        {
            if (BlockList.Count == 0)
            {
                return BlockColor.None;
            }
            else
            {
                return BlockList[0].Color;
            }
        }

        public Vector2Int GetCenter()
        {
            int center = BlockList.Count / 2;

            return BlockList[center].Pos;
        }
    }
}
