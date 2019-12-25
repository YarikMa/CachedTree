namespace Tree.Services
{
    using System;

    using Models;

    public interface ICachedTreeRepository : ITreeRepository<TreeNode>
    {
        TreeNode Load(Guid id);
    }
}
