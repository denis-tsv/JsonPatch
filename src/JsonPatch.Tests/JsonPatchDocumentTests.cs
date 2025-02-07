﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JsonPatch.Tests.Entitys;
using JsonPatch.Common;

namespace JsonPatch.Tests
{
    [TestClass]
    public class JsonPatchDocumentTests
    {

        #region JsonPatch Add Tests

        [TestMethod]
        public void Add_ValidPath_OperationAdded()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Add("Foo", "bar");

            //Assert
            Assert.AreEqual(1, patchDocument.Operations.Count);
            Assert.AreEqual(JsonPatchOperationType.add, patchDocument.Operations.Single().Operation);
        }

        [TestMethod, ExpectedException(typeof(JsonPatchParseException))]
        public void Add_InvalidPath_ThrowsJsonPatchParseException()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Add("FooMissing", "bar");
        }

        #endregion

        #region JsonPatch Remove Tests

        [TestMethod]
        public void Remove_ValidPath_OperationAdded()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Remove("Foo");

            //Assert
            Assert.AreEqual(1, patchDocument.Operations.Count);
            Assert.AreEqual(JsonPatchOperationType.remove, patchDocument.Operations.Single().Operation);
        }

        [TestMethod, ExpectedException(typeof(JsonPatchParseException))]
        public void Remove_InvalidPath_ThrowsJsonPatchParseException()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Remove("FooMissing");
        }

        #endregion

        #region JsonPatch Replace Tests

        [TestMethod]
        public void Replace_ValidPath_OperationAdded()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Replace("Foo", "bar");

            //Assert
            Assert.AreEqual(1, patchDocument.Operations.Count);
            Assert.AreEqual(JsonPatchOperationType.replace, patchDocument.Operations.Single().Operation);
        }

        [TestMethod, ExpectedException(typeof(JsonPatchParseException))]
        public void Replace_InvalidPath_ThrowsJsonPatchParseException()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Replace("FooMissing", "bar");
        }

        #endregion

        #region JsonPatch Move Tests

        [TestMethod]
        public void Move_ValidPaths_OperationAdded()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Move("Foo", "Baz");

            //Assert
            Assert.AreEqual(1, patchDocument.Operations.Count);
            Assert.AreEqual(JsonPatchOperationType.move, patchDocument.Operations.Single().Operation);
        }

        [TestMethod]
        public void Move_ArrayIndexes_OperationAdded()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<ArrayEntity>();

            //Act
            patchDocument.Move("Foo/5", "Foo/2");

            //Assert
            Assert.AreEqual(1, patchDocument.Operations.Count);
            Assert.AreEqual(JsonPatchOperationType.move, patchDocument.Operations.Single().Operation);
        }

        [TestMethod, ExpectedException(typeof(JsonPatchParseException))]
        public void Move_InvalidFromPath_ThrowsJsonPatchParseException()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Move("FooMissing", "Baz");
        }

        [TestMethod, ExpectedException(typeof(JsonPatchParseException))]
        public void Move_InvalidDestinationPath_ThrowsJsonPatchParseException()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Move("Foo", "BazMissing");
        }

        #endregion

        #region JsonPatch ApplyUpdatesTo Tests

        #region Add Operation

        [TestMethod]
        public void ApplyUpdate_AddOperation_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();
            var entity = new SimpleEntity();

            //Act
            patchDocument.Add("Foo", "bar");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.AreEqual("bar", entity.Foo);
        }

        #endregion

        #region Remove Operation

        [TestMethod]
        public void ApplyUpdate_RemoveOperation_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();
            var entity = new SimpleEntity { Foo = "bar" };

            //Act
            patchDocument.Remove("Foo");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.AreEqual(null, entity.Foo);
        }

        #endregion

        #region Replace Operation

        [TestMethod]
        public void ApplyUpdate_ReplaceOperation_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();
            var entity = new SimpleEntity { Foo = "bar" };

            //Act
            patchDocument.Replace("Foo", "baz");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.AreEqual("baz", entity.Foo);
        }

        #endregion

        #region Move Operation

        [TestMethod]
        public void ApplyUpdate_MoveOperation_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();
            var entity = new SimpleEntity { Foo = "bar", Baz = "qux" };

            //Act
            patchDocument.Move("Foo", "Baz");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.IsNull(entity.Foo);
            Assert.AreEqual("bar", entity.Baz);
        }

        [TestMethod]
        public void ApplyUpdate_MoveListElement_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<ListEntity>();
            var entity = new ListEntity
            {
                Foo = new List<string> { "Element One", "Element Two", "Element Three" }
            };

            //Act
            patchDocument.Move("/Foo/2", "/Foo/1");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.AreEqual(3, entity.Foo.Count);
            Assert.AreEqual("Element One", entity.Foo[0]);
            Assert.AreEqual("Element Three", entity.Foo[1]);
            Assert.AreEqual("Element Two", entity.Foo[2]);
        }

        [TestMethod]
        public void ApplyUpdate_MoveFromListToProperty_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<ComplexEntity>();
            var entity = new ComplexEntity
            {
                Bar = new SimpleEntity(),
                Norf = new List<ListEntity>
                {
                    new ListEntity
                    {
                        Foo = new List<string> { "Element One", "Element Two", "Element Three" }
                    }
                }
            };

            //Act
            patchDocument.Move("/Norf/0/Foo/1", "/Bar/Foo");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.AreEqual(2, entity.Norf[0].Foo.Count);
            Assert.AreEqual("Element One", entity.Norf[0].Foo[0]);
            Assert.AreEqual("Element Three", entity.Norf[0].Foo[1]);
            Assert.AreEqual("Element Two", entity.Bar.Foo);
        }

        [TestMethod]
        public void ApplyUpdate_MoveFromPropertyToList_EntityUpdated()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<ComplexEntity>();
            var entity = new ComplexEntity
            {
                Bar = new SimpleEntity
                {
                    Foo = "I am foo"
                },
                Norf = new List<ListEntity>
                {
                    new ListEntity
                    {
                        Foo = new List<string> { "Element One", "Element Two", "Element Three" }
                    }
                }
            };

            //Act
            patchDocument.Move("/Bar/Foo", "/Norf/0/Foo/1");
            patchDocument.ApplyUpdatesTo(entity);

            //Assert
            Assert.IsNull(entity.Bar.Foo);
            Assert.AreEqual(4, entity.Norf[0].Foo.Count);
            Assert.AreEqual("Element One", entity.Norf[0].Foo[0]);
            Assert.AreEqual("I am foo", entity.Norf[0].Foo[1]);
            Assert.AreEqual("Element Two", entity.Norf[0].Foo[2]);
            Assert.AreEqual("Element Three", entity.Norf[0].Foo[3]);
        }

        #endregion

        #endregion

        #region JsonPatch HasOperations Tests

        [TestMethod]
        public void HasOperations_FalseByDefault()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Assert
            Assert.IsFalse(patchDocument.HasOperations);
        }

        [TestMethod]
        public void Add_ValidPath_SetsHasOperations()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Add("Foo", "bar");

            //Assert
            Assert.IsTrue(patchDocument.HasOperations);
        }

        public void Remove_ValidPath_SetsHasOperations()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Remove("Foo");

            //Assert
            Assert.IsTrue(patchDocument.HasOperations);
        }

        [TestMethod]
        public void Replace_ValidPath_SetsHasOperations()
        {
            //Arrange
            var patchDocument = new JsonPatchDocument<SimpleEntity>();

            //Act
            patchDocument.Replace("Foo", "bar");

            //Assert
            Assert.IsTrue(patchDocument.HasOperations);
        }
        #endregion
    }
}
