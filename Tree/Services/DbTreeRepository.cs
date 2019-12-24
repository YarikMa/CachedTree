namespace Tree.Services
{
    using Data;

    using Models;

    public class DbTreeRepository : TreeRepository<TreeNode>, IDbTreeRepository
    {
        #region Constructors

        public DbTreeRepository(DbTreeContext treeContext)
            : base(treeContext)
        {
        }

        #endregion
    }
}
