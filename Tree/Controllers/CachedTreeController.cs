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
    public class CachedTreeController : ControllerBase
    {
        #region Fields

        private readonly ICachedTreeRepository _treeRepository;

        #endregion

        #region Constructors

        public CachedTreeController(ICachedTreeRepository treeRepository)
        {
            _treeRepository = treeRepository ?? throw new ArgumentNullException();
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<TreeNode>> GetNodes()
        {
            return _treeRepository.GetAll()?.ToList() ?? new List<TreeNode>();
        }

        [HttpGet("{id}")]
        public ActionResult<TreeNode> GetNode(Guid id)
        {
            TreeNode treeNode = _treeRepository.Load(id);

            if (treeNode == null)
            {
                return NotFound();
            }

            return treeNode;
        }

        [HttpPost]
        public ActionResult<TreeNode> AddNode([FromBody] TreeNode treeNode)
        {
            _treeRepository.Add(treeNode);

            return CreatedAtAction(
                nameof(GetNode),
                new
                {
                    id = treeNode.Id
                },
                treeNode);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateNode(Guid id, [FromBody] TreeNode treeNode)
        {
            if (id != treeNode.Id)
            {
                return BadRequest();
            }

            _treeRepository.Update(treeNode);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNode(Guid id)
        {
            TreeNode treeNode = _treeRepository.GetById(id);

            if (treeNode == null)
            {
                return NotFound();
            }

            _treeRepository.Delete(treeNode);
            return NoContent();
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Apply()
        {
            _treeRepository.Apply();

            return NoContent();
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Reset()
        {
            _treeRepository.Reset();

            return NoContent();
        }

        #endregion
    }
}
