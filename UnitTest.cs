using Blog;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace BlogTests
{
    [TestClass]
    public class BlogDataTests : IDisposable
    {
        private MyDbContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            // Arrange - Create in-memory database
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.SetMinimumLevel(LogLevel.Warning));

            _context = new MyDbContext(loggerFactory);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed test data
            SeedTestData();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        private void SeedTestData()
        {
            // Test data from Program.InitializeData
            _context.BlogPosts.AddRange(
                new BlogPost("Post1") {
                    Comments = new List<BlogComment>
                    {
                        new BlogComment("Comment1", new DateTime(2020, 3, 2), "Petr"),
                        new BlogComment("Comment2", new DateTime(2020, 3, 4), "Elena"),
                        new BlogComment("Comment3", new DateTime(2020, 3, 5), "Ivan")
                    }
                },
                new BlogPost("Post2") {
                    Comments = new List<BlogComment>
                    {
                        new BlogComment("Comment4", new DateTime(2020, 3, 5), "Elena"),
                        new BlogComment("Comment5", new DateTime(2020, 3, 6), "Ivan")
                    }
                },
                new BlogPost("Post3") {
                    Comments = new List<BlogComment>
                    {
                        new BlogComment("Comment6", new DateTime(2020, 2, 7), "Ivan"),
                        new BlogComment("Comment7", new DateTime(2020, 2, 9), "Elena"),
                        new BlogComment("Comment8", new DateTime(2020, 2, 10), "Ivan"),
                        new BlogComment("Comment9", new DateTime(2020, 2, 14), "Petr")
                    }
                }
            );
            _context.SaveChanges();
        }

        /* 
         * 1. Data Validation Tests 
         * 
         * Each test verifies one specific aspect of the seeded data
         */

        [TestMethod]
        public void SeedData_ContainsExactlyThreeBlogPosts()
        {
            // Act
            var postCount = _context.BlogPosts.Count();

            // Assert
            Assert.AreEqual(3, postCount);
        }

        [TestMethod]
        public void SeedData_Post1_HasExactlyThreeComments()
        {
            // Act
            var commentCount = _context.BlogPosts
                .Include(p => p.Comments)
                .First(p => p.Title == "Post1")
                .Comments.Count;

            // Assert
            Assert.AreEqual(3, commentCount);
        }

        [TestMethod]
        public void SeedData_Post2_HasExactlyTwoComments()
        {
            // Act
            var commentCount = _context.BlogPosts
                .Include(p => p.Comments)
                .First(p => p.Title == "Post2")
                .Comments.Count;

            // Assert
            Assert.AreEqual(2, commentCount);
        }

        [TestMethod]
        public void SeedData_Post3_HasExactlyFourComments()
        {
            // Act
            var commentCount = _context.BlogPosts
                .Include(p => p.Comments)
                .First(p => p.Title == "Post3")
                .Comments.Count;

            // Assert
            Assert.AreEqual(4, commentCount);
        }

        [TestMethod]
        public void SeedData_AllCommentsHaveValidDates()
        {
            // Act
            var invalidDates = _context.BlogComments
                .Where(c => c.CreatedDate > DateTime.Now)
                .ToList();

            // Assert
            Assert.AreEqual(0, invalidDates.Count);
        }

        /*
         * 2. Query Tests
         * Each test verifies one specific query from Program.cs
         */

        [TestMethod]
        public void Query1_CountCommentsPerUser_PetrHasTwoComments()
        {
            // Act
            var petrComments = _context.BlogPosts
                .SelectMany(p => p.Comments)
                .Count(c => c.UserName == "Petr");

            // Assert
            Assert.AreEqual(2, petrComments);
        }

        [TestMethod]
        public void Query1_CountCommentsPerUser_ElenaHasThreeComments()
        {
            // Act
            var elenaComments = _context.BlogPosts
                .SelectMany(p => p.Comments)
                .Count(c => c.UserName == "Elena");

            // Assert
            Assert.AreEqual(3, elenaComments);
        }

        [TestMethod]
        public void Query1_CountCommentsPerUser_IvanHasFourComments()
        {
            // Act
            var ivanComments = _context.BlogPosts
                .SelectMany(p => p.Comments)
                .Count(c => c.UserName == "Ivan");

            // Assert
            Assert.AreEqual(4, ivanComments);
        }

        [TestMethod]
        public void Query2_PostsOrderedByLastCommentDate_CorrectOrder()
        {
            // Act
            var orderedPosts = _context.BlogPosts
                .Select(p => new {
                    p.Title,
                    LastCommentDate = p.Comments.Max(c => c.CreatedDate)
                })
                .OrderByDescending(p => p.LastCommentDate)
                .Select(p => p.Title)
                .ToList();

            // Assert
            CollectionAssert.AreEqual(
                new List<string> { "Post2", "Post1", "Post3" },
                orderedPosts
            );
        }

        [TestMethod]
        public void Query3_CountLastCommentsPerUser_IvanHasTwo()
        {
            // Act
            var ivanLastComments = _context.BlogPosts
                .Select(p => p.Comments
                    .OrderByDescending(c => c.CreatedDate)
                    .First())
                .Count(c => c.UserName == "Ivan");

            // Assert
            Assert.AreEqual(2, ivanLastComments);
        }

        //[TestMethod]
        //public void Query3_CountLastCommentsPerUser_ElenaHasOne()
        //{
        //    // Arrange - Setup is handled in TestInitialize

        //    // Act
        //    var elenaLastComments = _context.BlogPosts
        //        .Select(p => p.Comments
        //            .OrderByDescending(c => c.CreatedDate)
        //            .FirstOrDefault())  // Use FirstOrDefault instead of First
        //        .Where(c => c != null && c.UserName == "Elena")  // Explicit null check
        //        .Count();

        //    // Assert
        //    Assert.AreEqual(1, elenaLastComments,
        //        "Elena should have exactly 1 last comment across all posts");
        //}

        [TestMethod]
        public void Query_Performance_GetAllPostsUnder50ms()
        {
            // Arrange
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var posts = _context.BlogPosts.ToList();
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50);
        }

        /*
         * 3. CRUD Operation Tests
         * Each test verifies one create/read/update/delete operation
         */

        [TestMethod]
        public void Create_NewBlogPost_SavesToDatabase()
        {
            // Arrange
            var newPost = new BlogPost("New Post");

            // Act
            _context.BlogPosts.Add(newPost);
            _context.SaveChanges();

            // Assert
            Assert.IsTrue(_context.BlogPosts.Any(p => p.Title == "New Post"));
        }

        [TestMethod]
        public void Create_NewComment_SavesToDatabase()
        {
            // Arrange
            var post = _context.BlogPosts.First();
            var newComment = new BlogComment("New Comment", DateTime.Now, "TestUser");

            // Act
            post.Comments.Add(newComment);
            _context.SaveChanges();

            // Assert
            Assert.IsTrue(_context.BlogComments.Any(c => c.Text == "New Comment"));
        }

        [TestMethod]
        public void Update_BlogPostTitle_UpdatesInDatabase()
        {
            // Arrange
            var post = _context.BlogPosts.First();
            var newTitle = "Updated Title";

            // Act
            post.Title = newTitle;
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(newTitle, _context.BlogPosts.Find(post.Id).Title);
        }

        [TestMethod]
        public void Delete_BlogPost_RemovesFromDatabase()
        {
            // Arrange
            var post = _context.BlogPosts.First();

            // Act
            _context.BlogPosts.Remove(post);
            _context.SaveChanges();

            // Assert
            Assert.IsFalse(_context.BlogPosts.Any(p => p.Id == post.Id));
        }

        //[TestMethod]
        //public void ConcurrentAccess_MultipleReads_Succeeds()
        //{
        //    // Arrange - Setup is handled in TestInitialize

        //    // Act
        //    var task1 = Task.Run(() => _context.BlogPosts.ToList());
        //    var task2 = Task.Run(() => _context.BlogComments.ToList());
        //    Task.WaitAll(task1, task2);

        //    // Assert
        //    Assert.AreEqual(3, task1.Result.Count,
        //        "Should retrieve all 3 seeded blog posts concurrently");
        //    Assert.AreEqual(9, task2.Result.Count,
        //        "Should retrieve all 9 seeded comments concurrently");
        //}

        /*
         * 4. Relationship Tests
         * Each test verifies navigation properties and foreign keys
         */

        [TestMethod]
        public void BlogComment_BlogPostNavigation_IsCorrect()
        {
            // Act
            var comment = _context.BlogComments
                .Include(c => c.BlogPost)
                .First();

            // Assert
            Assert.IsNotNull(comment.BlogPost);
            Assert.AreEqual(comment.BlogPostId, comment.BlogPost.Id);
        }

        [TestMethod]
        public void BlogPost_CommentsNavigation_IsCorrect()
        {
            // Act
            var post = _context.BlogPosts
                .Include(p => p.Comments)
                .First();

            // Assert
            Assert.IsNotNull(post.Comments);
            Assert.IsTrue(post.Comments.All(c => c.BlogPostId == post.Id));
        }

        [TestMethod]
        public void BlogPost_WithNoComments_ReturnsEmptyCollection()
        {
            // Arrange
            var newPost = new BlogPost("Empty Post");
            _context.BlogPosts.Add(newPost);
            _context.SaveChanges();

            // Act
            var comments = _context.Entry(newPost)
                .Collection(p => p.Comments)
                .Query()
                .ToList();

            // Assert
            Assert.AreEqual(0, comments.Count);
        }

        /*
         * 5. JSON Output Test
         * Verifies the JSON serialization shown in Program.cs
         */

        [TestMethod]
        public void JsonOutput_ContainsAllPostTitles()
        {
            // Act
            var json = System.Text.Json.JsonSerializer.Serialize(
                _context.BlogPosts.Select(p => p.Title).ToList()
            );

            // Assert
            Assert.IsTrue(json.Contains("Post1"));
            Assert.IsTrue(json.Contains("Post2"));
            Assert.IsTrue(json.Contains("Post3"));
        }
    }
}