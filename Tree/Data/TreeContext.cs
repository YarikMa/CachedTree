namespace Tree.Data
{
    using System;
    using System.Collections.Generic;

    using Models;

    public class TreeContext<T>
        where T : TreeNode
    {
        #region Properties

        public Dictionary<Guid, T> Tree { get; } = new Dictionary<Guid, T>();

        #endregion

        #region Methods

        public virtual void Seed()
        {
        }

        public void Clear()
        {
            Tree.Clear();
        }

        #endregion
    }
}
