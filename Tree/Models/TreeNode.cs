namespace Tree.Models
{
    using System;

    public class TreeNode
    {
        #region Properties

        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? OriginalParentId { get; set; }
        public string Value { get; set; }
        public bool IsDeleted { get; set; }
        public NodeState State { get; set; } = NodeState.Unchanged;

        #endregion

        public static TreeNode Clone(TreeNode treeNode)
        {
            return new TreeNode
            {
                Id = treeNode.Id,
                ParentId = treeNode.ParentId,
                OriginalParentId = treeNode.OriginalParentId,
                Value = treeNode.Value,
                IsDeleted = treeNode.IsDeleted,
                State = treeNode.State
            };
        }
    }
}
