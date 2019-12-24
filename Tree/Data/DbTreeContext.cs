namespace Tree.Data
{
    using System;

    using Models;

    public class DbTreeContext : TreeContext<TreeNode>
    {
        #region Methods

        public override void Seed()
        {
            var node0 = new TreeNode { Id = Guid.NewGuid(), ParentId = null, Value = "Node 1" };
            var node1 = new TreeNode { Id = Guid.NewGuid(), ParentId = node0.Id, Value = "Node 2" };
            var node2 = new TreeNode { Id = Guid.NewGuid(), ParentId = node0.Id, Value = "Node 3" };
            var node3 = new TreeNode { Id = Guid.NewGuid(), ParentId = node0.Id, Value = "Node 4" };
            var node4 = new TreeNode { Id = Guid.NewGuid(), ParentId = node2.Id, Value = "Node 5" };
            var node5 = new TreeNode { Id = Guid.NewGuid(), ParentId = node2.Id, Value = "Node 6" };
            var node6 = new TreeNode { Id = Guid.NewGuid(), ParentId = node5.Id, Value = "Node 7" };
            var node7 = new TreeNode { Id = Guid.NewGuid(), ParentId = node6.Id, Value = "Node 8" };
            var node8 = new TreeNode { Id = Guid.NewGuid(), ParentId = node4.Id, Value = "Node 9" };
            var node9 = new TreeNode { Id = Guid.NewGuid(), ParentId = node8.Id, Value = "Node 10" };

            Tree.Add(node0.Id, node0);
            Tree.Add(node1.Id, node1);
            Tree.Add(node2.Id, node2);
            Tree.Add(node3.Id, node3);
            Tree.Add(node4.Id, node4);
            Tree.Add(node5.Id, node5);
            Tree.Add(node6.Id, node6);
            Tree.Add(node7.Id, node7);
            Tree.Add(node8.Id, node8);
            Tree.Add(node9.Id, node9);
        }

        #endregion
    }
}
