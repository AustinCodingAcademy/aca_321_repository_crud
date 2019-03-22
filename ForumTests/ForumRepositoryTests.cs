using System;
using System.Linq;
using FluentAssertions;
using Forum.Models;
using Forum.Services;
using Xunit;

namespace ForumTests
{
    public class ForumRepositoryTests
    {
        [Fact]
        [Trait("Domain", "Create")]
        public void Create_ShouldPersistNewRecordAndGenerateId_WhenIdIsNull()
        {
            // Assemble
            var repo = new ForumRepository();
            var post = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User1",
                Message = "This post should be returned at the bottom."
            };

            // Act
            var savedPost = repo.Create(post);

            // Assert
            savedPost.Should().NotBeNull(because: "the Saved method should return what was saved");
            savedPost.Id.Should().NotBeNull(because: "the repository is responsible for generating Id of the record");
            savedPost.Date.Should().Be(post.Date, because: "the Date should be saved along with all other fields");
            savedPost.User.Should().Be(post.User, because: "the User should be saved along with all other fields");
            savedPost.Message.Should().Be(post.Message, because: "the Message should be saved along with all other fields");
        }

        [Fact]
        [Trait("Domain", "Create")]
        public void Create_MultipleCreates_GenerateAllUniqueIds()
        {
            // Assemble
            var repo = new ForumRepository();
            var post1 = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User1",
                Message = "This post should be returned at the bottom."
            };
            var post2 = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User1",
                Message = "This post should be returned at the bottom."
            };

            // Act
            var savedPost1 = repo.Create(post1);
            var savedPost2 = repo.Create(post2);

            // Assert
            Assert.Equal(2, new[] { savedPost1.Id, savedPost2.Id }.Distinct().Count());
        }

        [Fact]
        [Trait("Domain", "Save")]
        public void Save_ShouldPersistNewRecordNotGenerateANewRecord_WhenIdIsNotNull()
        {
            // Assemble
            var repo = new ForumRepository();

            // Create a record that will be updated
            var existingPost = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User1",
                Message = "This post should NOT be returned at the bottom."
            };

            // Add that record to the Repository
            var savedPost = repo.Create(existingPost);

            // Create an post with updated values
            var updatePost = new ForumPost
            {
                // Take the result from the create new record save and get the Id off it
                Id = savedPost.Id,
                Date = DateTime.Now.AddDays(1),
                User = "Test User2",
                Message = "This post SHOULD be returned at the bottom."
            };

            // build out what we expect the post to be.
            var expectedPost = new ForumPost
            {
                Id = updatePost.Id,
                Date = updatePost.Date,
                User = updatePost.User,
                Message = updatePost.Message
            };

            // Act
            var updatedPost = repo.Save(existingPost.Id.Value, updatePost);

            // Assert
            updatedPost.Should().NotBeNull(because: "the Saved method should return what was saved");
            updatedPost.Id.Should().Be(expectedPost.Id, because: "the repository is responsible for generating Id of the record");
            updatedPost.Date.Should().Be(expectedPost.Date, because: "the Date should be saved along with all other fields");
            updatedPost.User.Should().Be(expectedPost.User, because: "the User should be saved along with all other fields");
            updatedPost.Message.Should().Be(expectedPost.Message, because: "the Message should be saved along with all other fields");
        }

        [Fact]
        [Trait("Domain", "Save")]
        public void Save_ShouldNotAllowRecordToSaveWithADifferentId()
        {
            // Assemble
            var repo = new ForumRepository();

            // Create a record that will be updated
            var existingPost = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User1",
                Message = "This post should NOT be returned at the bottom."
            };

            // Add that record to the Repository
            var savedPost = repo.Create(existingPost);

            // Act / Assert
            repo.Invoking(x => x.Save(-1, savedPost)).Should().Throw<Exception>().WithMessage("'id' must match 'forum.Id'", because: "the repository should not allow accidently updating the wrong record.");
        }

        [Fact]
        [Trait("Domain", "GetAll")]
        public void GetAll_ShouldReturnAllExistingRecords()
        {
            // Assemble
            var repo = new ForumRepository();

            // Create a record that will be updated
            var existingPost1 = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User1",
                Message = "This post should be returned at the bottom."
            };
            repo.Create(existingPost1);
            var existingPost2 = new ForumPost
            {
                Date = DateTime.Now,
                User = "Test User2",
                Message = "This post should be returned at the bottom."
            };
            repo.Create(existingPost2);

            // Act
            var allPosts = repo.GetAll();

            // Assert
            allPosts.Should().HaveCount(2, because: "the Saved method should return what was saved");
            allPosts.Should().Contain(existingPost1, because: "the repository should have what we added to it");
            allPosts.Should().Contain(existingPost2, because: "the repository should have what we added to it");
        }

        [Fact]
        [Trait("Domain", "GetAll")]
        public void GetAll_ShouldReturnEmpty_WhenNoRecordsExist()
        {
            // Assemble
            var repo = new ForumRepository();

            // Act
            var allPosts = repo.GetAll();

            // Assert
            allPosts.Should().BeEmpty(because: "if there are no records, an empty list should be returned.");
        }

        [Fact]
        [Trait("Domain", "GetAll")]
        public void GetAll_ShouldNotExposeInternalData()
        {
            // Assemble
            var repo = new ForumRepository();
            repo.Create(new ForumPost());
            var allPosts = repo.GetAll();
            allPosts.Add(new ForumPost());

            // Act
            var copyOfInternalList = repo.GetAll();

            // Assert
            allPosts.Should().HaveCount(2, because: "we expect our local list to work like a list, but it should not affect what is going on inside of the repository");
            copyOfInternalList.Should().HaveCount(1, because: "the repository should not expose the internal workings of how it saves data. Make sure you are calling `.ToList()` before returning.");
        }

        [Fact]
        [Trait("Domain", "Delete")]
        public void Delete_ShouldReturnRemoveById()
        {
            // Assemble
            var repo = new ForumRepository();
            var recordToKeep1 = repo.Create(new ForumPost());
            var recordToDelete = repo.Create(new ForumPost());
            var recordToKeep2 = repo.Create(new ForumPost());

            // Act
            repo.Delete(recordToDelete.Id.Value);

            // Assert
            var allPosts = repo.GetAll();
            allPosts.Should().NotContain(recordToDelete, because: "Delete should not keep the deleted record");
            allPosts.Select(x => x.Id).Should().NotContain(recordToDelete.Id, because: "Delete should work based on Id, not instance reference.");
        }

    }
}
