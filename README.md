# StudentOnboard
Student on-boarding system 
is a **clean, scalable .NET Web API project structure using Dapper** with **Microsoft SQL Server** as the database. This is a **production-style layout** you can drop into your 


---

# 🗂️ Recommended Folder Structure

```
StudentOnboard/
└── api/
    └── StudentOnboard.Api/
        ├── Controllers/
        │   └── StudentsController.cs
        │
        ├── Data/
        │   ├── DbConnectionFactory.cs
        │   └── DbInitializer.cs
        │
        ├── Models/
        │   └── StudentProfile.cs
        │
        ├── Repositories/
        │   ├── IStudentRepository.cs
        │   └── StudentRepository.cs
        │
        ├── Services/
        │   ├── IStudentService.cs
        │   └── StudentService.cs
        │
        ├── appsettings.json
        ├── Program.cs
        └── StudentOnboard.Api.csproj
```

**Why this is clean:**

* `Controllers` → HTTP layer (API endpoints)
* `Services` → Business logic
* `Repositories` → Data access (Dapper + SQL)
* `Data` → DB connection handling
* `Models` → Plain C# objects (POCOs)

---

# 📦 1️⃣ Install Packages

Inside `StudentOnboard.Api`:

```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
```

---

# 🧱 2️⃣ Model

### `Models/StudentProfile.cs`

```csharp
public class StudentProfile
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }
    public string EducationBackground { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

# 🔌 3️⃣ DB Connection Factory

### `Data/DbConnectionFactory.cs`

```csharp
using System.Data;
using Microsoft.Data.SqlClient;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
```

---

# 🗃️ 4️⃣ Repository Layer (Dapper)

### `Repositories/IStudentRepository.cs`

```csharp
public interface IStudentRepository
{
    Task AddStudentAsync(StudentProfile student);
    Task<IEnumerable<StudentProfile>> GetAllAsync();
}
```

### `Repositories/StudentRepository.cs`

```csharp
using Dapper;

public class StudentRepository : IStudentRepository
{
    private readonly DbConnectionFactory _dbFactory;

    public StudentRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task AddStudentAsync(StudentProfile student)
    {
        const string sql = @"
            INSERT INTO StudentProfiles
            (FullName, Email, DateOfBirth, Address, EducationBackground, CreatedAt)
            VALUES
            (@FullName, @Email, @DateOfBirth, @Address, @EducationBackground, GETUTCDATE());
        ";

        using var conn = _dbFactory.CreateConnection();
        await conn.ExecuteAsync(sql, student);
    }

    public async Task<IEnumerable<StudentProfile>> GetAllAsync()
    {
        const string sql = "SELECT * FROM StudentProfiles ORDER BY CreatedAt DESC";

        using var conn = _dbFactory.CreateConnection();
        return await conn.QueryAsync<StudentProfile>(sql);
    }
}
```

---

# 🧠 5️⃣ Service Layer (Business Logic)

### `Services/IStudentService.cs`

```csharp
public interface IStudentService
{
    Task AddStudentAsync(StudentProfile student);
    Task<IEnumerable<StudentProfile>> GetAllAsync();
}
```

### `Services/StudentService.cs`

```csharp
public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo)
    {
        _repo = repo;
    }

    public async Task AddStudentAsync(StudentProfile student)
    {
        if (string.IsNullOrWhiteSpace(student.FullName))
            throw new ArgumentException("Full name is required");

        if (string.IsNullOrWhiteSpace(student.Email))
            throw new ArgumentException("Email is required");

        await _repo.AddStudentAsync(student);
    }

    public Task<IEnumerable<StudentProfile>> GetAllAsync()
    {
        return _repo.GetAllAsync();
    }
}
```

---

# 🌐 6️⃣ API Controller

### `Controllers/StudentsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    // POST: api/students (Android app)
    [HttpPost]
    public async Task<IActionResult> Create(StudentProfile student)
    {
        try
        {
            await _service.AddStudentAsync(student);
            return Ok(new { message = "Student saved successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/students (Web admin)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _service.GetAllAsync();
        return Ok(students);
    }
}
```

---

# ⚙️ 7️⃣ Program.cs (DI Wiring)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register dependencies
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
```

---

# 🗄️ 8️⃣ appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=StudentOnboardDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

---

# 🧾 9️⃣ SQL Table

Run in SQL Server:

```sql
CREATE TABLE StudentProfiles (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    EducationBackground NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
```

---

# 📱 How This Fits Your App

* Android App → `POST /api/students`
* Web Admin → `GET /api/students`
* API → Dapper → SQL Server

---

# ✅ Benefits of This Structure

* Clean separation of concerns
* Easy to test and maintain
* Fast (Dapper + raw SQL)
* Easy to add:

  * 🔐 JWT Auth
  * 👩‍💼 Admin roles
  * ✏ Update/Delete APIs
  * 📄 Paging & search

---

If you want, I can next generate:

* **JWT Login + Role-based access**
* **Admin Web UI (HTML/React/Blazor)**
* **Update/Delete student APIs**
* **Search & pagination for admin screen**
