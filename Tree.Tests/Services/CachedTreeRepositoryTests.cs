namespace Tree.Tests.Services
{
    using System;

    using Data;

    using Models;

    using NSubstitute;

    using NUnit.Framework;

    using Tree.Services;

    [TestFixture]
    public class CachedTreeRepositoryTests
    {
        #region Methods

        [Test]
        public void Ctor_DbTreeRepositoryIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.That(() => new CachedTreeRepository(new CachedTreeContext(), null), Throws.ArgumentNullException);
        }

        [Test]
        public void Add_NodeWithoutId_AddsNodeWithNewIdAndAddedState()
        {
            // Arrange
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);
            var node = new TreeNode();

            // Act
            cachedTreeRepository.Add(node);

            // Assert
            Assert.That(node.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(node.State, Is.EqualTo(NodeState.Added));
            Assert.That(treeContext.Tree[node.Id], Is.EqualTo(node));
        }

        [Test]
        public void Add_NodeIdIsNotEmpty_AddsNodeWithoutChangingIdAndState()
        {
            // Arrange
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);
            var id = Guid.NewGuid();
            var node = new TreeNode
            {
                Id = id
            };

            // Act
            cachedTreeRepository.Add(node);

            // Assert
            Assert.That(node.Id, Is.EqualTo(id));
            Assert.That(node.State, Is.EqualTo(NodeState.Unchanged));
            Assert.That(treeContext.Tree[node.Id], Is.EqualTo(node));
        }

        [Test]
        public void Add_ChildInCache_SetChildParentId()
        {
            // Arrange
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);
            var parentId = Guid.NewGuid();
            var childNode = new TreeNode
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                OriginalParentId = parentId
            };
            treeContext.Tree.Add(childNode.Id, childNode);
            var parentNode = new TreeNode
            {
                Id = parentId
            };

            // Act
            cachedTreeRepository.Add(parentNode);

            // Assert
            Assert.That(childNode.ParentId, Is.EqualTo(parentId));
        }

        [Test]
        public void Add_ChildWithoutParent_AddsChildNodeToRootAndStoreOriginalParentId()
        {
            // Arrange
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);
            var parentId = Guid.NewGuid();
            var childNode = new TreeNode
            {
                Id = Guid.NewGuid(),
                ParentId = parentId
            };

            // Act
            cachedTreeRepository.Add(childNode);

            // Assert
            Assert.That(childNode.ParentId, Is.Null);
            Assert.That(childNode.OriginalParentId, Is.EqualTo(parentId));
            Assert.That(treeContext.Tree[childNode.Id], Is.EqualTo(childNode));
        }

        [Test]
        public void Add_ChildToDeletedParent_DeleteChildAndGrandChild()
        {
            // Arrange
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);
            var parentNode = new TreeNode { Id = Guid.NewGuid(), ParentId = null, IsDeleted = true, State = NodeState.Deleted };
            var childNode = new TreeNode { Id = Guid.NewGuid(), ParentId = parentNode.Id };
            var grandchildNode = new TreeNode { Id = Guid.NewGuid(), ParentId = null, OriginalParentId = childNode.Id };
            treeContext.Tree.Add(parentNode.Id, parentNode);
            treeContext.Tree.Add(grandchildNode.Id, grandchildNode);

            // Act
            cachedTreeRepository.Add(childNode);

            // Assert
            Assert.That(treeContext.Tree[childNode.Id], Is.EqualTo(childNode));
            Assert.That(childNode.IsDeleted, Is.True);
            Assert.That(grandchildNode.ParentId, Is.EqualTo(childNode.Id));
            Assert.That(grandchildNode.IsDeleted, Is.True);
        }

        [Test]
        public void GetById_CacheContainsNode_ReturnsNodeWithoutRepositoryCall()
        {
            // Arrange
            var expectedNode = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(expectedNode.Id, expectedNode);
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            TreeNode node = cachedTreeRepository.GetById(expectedNode.Id);

            // Assert
            Assert.That(node, Is.EqualTo(expectedNode));
            dbTreeRepository.DidNotReceive().GetById(node.Id);
        }

        [Test]
        public void GetById_CacheDoesNotContainNode_ReturnsNodeWithRepositoryCall()
        {
            // Arrange
            var expectedNode = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(expectedNode.Id).Returns(expectedNode);
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            TreeNode node = cachedTreeRepository.GetById(expectedNode.Id);

            // Assert
            Assert.That(node.Id, Is.EqualTo(expectedNode.Id));
            dbTreeRepository.Received().GetById(node.Id);
        }

        [Test]
        public void GetById_CacheDoesNotContainNode_NodeAddsToContext()
        {
            // Arrange
            var expectedNode = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(expectedNode.Id).Returns(expectedNode);
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.GetById(expectedNode.Id);

            // Assert
            Assert.That(treeContext.Tree, Contains.Key(expectedNode.Id));
        }

        [Test]
        public void Apply_InAnyCase_CallsDbTreeRepositoryApply()
        {
            // Arrange
            var treeContext = new CachedTreeContext();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            dbTreeRepository.Received().Apply();
        }

        [TestCase(NodeState.Added)]
        [TestCase(NodeState.Modified)]
        [TestCase(NodeState.Deleted)]
        public void Apply_InAnyCase_SetNodeStateToUnchanged(NodeState state)
        {
            // Arrange
            var node = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = state
            };
            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(node.Id, node);
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(node.Id).Returns(node);
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            Assert.That(treeContext.Tree[node.Id].State, Is.EqualTo(NodeState.Unchanged));
        }

        [Test]
        public void Apply_CacheContainsAddedNode_CallsRepositoryAdd()
        {
            // Arrange
            var node = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = NodeState.Added
            };
            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(node.Id, node);
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(node.Id).Returns(node);
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            dbTreeRepository.Received().Add(Arg.Is<TreeNode>(n => n.Id == node.Id));
        }

        [Test]
        public void Apply_CacheContainsLoadedModifiedNode_CallsRepositoryUpdate()
        {
            // Arrange
            var node = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = NodeState.Modified
            };
            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(node.Id, node);
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(node.Id).Returns(node);
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            dbTreeRepository.Received().Update(Arg.Is<TreeNode>(n => n.Id == node.Id));
        }

        [Test]
        public void Apply_CacheContainsLoadedDeletedNode_CallsRepositoryDelete()
        {
            // Arrange
            var node = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = NodeState.Deleted
            };
            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(node.Id, node);
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(node.Id).Returns(node);
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            dbTreeRepository.Received().Delete(Arg.Is<TreeNode>(n => n.Id == node.Id));
        }

        [Test]
        public void Apply_DbContainsGrandParentDeletedNode_SetIsDeletedToCachedChildNode()
        {
            // Arrange
            var cachedNode = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = NodeState.Unchanged
            };
            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(cachedNode.Id, cachedNode);
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(cachedNode.Id).Returns(
                new TreeNode
                {
                    Id = cachedNode.Id,
                    IsDeleted = true
                });
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            Assert.That(cachedNode.IsDeleted, Is.True);
        }

        [Test]
        public void Apply_InAnyCase_RetrievesChangesOfNodesFromDb()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var treeContext = new CachedTreeContext();
            treeContext.Tree.Add(id1, new TreeNode { Id = id1 });
            treeContext.Tree.Add(id2, new TreeNode { Id = id2 });
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            dbTreeRepository.GetById(Arg.Any<Guid>()).Returns(new TreeNode { Id = id1 }, new TreeNode { Id = id2 });
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Apply();

            // Assert
            dbTreeRepository.Received().GetById(id1);
            dbTreeRepository.Received().GetById(id2);
        }

        [Test]
        public void Reset_InAnyCase_ResetsDbTreeRepository()
        {
            // Arrange
            var treeContext = Substitute.For<CachedTreeContext>();
            var dbTreeRepository = Substitute.For<IDbTreeRepository>();
            var cachedTreeRepository = new CachedTreeRepository(treeContext, dbTreeRepository);

            // Act
            cachedTreeRepository.Reset();

            // Assert
            treeContext.Received().Seed();
            dbTreeRepository.Received().Reset();
        }

        #endregion
    }
}
