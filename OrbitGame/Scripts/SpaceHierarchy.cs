using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class SpaceHierarchy
{
    public class KDTree
    {
        private KDTreeNode _rootNode;
        private Dictionary<string, KinematicObject> _objects;

        public KDTree(List<KinematicObject> objList)
        {
            _objects = new Dictionary<string, KinematicObject>();
            foreach (var obj in objList)
                _objects.Add(obj.Identifier, obj);
            _rootNode = objList.Count == 0 ? new KDEmptyNode() : Construct(objList);
        }

        private KDTreeNode Construct(List<KinematicObject> objList, int splittingAxis = 0)
        {
            if (objList.Count == 0) return new KDEmptyNode();
            
            if (objList.Count <= 1)
            {
                KDLeafNode kdLeafNode = new KDLeafNode(objList[0]);
                return kdLeafNode;
            }

            bool xAxis = splittingAxis == 0;
            SDecimal median = objList.Aggregate((SDecimal)0, (total, next) => total + (xAxis ? next.Position.X : next.Position.Y)) /
                              objList.Count;

            KDBranchNode kdBranchNode =  new KDBranchNode(splittingAxis, median,
                Construct(objList.Where(obj => (xAxis ? obj.Position.X : obj.Position.Y) <= median).ToList(), (splittingAxis + 1) % 2),
                Construct(objList.Where(obj => (xAxis ? obj.Position.X : obj.Position.Y) > median).ToList(), (splittingAxis + 1) % 2)
            );
            kdBranchNode.Right.Parent = kdBranchNode;
            kdBranchNode.Left.Parent = kdBranchNode;
            return kdBranchNode;
        }
        
        public void Reconstruct()
            => _rootNode = Construct(_objects.Values.ToList());

        public KDLeafNode GetPositionLeafNode(Vec2<SDecimal> position)
        {
            KDTreeNode currentNode = _rootNode;
            while (true)
            {
                if (currentNode is KDBranchNode branch)
                {
                    int side = branch.GetSide(position);
                    if (side <= 0) currentNode = branch.Left;
                    else currentNode = branch.Right;
                }
                else if (currentNode is KDLeafNode leaf)
                    return leaf;
            }
        }

        private List<KinematicObject> TraverseBranch(KDTreeNode rootNode, Vec2<SDecimal> queryPosition, SDecimal radius)
        {
            List<KinematicObject> objectsInRadius = new();
            switch (rootNode)
            {
                case KDLeafNode leafNode:
                    if (leafNode.ObjectWithinRadius(queryPosition, radius))
                        objectsInRadius.Add(leafNode.Object);
                    break;
                case KDBranchNode branchNode when branchNode.RadiusIntersectsPlane(queryPosition, radius):
                    objectsInRadius.AddRange(TraverseBranch(branchNode.Left, queryPosition, radius));
                    objectsInRadius.AddRange(TraverseBranch(branchNode.Right, queryPosition, radius));
                    break;
                case KDBranchNode branchNode:
                    if (branchNode.GetSide(queryPosition) <= 0)
                        objectsInRadius.AddRange(TraverseBranch(branchNode.Left, queryPosition, radius));
                    if (branchNode.GetSide(queryPosition) > 0)
                        objectsInRadius.AddRange(TraverseBranch(branchNode.Right, queryPosition, radius));
                    break;
            }

            return objectsInRadius;
        }

        public List<KinematicObject> GetObjectsInRadius(Vec2<SDecimal> queryPosition, SDecimal radius)
        {
            return TraverseBranch(_rootNode, queryPosition, radius);
        }
        
        public void AddKinematicObject(KinematicObject obj)
        {
            _objects.Add(obj.Identifier, obj);
            
            if (_rootNode is KDEmptyNode)
            {
                _rootNode = new KDLeafNode(obj);
                return;
            }
            
            KDLeafNode leafNode = GetPositionLeafNode(obj.Position);
            KDBranchNode? parent = (KDBranchNode?)leafNode.Parent;
            if (parent is null) _rootNode = Construct([leafNode.Object, obj]);
            else
            {
                if (parent.Right.Equals(leafNode)) parent.Right = Construct([leafNode.Object, obj], (parent.Axis + 1) % 2);
                else if (parent.Left.Equals(leafNode)) parent.Left = Construct([leafNode.Object, obj], (parent.Axis + 1) % 2);
            }
        }

        public void RemoveKinematicObject(string identifier)
        {
            KDLeafNode leafNode = GetPositionLeafNode(_objects[identifier].Position);
            _objects.Remove(identifier);
            KDBranchNode? parent = (KDBranchNode?)leafNode.Parent;
            if (parent is null) throw new InvalidOperationException("Cannot remove last object in kD-tree");
            KinematicObject newLeafNodeObject = parent.Right.Equals(leafNode) ? 
                ((KDLeafNode)parent.Left).Object : ((KDLeafNode)parent.Right).Object;
            KDBranchNode? grandParent = (KDBranchNode?)parent.Parent;
            if (grandParent is null) _rootNode = new KDLeafNode(_objects.Values.First());
            else
            {
                if (grandParent.Right.Equals(parent)) grandParent.Right = new KDLeafNode(newLeafNodeObject);
                else if (grandParent.Left.Equals(parent)) grandParent.Left = new KDLeafNode(newLeafNodeObject);
            }
        }
    }

    public abstract class KDTreeNode
    {
        public KDTreeNode? Parent;
    }

    private class KDEmptyNode : KDTreeNode;
    
    public class KDLeafNode(KinematicObject obj) : KDTreeNode
    {
        public readonly KinematicObject Object = obj;

        public bool ObjectWithinRadius(Vec2<SDecimal> queryPosition, SDecimal radius)
            => (Object.Position - queryPosition).MagnitudeSquared() <= radius * radius;
    }

    public class KDBranchNode(int axis, SDecimal median, KDTreeNode left, KDTreeNode right) : KDTreeNode
    {
        public readonly int Axis = axis;
        public SDecimal Median = median;
        public KDTreeNode Left = left;
        public KDTreeNode Right = right;

        public int GetSide(Vec2<SDecimal> position)
        {
            SDecimal comparisonVariable = Axis == 0 ? position.X : Axis == 1 ? position.Y : 
                throw new ArgumentOutOfRangeException(nameof(Axis), "Node axis is out of range");
            return comparisonVariable < Median ? -1 : comparisonVariable > Median ? 1 : 0;
        }

        public bool RadiusIntersectsPlane(Vec2<SDecimal> queryPosition, SDecimal radius)
            => SDecimal.Abs(Median - (Axis == 0 ? queryPosition.X : queryPosition.Y)) <= radius;

        public List<KinematicObject> GetObjects()
        {
            List<KinematicObject> objects = new();
            if (Right is KDBranchNode branchRight) objects.AddRange(branchRight.GetObjects());
            if (Left is KDBranchNode branchLeft) objects.AddRange(branchLeft.GetObjects());
            if (Right is KDLeafNode leafRight) objects.Add(leafRight.Object);
            if (Left is KDLeafNode leafLeft) objects.Add(leafLeft.Object);
            return objects;
        }
    }

    private readonly Dictionary<string, KinematicObject> _objects;
    private readonly Dictionary<Type, Dictionary<string, KinematicObject>> _objectTypes;
    private readonly KDTree _kdTree;
    
    public SpaceHierarchy(List<KinematicObject> objList)
    {
        _kdTree = new KDTree(objList);
        _objects = new Dictionary<string, KinematicObject>();
        _objectTypes = new Dictionary<Type, Dictionary<string, KinematicObject>>();
        foreach (var obj in objList) AddObjectToObjectList(obj);
    }

    public Dictionary<string, KinematicObject> GetAllObjects()
        => _objects;

    public Dictionary<string, T> GetAllObjectsOfType<T>() where T : KinematicObject
    {
        Dictionary<string, T> objects = new();
        if (!_objectTypes.ContainsKey(typeof(T))) return objects;
        foreach (var keyValuePair in _objectTypes[typeof(T)])
            objects.Add(keyValuePair.Key, (T)keyValuePair.Value);
        return objects;
    }

    public void RemoveObject(string identifier)
    {
        RemoveObjectFromObjectList(identifier);
        _kdTree.RemoveKinematicObject(identifier);
    }

    private void RemoveObjectFromObjectList(string identifier)
    {
        _objects.Remove(identifier);
        foreach (var type in _objectTypes)
            type.Value.Remove(identifier);
    }

    public void AddObject(KinematicObject obj)
    {
        AddObjectToObjectList(obj);
        _kdTree.AddKinematicObject(obj);
    }

    private void AddObjectToObjectList(KinematicObject obj)
    {
        _objects.Add(obj.Identifier, obj);
        Type? currentType = obj.GetType();
        while (true)
        {
            if (currentType is null) break;
            _objectTypes.TryAdd(currentType, new Dictionary<string, KinematicObject>());
            _objectTypes[currentType].Add(obj.Identifier, obj);
            if (currentType == typeof(KinematicObject)) break;
            currentType = currentType.BaseType;
        } 
    }

    public void ReconstructTree()
        => _kdTree.Reconstruct();

    public List<KinematicObject> GetObjectsInRadius(Vec2<SDecimal> queryPosition, SDecimal radius)
    {
        return _kdTree.GetObjectsInRadius(queryPosition, radius);
    }
}