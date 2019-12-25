namespace Tree.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Data;

    using Models;

    public class TreeRepository<T> : ITreeRepository<T>
        where T : TreeNode
    {
        #region Fields

        protected readonly TreeContext<T> _treeContext;

        #endregion

        #region Constructors

        public TreeRepository(TreeContext<T> treeContext)
        {
            _treeContext = treeContext ?? throw new ArgumentNullException(nameof(treeContext));
        }

        #endregion

        #region Methods

        public virtual T GetById(Guid id)
        {
            return _treeContext.Tree[id];
        }

        public IEnumerable<T> GetAll()
        {
            return _treeContext.Tree.Values;
        }

        public IEnumerable<T> Find(Func<T, bool> filter)
        {
            return GetAll().Where(filter);
        }

        public IEnumerable<T> FindChildren(Guid? parentId)
        {
            return Find(n => n.ParentId == parentId);
        }

        public virtual void Add(T node)
        {
            if (node.ParentId == null && Find(n => n.ParentId == null).Any())
            {
                throw new ArgumentException("Can't add one more root");
            }

            try
            {
                if (node.ParentId.HasValue)
                {
                    T parent = GetById(node.ParentId.Value);
                    if (parent.IsDeleted)
                    {
                        if (parent.State == NodeState.Unchanged)
                        {
                            throw new InvalidOperationException("Can't add node to deleted parent");
                        }

                        node.IsDeleted = true;
                    }
                }
            }
            catch (KeyNotFoundException e)
            {
                throw new ArgumentException("Can't add node without parent", e);
            }

            if (node.Id == Guid.Empty)
            {
                node.Id = Guid.NewGuid();
            }

            node.State = NodeState.Added;
            _treeContext.Tree.Add(node.Id, node);
        }

        public void Update(T node)
        {
            T oldNode = _treeContext.Tree[node.Id];

            if (oldNode.IsDeleted && oldNode.State == NodeState.Unchanged)
            {
                throw new InvalidOperationException("Can't update deleted node");
            }

            oldNode.Value = node.Value;
            oldNode.State = oldNode.State == NodeState.Unchanged ? NodeState.Modified : oldNode.State;
        }

        public void Delete(T node)
        {
            T oldNode = _treeContext.Tree[node.Id];
            oldNode.Value = node.Value;
            oldNode.IsDeleted = true;
            oldNode.State = oldNode.State == NodeState.Added ? NodeState.Added : NodeState.Deleted;

            foreach (T treeNode in FindChildren(node.Id))
            {
                Delete(treeNode);
            }
        }

        public virtual void Apply()
        {
            foreach (T treeNode in Find(n => n.State != NodeState.Unchanged))
            {
                treeNode.State = NodeState.Unchanged;
            }
        }

        public virtual void Reset()
        {
            _treeContext.Clear();
            _treeContext.Seed();
        }

        #endregion
    }
}
