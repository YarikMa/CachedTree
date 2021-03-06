﻿namespace Tree.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Data;

    using Models;


    public class CachedTreeRepository : TreeRepository<TreeNode>, ICachedTreeRepository
    {
        #region Fields

        private readonly IDbTreeRepository _treeRepository;

        #endregion

        #region Constructors

        public CachedTreeRepository(CachedTreeContext cachedTreeContext, IDbTreeRepository treeRepository)
            : base(cachedTreeContext)
        {
            _treeRepository = treeRepository ?? throw new ArgumentNullException(nameof(treeRepository));
        }

        #endregion

        #region Methods

        public override void Add(TreeNode node)
        {
            if (node.Id == Guid.Empty)
            {
                node.Id = Guid.NewGuid();
                node.State = NodeState.Added;
            }
            
            _treeContext.Tree.Add(node.Id, node);

            RestoreLinks(node);
        }

        public TreeNode Load(Guid id)
        {
            TreeNode treeNode = GetById(id);
            if (treeNode != null)
            {
                return treeNode;
            }
                
            treeNode = _treeRepository.GetById(id);
            
            if (treeNode == null || treeNode.IsDeleted)
            {
                return null;
            }

            TreeNode node = TreeNode.Clone(treeNode);
            Add(node);

            return node;
        }

        public override void Apply()
        {
            ApplyChanges(FindChildren(null));
            base.Apply();
            _treeRepository.Apply();
            RetrieveChanges();
        }

        public override void Reset()
        {
            base.Reset();
            _treeRepository.Reset();
        }

        private void RestoreLinks(TreeNode node)
        {
            // try to find children with original parentId
            IEnumerable<TreeNode> children = Find(n => n.OriginalParentId == node.Id);
            foreach (TreeNode child in children)
            {
                child.ParentId = node.Id;
            }

            if (!node.ParentId.HasValue)
            {
                return;
            }

            // try to find parent
            if (_treeContext.Tree.TryGetValue(node.ParentId.Value, out TreeNode parentNode))
            {
                if (parentNode.IsDeleted)
                {
                    Delete(node);
                }
            }
            else
            {
                node.OriginalParentId = node.ParentId;
                node.ParentId = null;
            }
        }

        private void ApplyChanges(IEnumerable<TreeNode> children)
        {
            foreach (TreeNode child in children)
            {
                switch (child.State)
                {
                    case NodeState.Added:
                        _treeRepository.Add(TreeNode.Clone(child));
                        break;
                    case NodeState.Modified:
                        _treeRepository.Update(TreeNode.Clone(child));
                        break;
                    case NodeState.Deleted:
                        _treeRepository.Delete(TreeNode.Clone(child));
                        break;
                    case NodeState.Unchanged:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ApplyChanges(_treeContext.Tree.Values.Where(n => n.ParentId == child.Id));
            }
        }

        private void RetrieveChanges()
        {
            foreach (TreeNode treeNode in GetAll())
            {
                treeNode.IsDeleted = _treeRepository.GetById(treeNode.Id).IsDeleted;
            }
        }

        #endregion
    }
}
