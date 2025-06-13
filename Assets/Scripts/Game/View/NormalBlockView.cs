using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class NormalBlockView : BlockView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private List<Sprite> normalSprites;

        public void SetSprite(BlockColor color)
        {
            var index = (int)color - 1;
            _spriteRenderer.sprite = normalSprites[index];
        }
    }
}