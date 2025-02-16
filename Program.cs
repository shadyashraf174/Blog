﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Blog {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            // Configure LoggerFactory to suppress detailed logs
            var loggerFactory = LoggerFactory.Create(builder => {
                builder.AddConsole(); 
                builder.SetMinimumLevel(LogLevel.Warning); 
            });

            var context = new MyDbContext(loggerFactory);
            context.Database.EnsureCreated();
            InitializeData(context);

            // Query 1: How many comments each user left
            Console.WriteLine("How many comments each user left:");
            var commentsPerUser = context.BlogPosts
                .SelectMany(post => post.Comments)
                .GroupBy(comment => comment.UserName)
                .Select(group => new {
                    UserName = group.Key,
                    CommentCount = group.Count()
                })
                .ToList();
            foreach (var result in commentsPerUser) {
                Console.WriteLine($"{result.UserName}: {result.CommentCount}");
            }

            // Query 2: Posts ordered by date of last comment
            Console.WriteLine("Posts ordered by date of last comment:");
            var postsOrderedByLastComment = context.BlogPosts
                .Select(post => new {
                    PostTitle = post.Title,
                    LastCommentDate = post.Comments.Max(comment => comment.CreatedDate)
                })
                .OrderByDescending(post => post.LastCommentDate)
                .ToList();
            foreach (var result in postsOrderedByLastComment) {
                Console.WriteLine($"{result.PostTitle}: '{result.LastCommentDate:yyyy-MM-dd}'");
            }

            // Query 3: How many last comments each user left
            Console.WriteLine("How many last comments each user left:");
            var lastCommentsPerUser = context.BlogPosts
                .Select(post => post.Comments
                    .OrderByDescending(comment => comment.CreatedDate)
                    .FirstOrDefault())
                .Where(comment => comment != null)
                .GroupBy(comment => comment.UserName)
                .Select(group => new {
                    UserName = group.Key,
                    LastCommentCount = group.Count()
                })
                .ToList();
            foreach (var result in lastCommentsPerUser) {
                Console.WriteLine($"{result.UserName}: {result.LastCommentCount}");
            }

            // Output blog post titles as JSON
            Console.WriteLine(JsonSerializer.Serialize(context.BlogPosts.Select(x => x.Title).ToList()));
        }

        private static void InitializeData(MyDbContext context) {
            context.BlogPosts.Add(new BlogPost("Post1") {
                Comments = new List<BlogComment>()
                {
                    new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                    new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                    new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
                }
            });
            context.BlogPosts.Add(new BlogPost("Post2") {
                Comments = new List<BlogComment>()
                {
                    new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                    new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
                }
            });
            context.BlogPosts.Add(new BlogPost("Post3") {
                Comments = new List<BlogComment>()
                {
                    new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                    new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                    new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                    new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
                }
            });
            context.SaveChanges();
        }
    }
}