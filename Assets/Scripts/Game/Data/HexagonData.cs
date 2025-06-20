using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        Top, //팽이
    }

    public class HexagonBlockDataFactory
    {
        public HexagonBlockData Create(int id, Vector2Int pos)
        {
            var type = (BlockType) (id / 10);

            return type switch
            {
                BlockType.Normal => new NormalBlockData(id, pos),
                BlockType.Top => new TopBlockData(id, pos),
                _ => throw new ArgumentOutOfRangeException()
            };  
        }
    }
    
    public abstract class HexagonBlockData
    {
        private Vector2Int _pos;
        private BlockColor _color;
        private BlockType _type;
        
        public Vector2Int Pos => _pos;
        public BlockColor Color => _color;
        public BlockType Type => _type;

        public HexagonBlockData(int id, Vector2Int pos)
        {
            _pos = pos;

            _color = (BlockColor)(id % 10);
            _type = (BlockType) (id / 10);
        }
        public void Move(Vector2Int goal)
        {
            _pos = goal;
        }

        public void Change()
        {
            
        }

        public void Remove()
        {
        }
    }

    public class NormalBlockData : HexagonBlockData
    {
        public NormalBlockData(int id, Vector2Int pos) : base(id, pos)
        {
        }
    }

    public class TopBlockData : HexagonBlockData
    {
        public TopBlockData(int id, Vector2Int pos) : base(id, pos)
        {
        }
    }
}