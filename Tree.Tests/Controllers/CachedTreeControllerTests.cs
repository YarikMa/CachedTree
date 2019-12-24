namespace Tree.Tests.Controllers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc;

    using Models;

    using NSubstitute;

    using NUnit.Framework;

    using Tree.Controllers;
    using Tree.Services;

    [TestFixture]
    public class CachedTreeControllerTests
    {
        #region Methods

        [Test]
        public void Ctor_RepositoryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.That(() => new CachedTreeController(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetNodes_RepositoryIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            treeRepository.GetAll().Returns(new List<TreeNode>());
            var cachedTreeController = new CachedTreeController(treeRepository);

            // Act
            IEnumerable<TreeNode> nodes = cachedTreeController.GetNodes().Value;

            // Assert
            Assert.That(nodes, Is.Empty);
        }

        [Test]
        public void GetNodes_RepositoryReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            treeRepository.GetAll().Returns((List<TreeNode>)null);
            var cachedTreeController = new CachedTreeController(treeRepository);

            // Act
            IEnumerable<TreeNode> nodes = cachedTreeController.GetNodes().Value;

            // Assert
            Assert.That(nodes, Is.Empty);
        }

        [Test]
        public void GetNodes_NodesExists_ReturnsNodes()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
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
            var cachedTreeController = new CachedTreeController(treeRepository);

            // Act
            IEnumerable<TreeNode> nodes = cachedTreeController.GetNodes().Value;

            // Assert
            Assert.That(nodes, Is.EquivalentTo(expectedNodes));
        }

        [Test]
        public void GetNode_NodeExists_ReturnsNode()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            var expectedNode = new TreeNode
            {
                Id = Guid.NewGuid(),
                IsDeleted = true,
                ParentId = Guid.NewGuid(),
                Value = "text"
            };

            treeRepository.GetById(expectedNode.Id).Returns(expectedNode);
            var cachedTreeController = new CachedTreeController(treeRepository);

            // Act
            TreeNode node = cachedTreeController.GetNode(expectedNode.Id).Value;

            // Assert
            Assert.That(node, Is.EqualTo(expectedNode));
        }

        [Test]
        public void GetNode_NodeNotExists_ReturnsNotFoundResult()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            treeRepository.GetById(Arg.Any<Guid>()).Returns((TreeNode)null);
            var cachedTreeController = new CachedTreeController(treeRepository);

            // Act
            ActionResult result = cachedTreeController.GetNode(Arg.Any<Guid>()).Result;

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void AddNode_NewNodeIsValid_ReturnsCreatedAtActionWithNewId()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            Guid newId = Guid.NewGuid();
            treeRepository.Add(Arg.Do<TreeNode>(node => node.Id = newId));
            var cachedTreeController = new CachedTreeController(treeRepository);

            // Act
            ActionResult result = cachedTreeController.AddNode(new TreeNode()).Result;

            // Assert
            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
            Assert.That(((CreatedAtActionResult)result).RouteValues["id"], Is.EqualTo(newId));
        }

        [Test]
        public void UpdateNode_NodeIsValid_CallsRepositoryUpdate()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            Guid id = Guid.NewGuid();
            var node = new TreeNode
            {
                Id = id
            };

            // Act
            IActionResult result = new CachedTreeController(treeRepository).UpdateNode(id, node);

            // Assert
            treeRepository.Received().Update(node);
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public void UpdateNode_NodeIdNotValid_CallsRepositoryUpdate()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();

            // Act
            IActionResult result = new CachedTreeController(treeRepository).UpdateNode(
                Guid.NewGuid(),
                new TreeNode
                {
                    Id = Guid.NewGuid()
                });

            // Assert
            treeRepository.DidNotReceive().Update(Arg.Any<TreeNode>());
            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public void DeleteNode_NodeIsValid_CallsRepositoryDelete()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var node = new TreeNode
            {
                Id = id
            };
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            treeRepository.GetById(id).Returns(node);

            // Act
            IActionResult result = new CachedTreeController(treeRepository).DeleteNode(id);

            // Assert
            treeRepository.Received().Delete(node);
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public void DeleteNode_NodeIdNotValid_CallsRepositoryDelete()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var treeRepository = Substitute.For<ICachedTreeRepository>();
            treeRepository.GetById(id).Returns((TreeNode)null);

            // Act
            IActionResult result = new CachedTreeController(treeRepository).DeleteNode(id);

            // Assert
            treeRepository.DidNotReceive().Delete(Arg.Any<TreeNode>());
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void Apply_InAnyCase_CallsRepositoryApply()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();

            // Act
            IActionResult result = new CachedTreeController(treeRepository).Apply();

            // Assert
            treeRepository.Received().Apply();
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public void Reset_InAnyCase_CallsRepositoryReset()
        {
            // Arrange
            var treeRepository = Substitute.For<ICachedTreeRepository>();

            // Act
            IActionResult result = new CachedTreeController(treeRepository).Reset();

            // Assert
            treeRepository.Received().Reset();
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        #endregion
    }
}
