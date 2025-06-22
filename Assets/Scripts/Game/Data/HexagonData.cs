using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Game
{
    public enum BlockColor
    {
        None,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple
    }

    public enum BlockType
    {
        Normal, //일반블럭
        ItemRocket, //로켓
        ItemBoomerang, //부메랑
        Top, //팽이
    }

    public class HexagonBlockDataFactory
    {
        public HexagonBlockData Create(int id, Vector2Int pos, MatchInfo matchInfo = null)
        {
            var type = (BlockType)(id / 10);

            switch (type)
            {
                case BlockType.Normal:
                    return new NormalBlockData(id, pos);
                case BlockType.ItemRocket:
                    {
                        if (matchInfo == null) throw new ArgumentNullException("ItemRocket need matchInfo");

                        Vector2Int forward = matchInfo.BlockList[1].Pos - matchInfo.BlockList[0].Pos;
                        Vector2Int backward = matchInfo.BlockList[0].Pos - matchInfo.BlockList[1].Pos;

                        return new RocketBlockData(forward, backward, id, pos);
                    }
                case BlockType.ItemBoomerang:
                    return new BoomerangBlockData(id, pos);
                case BlockType.Top:
                    return new TopBlockData(id, pos);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    public abstract class HexagonBlockData
    {
        private Vector2Int _pos;
        private BlockType _type;

        protected BlockColor _color;

        public Vector2Int Pos => _pos;
        public BlockColor Color => _color;
        public BlockType Type => _type;

        public HexagonBlockData(int id, Vector2Int pos)
        {
            _pos = pos;

            _color = (BlockColor)(id % 10);
            _type = (BlockType)(id / 10);
        }
        public void Move(Vector2Int goal)
        {
            _pos = goal;
        }
    }

    public interface IItemBlock
    {
        public List<HexagonBlockData> ProcessItem(Dictionary<Vector2Int, HexagonBlockData> map);
    }

    public interface IObstacleBlock
    {

    }

    public class NormalBlockData : HexagonBlockData
    {
        public NormalBlockData(int id, Vector2Int pos) : base(id, pos)
        {
        }
    }

    public class TopBlockData : HexagonBlockData, IObstacleBlock
    {
        public TopBlockData(int id, Vector2Int pos) : base(id, pos)
        {
            _color = BlockColor.None;
        }
    }

    public class BoomerangBlockData : HexagonBlockData, IItemBlock
    {
        public BoomerangBlockData(int id, Vector2Int pos) : base(id, pos)
        {

        }

        public List<HexagonBlockData> ProcessItem(Dictionary<Vector2Int, HexagonBlockData> map)
        {
            List<HexagonBlockData> ret = new List<HexagonBlockData>();

            HexagonBlockData[] mapBlocks = map.Values.Where(data => data != null).ToArray();

            HexagonBlockData[] obstacleBlocks = mapBlocks.Where(data => data is IObstacleBlock).ToArray();
            if (obstacleBlocks.Length > 0)
            {
                obstacleBlocks = obstacleBlocks.OrderBy(data => data.Pos.GetAxialDistance(this.Pos)).ToArray();
                ret.Add(obstacleBlocks[0]);

                return ret;
            }

            HexagonBlockData[] sameColor = mapBlocks.Where(data => data.Color == this.Color).ToArray();
            if (sameColor.Length > 0)
            {
                sameColor = sameColor.OrderBy(data => data.Pos.GetAxialDistance(this.Pos)).ToArray();
                ret.Add(sameColor[0]);

                return ret;
            }

            //무작위 일반
            HexagonBlockData[] normals = mapBlocks.Where(data => data.Type == BlockType.Normal).ToArray();
            if (sameColor.Length > 0)
            {
                ret.Add(normals[UnityEngine.Random.Range(0, normals.Length)]);

                return ret;
            }

            return ret;
        }
    }

    public class RocketBlockData : HexagonBlockData, IItemBlock
    {
        Vector2Int _forward;
        Vector2Int _backward;

        public Vector2Int Direction => _forward;

        public RocketBlockData(Vector2Int forward, Vector2Int backward, int id, Vector2Int pos) : base(id, pos)
        {
            _forward = forward;
            _backward = backward;
        }

        public List<HexagonBlockData> ProcessItem(Dictionary<Vector2Int, HexagonBlockData> map)
        {
            List<HexagonBlockData> ret = new List<HexagonBlockData>();

            Vector2Int Current = this.Pos + _forward;

            while(map.ContainsKey(Current))
            {
                if(map[Current] != null)
                    ret.Add(map[Current]);
                
                Current += _forward;
            }

            Current = this.Pos + _backward;

            while (map.ContainsKey(Current))
            {
                if(map[Current] != null)
                    ret.Add(map[Current]);
                
                Current += _backward;
            }

            return ret;
        }
    }
}