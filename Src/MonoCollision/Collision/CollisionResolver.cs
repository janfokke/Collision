using System;
using System.Collections.Generic;
using ConsoleApp26;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoCollision
{
    internal readonly struct QuadTreeData
    {
        public QuadTreeData(Collider collider, ICollisionActor collisionActor)
        {
            Collider = collider;
            CollisionActor = collisionActor;
        }

        public bool Equals(QuadTreeData quadTree)
        {
            return CollisionActor == quadTree.CollisionActor;
        }

        public readonly Collider Collider;
        public readonly ICollisionActor CollisionActor;
    }

    internal class CollisionResolver // Same as MGE CollisionComponent
    {
        private readonly List<QuadTree> _activeQuadTrees = new List<QuadTree>();
        //Hashset to speedup removing and it avoids double insertion
        private readonly HashSet<ICollisionActor> _collisionActors = new HashSet<ICollisionActor>();
        private readonly Stack<WeakReference<QuadTree>> _inactiveQuadTrees = new Stack<WeakReference<QuadTree>>();
        private readonly List<QuadTreeData> _quadTreeDataCollection = new List<QuadTreeData>();
        public RectangleF Bounds { get; }
        protected int MaxCollidersPerNode { get; set; } = 25;

        public CollisionResolver(RectangleF bounds)
        {
            Bounds = bounds;
        }

        private QuadTree CreateQuadTree(RectangleF bounds)
        {
            while (_inactiveQuadTrees.TryPop(out WeakReference<QuadTree> weakReference))
            {
                if (weakReference.TryGetTarget(out QuadTree target))
                {
                    _activeQuadTrees.Add(target);
                    target.Reset(bounds);
                    return target;
                }
            }

            var quadTree = new QuadTree(this);
            quadTree.Reset(bounds);
            _activeQuadTrees.Add(quadTree);
            return quadTree;
        }

        public bool Insert(ICollisionActor collisionActor)
        {
            return _collisionActors.Add(collisionActor);
        }

        public bool Remove(ICollisionActor collisionActor)
        {
            return _collisionActors.Remove(collisionActor);
        }

        public void Update()
        {
            QuadTree quadTree = CreateQuadTree(Bounds);

            _quadTreeDataCollection.Clear();
            foreach (ICollisionActor collisionActor in _collisionActors)
            {
                var quadTreeData = new QuadTreeData(collisionActor.GetCollider(), collisionActor);
                _quadTreeDataCollection.Add(quadTreeData);
                quadTree.Insert(quadTreeData);
            }

            var queryResult = new List<QuadTreeData>();
            for (var i = 0; i < _quadTreeDataCollection.Count; i++)
            {
                QuadTreeData quadTreeData = _quadTreeDataCollection[i];
                quadTree.Query(quadTreeData.Collider, queryResult);
                for (var index = 0; index < queryResult.Count; index++)
                {
                    QuadTreeData other = queryResult[index];
                    if (other.Equals(quadTreeData))
                    {
                        continue;
                    }

                    Vector2 penetrationVector = quadTreeData.Collider.CalculatePenetrationVector(other.Collider);
                    quadTreeData.CollisionActor.HandleCollision(new Collision
                        {Penetration = penetrationVector, Other = other.CollisionActor});
                }

                queryResult.Clear();
            }

            for (var index = 0; index < _activeQuadTrees.Count; index++)
            {
                QuadTree activeQuadTree = _activeQuadTrees[index];
                _inactiveQuadTrees.Push(new WeakReference<QuadTree>(activeQuadTree));
            }

            _activeQuadTrees.Clear();
        }


        private class QuadTree
        {
            private readonly QuadTree[] _branches = new QuadTree[4];
            private readonly List<QuadTreeData> _colliders = new List<QuadTreeData>();
            private readonly CollisionResolver _collisionResolver;
            public RectangleF Bounds { get; set; }
            public bool IsLeaf { get; set; }

            public QuadTree(CollisionResolver collisionResolver)
            {
                _collisionResolver = collisionResolver;
                IsLeaf = true;
            }

            public void Insert(in QuadTreeData quadTreeData)
            {
                if (quadTreeData.Collider.Intersects(Bounds) == false)
                {
                    return;
                }

                if (IsLeaf && _colliders.Count >= _collisionResolver.MaxCollidersPerNode)
                {
                    Split();
                }

                if (IsLeaf)
                {
                    _colliders.Add(quadTreeData);
                }
                else
                {
                    for (var index = 0; index < _branches.Length; index++)
                    {
                        QuadTree branch = _branches[index];
                        branch.Insert(in quadTreeData);
                    }
                }
            }

            public void Query(in Collider collider, List<QuadTreeData> collisions)
            {
                // Collider is not in quad tree
                if (!collider.Intersects(Bounds))
                {
                    return;
                }

                if (IsLeaf)
                {
                    for (var index = 0; index < _colliders.Count; index++)
                    {
                        QuadTreeData childCollider = _colliders[index];
                        if (collider.Intersects(childCollider.Collider))
                        {
                            collisions.Add(childCollider);
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < _branches.Length; i++)
                    {
                        _branches[i].Query(collider, collisions);
                    }
                }
            }

            private void Split()
            {
                Point2 min = Bounds.TopLeft;
                Point2 max = Bounds.BottomRight;
                Point2 center = Bounds.Center;

                Span<RectangleF> childAreas = stackalloc RectangleF[]
                {
                    RectangleF.CreateFrom(min, center),
                    RectangleF.CreateFrom(new Point2(center.X, min.Y), new Point2(max.X, center.Y)),
                    RectangleF.CreateFrom(center, max),
                    RectangleF.CreateFrom(new Point2(min.X, center.Y), new Point2(center.X, max.Y))
                };

                for (var i = 0; i < childAreas.Length; ++i)
                {
                    QuadTree node = _collisionResolver.CreateQuadTree(childAreas[i]);
                    _branches[i] = node;
                }

                for (var index = 0; index < _colliders.Count; index++)
                {
                    QuadTreeData collider = _colliders[index];
                    for (var i = 0; i < _branches.Length; i++)
                    {
                        QuadTree childQuadTree = _branches[i];
                        childQuadTree.Insert(collider);
                    }
                }

                IsLeaf = false;
            }

            public void Reset(RectangleF bounds)
            {
                _colliders.Clear();
                Bounds = bounds;
                IsLeaf = true;
            }
        }
    }
}