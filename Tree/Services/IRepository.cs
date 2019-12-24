namespace Tree.Services
{
    using System;
    using System.Collections.Generic;

    public interface IRepository<TEntity>
        where TEntity : class
    {
        #region Methods

        TEntity GetById(Guid id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Func<TEntity, bool> filter);

        void Add(TEntity node);
        void Update(TEntity node);
        void Delete(TEntity node);
        void Apply();
        void Reset();

        #endregion
    }
}
