using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentOnboard.Api.Data;
using StudentOnboard.Api.Models;

namespace StudentOnboard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StudentsController(AppDbContext db)
        {
            _db = db;
        }

        // POST: api/students (Android app uses this)
        [HttpPost]
        public async Task<IActionResult> CreateStudent(StudentProfile student)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _db.StudentProfiles.Add(student);
            await _db.SaveChangesAsync();

            return Ok(student);
        }

        // GET: api/students (Web admin uses this)
        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _db.StudentProfiles.ToListAsync();
            return Ok(students);
        }
    }
}
