using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Il2CppNocturne
{
    public sealed class OverrideCustomAnimator : MonoBehaviour
    {
        public SpriteRenderer newEnemy;
        public SpriteRenderer baseEnemy;
        public Dictionary<String, Sprite> sprites;
        //public List<string> debug = new List<string>();

        public OverrideCustomAnimator(SpriteRenderer baseEnemy, SpriteRenderer newEnemy, Dictionary<String, Sprite> sprites)
        {
            this.baseEnemy = baseEnemy;
            this.newEnemy = newEnemy;
            this.sprites = sprites;
        }

        public OverrideCustomAnimator(IntPtr ptr) : base(ptr) {
            newEnemy = null;
            baseEnemy = null;
            sprites = null;
        }

        void Update()
        {

            //if (!debug.Contains(baseEnemy.sprite.name))
            //{
            //    debug.Add(baseEnemy.sprite.name);
            //    MelonLogger.Msg("[DEBUG] " + baseEnemy.sprite.name);
            //}

            if (sprites == null || !sprites.Any())
            {
                return;
            }

            if (baseEnemy.gameObject.layer != 0)
                baseEnemy.gameObject.layer = 0;

            if (sprites.ContainsKey(baseEnemy.sprite.name)){
                newEnemy.sprite = sprites[baseEnemy.sprite.name];
            }
        }
    }
}
