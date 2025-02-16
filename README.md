
# Blog Application

This is a console application built using **C#** and **Entity Framework Core** that demonstrates querying data from a blog context. The application calculates and displays three specific metrics related to blog posts and comments:

1. **How many comments each user left.**
2. **Posts ordered by the date of the last comment.**
3. **How many "last comments" each user left (where "last comment" is the latest comment in each post).**

The application uses an in-memory SQLite database for simplicity and ensures clean console output without verbose logging.

---

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Database Setup](#database-setup)
- [How to Run](#how-to-run)
- [Expected Output](#expected-output)
- [Code Structure](#code-structure)
- [Technical Details](#technical-details)
- [Suppressing Logs](#suppressing-logs)
- [Query](#Query)

---

## Overview

The application initializes an in-memory SQLite database with sample data, performs LINQ queries to calculate the required metrics, and outputs the results to the console. It ensures clean and concise output by disabling unnecessary logging.

---

## Prerequisites

Before running the application, ensure you have the following installed on your machine:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 7.0 or later)
- A text editor or IDE (e.g., Visual Studio Code, Visual Studio)

---

## Setup Instructions

1. **Clone the Repository:**
   
   ```bash
   git clone https://github.com/shadyashraf174/Blog.git
   ```
---

## Database Setup

The application uses an in-memory SQLite database for simplicity. The database schema consists of two entities:

1. **BlogPost**:
   - `Id`: Unique identifier for the blog post.
   - `Title`: Title of the blog post.
   - `Text`: Content of the blog post.
   - `Comments`: A collection of `BlogComment` objects associated with the post.

2. **BlogComment**:
   - `Id`: Unique identifier for the comment.
   - `Text`: Content of the comment.
   - `CreatedDate`: Date and time when the comment was created.
   - `UserName`: Name of the user who left the comment.
   - `BlogPostId`: Foreign key referencing the associated `BlogPost`.

The database is initialized with sample data in the `InitializeData` method of the `Program` class.

---

## How to Run

To run the application, execute the following command in the terminal:
```bash
dotnet run
```

The program will output the results of the three queries to the console.

---

## Expected Output

When the program runs successfully, you should see the following clean output:

```
Hello World!
How many comments each user left:
Ivan: 4
Petr: 2
Elena: 3
Posts ordered by date of last comment:
Post2: '2020-03-06'
Post1: '2020-03-05'
Post3: '2020-02-14'
How many last comments each user left:
Ivan: 2
Petr: 1
["Post1","Post2","Post3"]
```

---

## Code Structure

### **1. `BlogComment` Class**
Represents a comment entity with properties for `Id`, `Text`, `CreatedDate`, `UserName`, and `BlogPostId`.

### **2. `BlogPost` Class**
Represents a blog post entity with properties for `Id`, `Title`, `Text`, and a collection of `BlogComment` objects.

### **3. `MyDbContext` Class**
Defines the database context and configures the in-memory SQLite database.

### **4. `Program` Class**
Contains the main logic of the application, including:
- Initialization of the database with sample data.
- Execution of the three LINQ queries.
- Output of the results to the console.

---

## Technical Details

### **1. Entity Framework Core**
- Used for ORM (Object-Relational Mapping) and database operations.
- Configured to use an in-memory SQLite database for simplicity.

### **2. LINQ Queries**
- **Query 1**: Flattens the list of comments from all posts, groups them by `UserName`, and counts the number of comments per user.
- **Query 2**: Finds the maximum `CreatedDate` for each post's comments and orders the posts by this date in descending order.
- **Query 3**: Identifies the latest comment for each post, groups these "last comments" by `UserName`, and counts the number of last comments per user.

### **3. Logging**
- Uses `Microsoft.Extensions.Logging.Console` for logging.
- Logging is configured to suppress informational messages (e.g., SQL commands) and display only warnings or higher-level logs.

---

## Suppressing Logs

To ensure clean console output without verbose logging, the `LoggerFactory` is configured as follows:

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(); // Enable console logging
    builder.SetMinimumLevel(LogLevel.Warning); // Suppress info-level logs
});
```

This configuration disables informational logs such as SQL commands and other detailed messages, resulting in a cleaner output.

---

Below is a detailed explanation of the three LINQ queries implemented in your project. Each query is designed to calculate specific metrics related to blog posts and comments, and I'll break down how they work step by step.

---
## Query

### **Query 1: How many comments each user left?**

#### **Purpose**
This query calculates the total number of comments left by each user across all blog posts.

#### **Code**
```csharp
var commentsPerUser = context.BlogPosts
    .SelectMany(post => post.Comments) // Flatten the list of comments from all posts
    .GroupBy(comment => comment.UserName) // Group comments by UserName
    .Select(group => new {
        UserName = group.Key, // The user's name
        CommentCount = group.Count() // Count the number of comments for this user
    })
    .ToList();
```

#### **Explanation**
1. **`SelectMany(post => post.Comments)`**:
   - This flattens the nested collections of comments from all blog posts into a single enumerable.
   - Instead of working with a collection of collections (`IEnumerable<IEnumerable<BlogComment>>`), it creates a single `IEnumerable<BlogComment>`.

2. **`GroupBy(comment => comment.UserName)`**:
   - Groups the flattened list of comments by the `UserName` property.
   - This groups all comments made by the same user together.

3. **`Select(group => new { ... })`**:
   - For each group of comments (one group per user), it creates an anonymous object containing:
     - `UserName`: The name of the user.
     - `CommentCount`: The total number of comments made by that user.

4. **`ToList()`**:
   - Converts the result into a list for easier iteration and output.

#### **Output**
The result is a list of objects, where each object contains:
- `UserName`: The name of the user.
- `CommentCount`: The total number of comments left by that user.

Example Output:
```
Ivan: 4
Petr: 2
Elena: 3
```

---

### **Query 2: Posts ordered by the date of the last comment**

#### **Purpose**
This query determines the latest comment for each blog post and orders the posts by this date in descending order.

#### **Code**
```csharp
var postsOrderedByLastComment = context.BlogPosts
    .Select(post => new {
        PostTitle = post.Title, // The title of the blog post
        LastCommentDate = post.Comments.Max(comment => comment.CreatedDate) // Find the latest comment's date
    })
    .OrderByDescending(post => post.LastCommentDate) // Order posts by the latest comment date (descending)
    .ToList();
```

#### **Explanation**
1. **`Select(post => new { ... })`**:
   - For each blog post, it creates an anonymous object containing:
     - `PostTitle`: The title of the blog post.
     - `LastCommentDate`: The latest comment's `CreatedDate` for this post, calculated using `Max(comment => comment.CreatedDate)`.

2. **`OrderByDescending(post => post.LastCommentDate)`**:
   - Orders the posts based on the `LastCommentDate` in descending order, so the post with the most recent comment appears first.

3. **`ToList()`**:
   - Converts the result into a list for easier iteration and output.

#### **Output**
The result is a list of objects, where each object contains:
- `PostTitle`: The title of the blog post.
- `LastCommentDate`: The date of the latest comment for that post.

Example Output:
```
Post2: '2020-03-06'
Post1: '2020-03-05'
Post3: '2020-02-14'
```

---

### **Query 3: How many last comments each user left?**

#### **Purpose**
This query identifies the latest comment for each blog post, groups these "last comments" by the user who made them, and counts the number of last comments per user.

#### **Code**
```csharp
var lastCommentsPerUser = context.BlogPosts
    .Select(post => post.Comments
        .OrderByDescending(comment => comment.CreatedDate) // Sort comments by CreatedDate (descending)
        .FirstOrDefault()) // Take the latest comment for this post
    .Where(comment => comment != null) // Filter out any null values (posts without comments)
    .GroupBy(comment => comment.UserName) // Group the latest comments by UserName
    .Select(group => new {
        UserName = group.Key, // The user's name
        LastCommentCount = group.Count() // Count the number of last comments for this user
    })
    .ToList();
```

#### **Explanation**
1. **`Select(post => post.Comments.OrderByDescending(...).FirstOrDefault())`**:
   - For each blog post, it sorts the comments by `CreatedDate` in descending order and selects the latest comment using `FirstOrDefault()`.
   - If a post has no comments, `FirstOrDefault()` will return `null`.

2. **`Where(comment => comment != null)`**:
   - Filters out any `null` values (i.e., posts without comments).

3. **`GroupBy(comment => comment.UserName)`**:
   - Groups the latest comments by the `UserName` property.
   - This groups all "last comments" made by the same user together.

4. **`Select(group => new { ... })`**:
   - For each group of comments (one group per user), it creates an anonymous object containing:
     - `UserName`: The name of the user.
     - `LastCommentCount`: The total number of last comments made by that user.

5. **`ToList()`**:
   - Converts the result into a list for easier iteration and output.

#### **Output**
The result is a list of objects, where each object contains:
- `UserName`: The name of the user.
- `LastCommentCount`: The total number of last comments made by that user.

Example Output:
```
Ivan: 2
Petr: 1
```

---

### **Summary of Queries**

| Query Number | Metric Calculated                                                                 | Key Operations Used                                                                 |
|--------------|-----------------------------------------------------------------------------------|-------------------------------------------------------------------------------------|
| **Query 1**  | Total number of comments per user.                                               | `SelectMany`, `GroupBy`, `Count`                                                    |
| **Query 2**  | Blog posts ordered by the date of their latest comment.                          | `Select`, `Max`, `OrderByDescending`                                                |
| **Query 3**  | Total number of "last comments" per user (latest comment per post).              | `OrderByDescending`, `FirstOrDefault`, `Where`, `GroupBy`, `Count`                  |

These queries demonstrate the power of LINQ in processing complex data relationships and calculating meaningful insights from the database. Each query is efficient, concise, and leverages LINQ's ability to transform and aggregate data seamlessly.

---
![image](https://github.com/user-attachments/assets/1b0902ed-545c-4169-a672-135a87147e67)

---

