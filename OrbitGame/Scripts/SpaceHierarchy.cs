using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class SpaceHierarchy
{
    public class Node(int axis, SDecimal? median, Node? left, Node? right, List<KinematicObject> objList)
    {
        public readonly int Axis = axis;
        public SDecimal? Median = median;
        public Node? Left = left;
        public Node? Right = right;
        public readonly List<KinematicObject> ObjectList = objList;

        public Node(int? axis, KinematicObject obj)
            : this(axis ?? 0, null, null, null, [obj])
        { }
        
        /// <summary>
        /// Returns the side of a node a coordinate is on.
        /// </summary>
        /// <param name="position">Coordinate to find the side of.</param>
        /// <returns>
        /// <para>-1 - Object is on the left side of the median (lesser side).</para>
        /// <para> 0 - Object is on the median.</para>
        /// <para>+1 - Object is on the right side of the median (greater side).</para>
        /// </returns>
        /// <exception cref="NullReferenceException">Node is a leaf node and has no median.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Node has an invalid axis.</exception>
        public int GetSide(Vec2<SDecimal> position)
        {
            if (Median is null) throw new NullReferenceException("Cannot access side of kD-tree node with no median");
            SDecimal comparisonVariable = Axis == 0 ? position.X : Axis == 1 ? position.Y : 
                throw new ArgumentOutOfRangeException(nameof(Axis), "Node axis is out of range");
            return comparisonVariable < Median ? -1 : comparisonVariable > Median.Value ? 1 : 0;
        }

        public void AddKinematicObject(KinematicObject newObj)
        {
            KinematicObject? leafObject = GetLeafObject();
            if (leafObject is null) throw new Exception("Cannot add object to branch node");
            if (Axis == 0) Median = (leafObject.Position.X + newObj.Position.X) / 2;
            if (Axis == 1) Median = (leafObject.Position.Y + newObj.Position.Y) / 2;

            int childAxis = Axis == 0 ? 1 : 0;
            
            if (GetSide(newObj.Position) <= 0)
            {
                Left = new Node(childAxis, newObj);
                Right = new Node(childAxis, leafObject);
            }

            if (GetSide(newObj.Position) > 0)
            {
                Left = new Node(childAxis, leafObject);
                Right = new Node(childAxis, newObj);
            }
        }

        public bool IsLeaf() 
            => Median is null;
        
        private KinematicObject? GetLeafObject()
        {
            if (!IsLeaf()) return null;
            return ObjectList[0];
        }
    }

    private Dictionary<string, KinematicObject> _objects;
    private Dictionary<Type, Dictionary<string, KinematicObject>> _objectTypes;
    public Node? RootNode;
    
    public SpaceHierarchy(List<KinematicObject> objList)
    {
        RootNode = ConstructKdTree(objList);
        _objects = new Dictionary<string, KinematicObject>();
        _objectTypes = new Dictionary<Type, Dictionary<string, KinematicObject>>();
        foreach (var obj in objList) AddObjectToObjectList(obj);
    }

    public Dictionary<string, KinematicObject> GetAllObjects()
        => _objects;

    public Dictionary<string, KinematicObject> GetAllObjectsOfType<T>() where T : KinematicObject
        => _objectTypes[typeof(T)];
    
    public Node TraverseTreeToPosition(Vec2<SDecimal> position, Action<Node>? action = null)
    {
        if (RootNode is null) throw new NullReferenceException("Root node does not exist");
        
        Node currentNode = RootNode;
        while (true)
        {
            if (currentNode.IsLeaf()) return currentNode;
            action?.Invoke(currentNode);
            int side = currentNode.GetSide(position);
            if (side <= 0) currentNode = currentNode.Left ?? throw new NullReferenceException("Non-leaf node has no children");
            if (side > 0) currentNode = currentNode.Right ?? throw new NullReferenceException("Non-leaf node has no children");
        }
    }

    public Node TraverseTreeToIdentifier(string identifier, Action<Node>? action = null)
    {
        return TraverseTreeToPosition(_objects[identifier].Position, action);
    }

    public void RemoveObject(string identifier)
    {
        RemoveObjectFromKdTree(identifier);
        RemoveObjectFromObjectList(identifier);
    }

    private void RemoveObjectFromObjectList(string identifier)
    {
        _objects.Remove(identifier);
        foreach (var type in _objectTypes)
            type.Value.Remove(identifier);
    }

    private void RemoveObjectFromKdTree(string identifier)
    {
        if (RootNode is null) throw new NullReferenceException("Root node does not exist");
        Node parentNode = RootNode;
        TraverseTreeToIdentifier(identifier, node => {
                node.ObjectList.RemoveAll(obj => obj.Identifier == identifier);
                parentNode = node;
            });
        parentNode.Left = null;
        parentNode.Right = null;
        parentNode.Median = null;
    }

    public void AddObject(KinematicObject obj)
    {
        AddObjectToObjectList(obj);
        AddObjectToKdTree(obj);
    }

    private void AddObjectToObjectList(KinematicObject obj)
    {
        _objects.Add(obj.Identifier, obj);
        Type? currentType = obj.GetType();
        do
        {
            if (currentType is null) break;
            _objectTypes.TryAdd(currentType, new Dictionary<string, KinematicObject>());
            _objectTypes[currentType].Add(obj.Identifier, obj);
            currentType = currentType.BaseType;
        } while (currentType != typeof(KinematicObject));
    }

    private void AddObjectToKdTree(KinematicObject obj)
    {
        // if the space hierarchy does not exist, create one.
        if (RootNode == null)
        {
            RootNode = new Node(null, obj);
            return;
        }

        Node leafNode = TraverseTreeToPosition(obj.Position, node => node.ObjectList.Add(obj));
        leafNode.AddKinematicObject(obj);
    }
    
    public void ReconstructKdTree()
    {
        RootNode = ConstructKdTree(_objects.Values.ToList());
    }

    private Node ConstructKdTree(List<KinematicObject> objList, int depth = 0)
    {
        int axis = depth % 2;

        if (objList.Count == 1) return new Node(axis, objList[0]);

        switch (axis)
        {
            case 0:
            {
                objList = objList.OrderBy(obj => obj.Position.X).ToList();
                SDecimal median = objList.Aggregate((SDecimal)0, (total, next) => total + next.Position.X) / objList.Count;

                Node node = new Node(axis, median,
                    ConstructKdTree(objList.Where(obj => obj.Position.X <= median).ToList(), depth + 1),
                    ConstructKdTree(objList.Where(obj => obj.Position.X > median).ToList(), depth + 1), 
                    objList
                );
                return node;
            }
            case 1:
            {
                objList = objList.OrderBy(obj => obj.Position.Y).ToList();
                SDecimal median = objList.Aggregate((SDecimal)0, (total, next) => total + next.Position.Y) / objList.Count;

                Node node = new Node(axis, median,
                    ConstructKdTree(objList.Where(obj => obj.Position.Y <= median).ToList(), depth + 1),
                    ConstructKdTree(objList.Where(obj => obj.Position.Y > median).ToList(), depth + 1), 
                    objList
                );
                return node;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, "KdTree axis is invalid");
        }
    }
}