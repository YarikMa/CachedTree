namespace Tree.Tests.Services
{
    using System;
    using System.Collections.Generic;

    using Data;

    using Models;

    using NSubstitute;

    using NUnit.Framework;

    using Tree.Services;

    [TestFixture]
    public class DbTreeRepositoryTests
    {
        #region Methods

        [Test]
        public void Ctor_TreeContextIsNull_ThrowArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            Assert.That(() => new DbTreeRepository(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetById_NodeExists_ReturnsNode()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var id = Guid.NewGuid();
            var expectedNode = new TreeNode
            {
                Id = id
            };
            treeContext.Tree.Add(id, expectedNode);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            TreeNode node = dbTreeRepository.GetById(id);

            // Assert
            Assert.That(node, Is.EqualTo(expectedNode));
        }

        [Test]
        public void GetById_NodeNotExists_ThrowsKeyNotFoundException()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var id = Guid.NewGuid();
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            // Assert
            Assert.That(() => dbTreeRepository.GetById(id), Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GetAll_InAnyCase_ReturnsNodes()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            TreeNode[] expectedNodes =
            {
                new TreeNode
                {
                    Id = Guid.NewGuid()
                },
                new TreeNode
                {
                    Id = Guid.NewGuid()
                }
            };
            treeContext.Tree.Add(expectedNodes[0].Id, expectedNodes[0]);
            treeContext.Tree.Add(expectedNodes[1].Id, expectedNodes[1]);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeRepository.GetAll();

            // Assert
            Assert.That(nodes, Is.EquivalentTo(expectedNodes));
        }

        [Test]
        public void FindChildren_ParentIdIsNull_ReturnsRoot()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            var node = new TreeNode
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid()
            };
            treeContext.Tree.Add(root.Id, root);
            treeContext.Tree.Add(node.Id, node);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeRepository.FindChildren(null);

            // Assert
            Assert.That(nodes, Is.EquivalentTo(new[] { root }));
        }

        [Test]
        public void FindChildren_ParentIdIsNotNull_ReturnsChildren()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var parentId = Guid.NewGuid();
            TreeNode[] expectedNodes =
            {
                new TreeNode
                {
                    Id = Guid.NewGuid(),
                    ParentId = parentId
                },
                new TreeNode
                {
                    Id = Guid.NewGuid(),
                    ParentId = parentId
                }
            };
            treeContext.Tree.Add(expectedNodes[0].Id, expectedNodes[0]);
            treeContext.Tree.Add(expectedNodes[1].Id, expectedNodes[1]);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeRepository.FindChildren(parentId);

            // Assert
            Assert.That(nodes, Is.EquivalentTo(expectedNodes));
        }

        [Test]
        public void FindChildren_NoChildren_ReturnsEmpty()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var parentId = Guid.NewGuid();
            TreeNode[] expectedNodes =
            {
                new TreeNode
                {
                    Id = Guid.NewGuid(),
                    ParentId = parentId
                },
                new TreeNode
                {
                    Id = Guid.NewGuid(),
                    ParentId = parentId
                }
            };
            treeContext.Tree.Add(expectedNodes[0].Id, expectedNodes[0]);
            treeContext.Tree.Add(expectedNodes[1].Id, expectedNodes[1]);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            IEnumerable<TreeNode> nodes = dbTreeRepository.FindChildren(Guid.NewGuid());

            // Assert
            Assert.That(nodes, Is.Empty);
        }

        [Test]
        public void Add_NodeIsValid_AddsNode()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var newNode = new TreeNode
            {
                ParentId = root.Id
            };

            // Act
            dbTreeRepository.Add(newNode);

            // Assert
            Assert.That(treeContext.Tree[newNode.Id], Is.EqualTo(newNode));
        }

        [Test]
        public void Add_OneMoreRoot_ThrowsArgumentException()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var newNode = new TreeNode();

            // Act
            // Assert
            Assert.That(() => dbTreeRepository.Add(newNode), Throws.ArgumentException);
        }

        [Test]
        public void Add_ToDeletedParent_ThrowsInvalidOperationException()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                IsDeleted = true
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var newNode = new TreeNode
            {
                ParentId = root.Id
            };

            // Act
            // Assert
            Assert.That(() => dbTreeRepository.Add(newNode), Throws.InvalidOperationException);
        }

        [TestCase(NodeState.Added)]
        [TestCase(NodeState.Modified)]
        [TestCase(NodeState.Deleted)]
        public void Add_ToDeletedParentWithChangedState_AddsNodeAndIsDeletedTrue(NodeState state)
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                IsDeleted = true,
                State = state
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var newNode = new TreeNode
            {
                ParentId = root.Id
            };

            // Act
            dbTreeRepository.Add(newNode);

            // Assert
            Assert.That(treeContext.Tree[newNode.Id], Is.EqualTo(newNode));
            Assert.That(treeContext.Tree[newNode.Id].IsDeleted, Is.True);
        }

        [Test]
        public void Add_ParentNotExists_ThrowsArgumentNullException()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var newNode = new TreeNode
            {
                ParentId = Guid.NewGuid()
            };

            // Act
            // Assert
            Assert.That(() => dbTreeRepository.Add(newNode), Throws.ArgumentException);
        }

        [Test]
        public void Add_NodeIdIsEmpty_SetsNewId()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var newNode = new TreeNode();

            // Act
            dbTreeRepository.Add(newNode);

            // Assert
            Assert.That(newNode.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void Add_NodeIdIsNotEmpty_DoesNotSetNewId()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var id = Guid.NewGuid();
            var newNode = new TreeNode
            {
                Id = id
            };

            // Act
            dbTreeRepository.Add(newNode);

            // Assert
            Assert.That(newNode.Id, Is.EqualTo(id));
        }

        [Test]
        public void Update_NodeIsValid_UpdatesNode()
        {
            // Arrange
            const string TEXT = "text";
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var updatedNode = new TreeNode
            {
                Id = root.Id,
                Value = TEXT
            };

            // Act
            dbTreeRepository.Update(updatedNode);

            // Assert
            Assert.That(treeContext.Tree[root.Id].Value, Is.EqualTo(TEXT));
            Assert.That(treeContext.Tree[root.Id].State, Is.EqualTo(NodeState.Modified));
        }

        [Test]
        public void Update_NodeIsDeleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                IsDeleted = true
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var updatedNode = new TreeNode
            {
                Id = root.Id,
                Value = "text"
            };

            // Act
            // Assert
            Assert.That(() => dbTreeRepository.Update(updatedNode), Throws.InvalidOperationException);
        }

        [Test]
        public void Update_NodeNotExists_ThrowsKeyNotFoundException()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var updatedNode = new TreeNode
            {
                Id = Guid.NewGuid()
            };

            // Act
            // Assert
            Assert.That(() => dbTreeRepository.Update(updatedNode), Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void Update_UpdatedNodeWithDeleteState_UpdatesNode()
        {
            // Arrange
            const string TEXT = "text";
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                IsDeleted = true,
                State = NodeState.Deleted
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var updatedNode = new TreeNode
            {
                Id = root.Id,
                Value = TEXT
            };

            // Act
            dbTreeRepository.Update(updatedNode);

            // Assert
            Assert.That(treeContext.Tree[root.Id].Value, Is.EqualTo(TEXT));
        }

        [TestCase(NodeState.Added)]
        [TestCase(NodeState.Modified)]
        [TestCase(NodeState.Deleted)]
        public void Update_UpdatedNodeWithChangedState_StateIsTheSame(NodeState state)
        {
            // Arrange
            const string TEXT = "text";
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = state
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);
            var updatedNode = new TreeNode
            {
                Id = root.Id,
                Value = TEXT
            };

            // Act
            dbTreeRepository.Update(updatedNode);

            // Assert
            Assert.That(treeContext.Tree[root.Id].State, Is.EqualTo(state));
        }

        [Test]
        public void Delete_NodeWithoutChildren_SetNodeDeleted()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            dbTreeRepository.Delete(root);

            // Assert
            Assert.That(treeContext.Tree[root.Id].IsDeleted, Is.True);
        }

        [Test]
        public void Delete_NodeWithChildren_DeletedByCascade()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid()
            };
            var node1 = new TreeNode
            {
                Id = Guid.NewGuid(),
                ParentId = root.Id
            };
            var node2 = new TreeNode
            {
                Id = Guid.NewGuid(),
                ParentId = node1.Id
            };
            treeContext.Tree.Add(root.Id, root);
            treeContext.Tree.Add(node1.Id, node1);
            treeContext.Tree.Add(node2.Id, node2);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            dbTreeRepository.Delete(root);

            // Assert
            Assert.That(treeContext.Tree[root.Id].IsDeleted, Is.True);
            Assert.That(treeContext.Tree[node1.Id].IsDeleted, Is.True);
            Assert.That(treeContext.Tree[node2.Id].IsDeleted, Is.True);
        }

        [Test]
        public void Delete_NodeInAddedState_StateDoesNotChange()
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = NodeState.Added
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            dbTreeRepository.Delete(root);

            // Assert
            Assert.That(treeContext.Tree[root.Id].State, Is.EqualTo(NodeState.Added));
            Assert.That(treeContext.Tree[root.Id].IsDeleted, Is.True);
        }

        [Test]
        public void Delete_NodeHasAnotherValue_ValueChanges()
        {
            // Arrange
            const string CHANGED_VALUE = "changed";
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = NodeState.Unchanged,
                Value = "unchanged"
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            dbTreeRepository.Delete(new TreeNode {Id = root.Id, IsDeleted = true, Value = CHANGED_VALUE});

            // Assert
            Assert.That(root.Value, Is.EqualTo(CHANGED_VALUE));
        }

        [TestCase(NodeState.Added)]
        [TestCase(NodeState.Modified)]
        [TestCase(NodeState.Deleted)]
        public void Apply_InAnyCase_SetNodeStateToUnchanged(NodeState state)
        {
            // Arrange
            var treeContext = new DbTreeContext();
            var root = new TreeNode
            {
                Id = Guid.NewGuid(),
                State = state
            };
            treeContext.Tree.Add(root.Id, root);
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            dbTreeRepository.Apply();

            // Assert
            Assert.That(treeContext.Tree[root.Id].State, Is.EqualTo(NodeState.Unchanged));
        }

        [Test]
        public void Reset_InAnyCase_CallsSeedForContext()
        {
            // Arrange
            var treeContext = Substitute.For<DbTreeContext>();
            var dbTreeRepository = new DbTreeRepository(treeContext);

            // Act
            dbTreeRepository.Reset();

            // Assert
            treeContext.Received().Seed();
        }

        #endregion
    }
}
