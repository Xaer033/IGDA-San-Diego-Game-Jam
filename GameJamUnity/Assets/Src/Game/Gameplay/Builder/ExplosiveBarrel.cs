﻿using UnityEngine;
using DG.Tweening;

namespace Gameplay.Building
{
    public class ExplosiveBarrel : Buildable
    {
        public override void Build(Vector3 finalPos, float buildTime, int fallheight, Ease easeType, BuildConfig.BuildableBlueprint blueprint)
        {
            base.Build(finalPos, buildTime, fallheight, easeType, blueprint);
            Tween t = transform.DOMove(finalPos, buildTime).SetEase(easeType).OnComplete(() =>
            {
               
            });
        }

        public override void RemovePhysics()
        {
            base.RemovePhysics();
            Destroy(_rigidBody);
        }
    }
}
