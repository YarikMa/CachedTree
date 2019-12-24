namespace Tree.Services
{
    using System;
    using System.Collections.Generic;

    using Models;

    public interface ITreeRepository<T> : IRepository<T>
        where T : TreeNode
    {
        #region Methods

        IEnumerable<T> FindChildren(Guid? parentId);

        #endregion
    }
}
