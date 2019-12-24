namespace Tree.Tests.Controllers
{
    using System;
    using System.Collections.Generic;

    using Models;

    using NSubstitute;

    using NUnit.Framework;

    using Tree.Controllers;
    using Tree.Services;

    [TestFixture]
    public class DbTreeControllerTests
    {
        #region Methods

        [Test]
        public void Ctor_RepositoryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.That(() => new DbTreeController(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetNodes_RepositoryIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var treeRepository = Substitute.For<IDbTreeRepository>();
            treeRepository.GetAll().Returns(new List<TreeNode>());
            var dbTreeController = new DbTreeController(treeRepository);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeController.GetNodes().Value;

            // Assert
            Assert.That(nodes, Is.Empty);
        }

        [Test]
        public void GetNodes_RepositoryReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var treeRepository = Substitute.For<IDbTreeRepository>();
            treeRepository.GetAll().Returns((List<TreeNode>)null);
            var dbTreeController = new DbTreeController(treeRepository);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeController.GetNodes().Value;

            // Assert
            Assert.That(nodes, Is.Empty);
        }

        [Test]
        public void GetNodes_NodesExists_ReturnsNodes()
        {
            // Arrange
            var treeRepository = Substitute.For<IDbTreeRepository>();
            var expectedNodes = new List<TreeNode>
            {
                new TreeNode
                {
                    Id = Guid.NewGuid(),
                    IsDeleted = true,
                    ParentId = Guid.NewGuid(),
                    Value = "text"
                }
            };
            treeRepository.GetAll().Returns(expectedNodes);
            var dbTreeController = new DbTreeController(treeRepository);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeController.GetNodes().Value;

            // Assert
            Assert.That(nodes, Is.EquivalentTo(expectedNodes));
        }

        #endregion
    }
}
