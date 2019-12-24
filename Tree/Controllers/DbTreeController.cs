namespace Tree.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc;

    using Models;

    using Services;

    [Route("api/[controller]")]
    [ApiController]
    public class DbTreeController : ControllerBase
    {
        #region Fields

        private readonly IDbTreeRepository _treeRepository;

        #endregion

        #region Constructors

        public DbTreeController(IDbTreeRepository treeRepository)
        {
            _treeRepository = treeRepository ?? throw new ArgumentNullException(nameof(treeRepository));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<TreeNode>> GetNodes()
        {
            return _treeRepository.GetAll()?.ToList() ?? new List<TreeNode>();
        }

        #endregion
    }
}
