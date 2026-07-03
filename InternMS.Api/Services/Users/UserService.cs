using InternMS.Api.Services;
using InternMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InternMS.Domain.Entities;
using InternMS.Api.DTOs.Users;
using InternMS.Api.DTOs.Profiles;
using InternMS.Api.DTOs.Common;
using InternMS.Api.DTOs.Projects;
using InternMS.Api.Services.Pagination;
using InternMS.Api.Utils;
using AutoMapper;

namespace InternMS.Api.Services.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public UserService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Profile).ThenInclude(p => p.Skills)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all users paginated with default page size of 20
        /// </summary>
        public async Task<PaginatedResponse<UserDto>> GetAllUsersPaginatedAsync(int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var totalCount = await _db.Users.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var users = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Profile).ThenInclude(p => p.Skills)
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return PaginatedResponse<UserDto>.Create(userDtos, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Profile).ThenInclude(p => p.Skills)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateUserAsync(Guid id, UserDto updatedUser)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            
            if( user == null)
            {
                throw new Exception("User not found");
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.IsActive = updatedUser.IsActive;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public async Task<UserProfile?> GetProfileAsync(Guid userId)
        {
            return await _db.Profiles
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<UserProfileDetailDto?> GetUserProfileDetailAsync(Guid userId)
        {
            var user = await _db.Users
                .Include(u => u.Profile)
                .ThenInclude(p => p.Skills)
                .Include(u => u.UserRoles)  // Include roles
                .ThenInclude(ur => ur.Role)  // Include role details
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            // Get the primary role (first role if multiple)
            var userRole = user.UserRoles?.FirstOrDefault()?.Role?.Name ?? null;

            var profileDetail = new UserProfileDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = userRole,  // Set the role
                Phone = user.Profile?.Phone,
                Department = user.Profile?.Department,
                Position = user.Profile?.Position,
                Bio = user.Profile?.Bio,
                StartDate = user.Profile?.StartDate,
                EndDate = user.Profile?.EndDate,
                Interests = user.Profile?.Interests,
                ProfileImageUrl = user.Profile?.ProfileImageUrl,
                Skills = user.Profile?.Skills?.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level
                }).ToList() ?? new List<SkillDto>()
            };

            return profileDetail;
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var profile = await _db.Profiles
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _db.Profiles.Add(profile);
            }

            // Update basic fields - only if provided
            if (!string.IsNullOrEmpty(dto.Phone))
                profile.Phone = dto.Phone;
            if (!string.IsNullOrEmpty(dto.Department))
                profile.Department = dto.Department;
            if (!string.IsNullOrEmpty(dto.Position))
                profile.Position = dto.Position;
            if (!string.IsNullOrEmpty(dto.Bio))
                profile.Bio = dto.Bio;
            if (dto.StartDate.HasValue)
                profile.StartDate = dto.StartDate;
            if (dto.EndDate.HasValue)
                profile.EndDate = dto.EndDate;
            if (!string.IsNullOrEmpty(dto.Interests))
                profile.Interests = dto.Interests;
            if (!string.IsNullOrEmpty(dto.ProfileImageUrl))
                profile.ProfileImageUrl = dto.ProfileImageUrl;

            // Handle skills update
            if (dto.Skills != null)
            {
                // Remove existing skills
                if (profile.Skills.Any())
                {
                    foreach (var skill in profile.Skills.ToList())
                    {
                        _db.Skills.Remove(skill);
                    }
                }

                // Add new skills
                foreach (var skillDto in dto.Skills)
                {
                    var skill = new Skill
                    {
                        Id = Guid.NewGuid(),
                        Name = skillDto.Name,
                        Level = skillDto.Level,
                        UserProfileId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    profile.Skills.Add(skill);
                    _db.Skills.Add(skill);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetInternsByMentorAsync(Guid mentorId)
        {
            Console.WriteLine($"\n=== GetInternsByMentorAsync START ===");
            Console.WriteLine($"Looking for interns assigned to mentor: {mentorId}");
            
            try
            {
                // Check total ProjectAssignments in database
                var totalAssignments = await _db.ProjectAssignments.CountAsync();
                Console.WriteLine($"Total ProjectAssignments in database: {totalAssignments}");
                
                // Get all assignments for this mentor
                var assignmentsForMentor = await _db.ProjectAssignments
                    .Where(pa => pa.MentorId == mentorId)
                    .ToListAsync();
                
                Console.WriteLine($"ProjectAssignments found for mentorId {mentorId}: {assignmentsForMentor.Count()}");
                foreach (var pa in assignmentsForMentor)
                {
                    Console.WriteLine($"  - ProjectId: {pa.ProjectId}, InternId: {pa.InternId}");
                }
                
                // Now get the interns with full data
                var interns = await _db.ProjectAssignments
                    .Where(pa => pa.MentorId == mentorId)
                    .Include(pa => pa.Intern)
                    .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(pa => pa.Intern)
                    .ThenInclude(u => u.Profile)
                    .ThenInclude(p => p.Skills)
                    .Select(pa => pa.Intern)
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine($"Distinct interns fetched: {interns.Count}");
                foreach (var intern in interns)
                {
                    Console.WriteLine($"  - {intern.FirstName} {intern.LastName} (ID: {intern.Id})");
                }
                
                Console.WriteLine($"=== GetInternsByMentorAsync END ===\n");
                return interns;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetInternsByMentorAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"=== GetInternsByMentorAsync END (ERROR) ===\n");
                throw;
            }
        }

        /// <summary>
        /// Gets interns assigned to a mentor with pagination
        /// </summary>
        public async Task<PaginatedResponse<UserDto>> GetInternsByMentorPaginatedAsync(Guid mentorId, int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var query = _db.ProjectAssignments
                .Where(pa => pa.MentorId == mentorId)
                .Include(pa => pa.Intern)!
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(pa => pa.Intern)!
                .ThenInclude(u => u.Profile)!
                .ThenInclude(p => p.Skills)
                .Where(pa => pa.Intern != null)
                .Select(pa => pa.Intern);

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var interns = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(interns);

            return PaginatedResponse<UserDto>.Create(userDtos, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<IEnumerable<User>> GetInternsByMentorAndProjectAsync(Guid mentorId, Guid projectId)
        {
            Console.WriteLine($"\n=== GetInternsByMentorAndProjectAsync START ===");
            Console.WriteLine($"MentorId: {mentorId}, ProjectId: {projectId}");
            
            try
            {
                // Get interns assigned to this mentor for the specific project
                var interns = await _db.ProjectAssignments
                    .Where(pa => pa.MentorId == mentorId && pa.ProjectId == projectId && pa.Intern != null)
                    .Include(pa => pa.Intern)!
                    .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(pa => pa.Intern)!
                    .ThenInclude(u => u.Profile)!
                    .ThenInclude(p => p.Skills)
                    .Select(pa => pa.Intern)
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine($"Interns found for mentor {mentorId} on project {projectId}: {interns.Count}");
                foreach (var intern in interns)
                {
                    Console.WriteLine($"  - {intern!.FirstName} {intern.LastName} (ID: {intern.Id})");
                }
                
                Console.WriteLine($"=== GetInternsByMentorAndProjectAsync END ===");
                return interns;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetInternsByMentorAndProjectAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<User?> GetAssignedMentorAsync(Guid internId)
        {
            Console.WriteLine($"\n=== GetAssignedMentorAsync called ===");
            Console.WriteLine($"Looking for mentor for intern: {internId}");
            
            // First, check if any ProjectAssignments exist for this intern
            var projectAssignmentCount = await _db.ProjectAssignments
                .Where(pa => pa.InternId == internId)
                .CountAsync();
            Console.WriteLine($"ProjectAssignments found for this intern: {projectAssignmentCount}");
            
            if (projectAssignmentCount == 0)
            {
                Console.WriteLine($"❌ No ProjectAssignment row exists for intern {internId}");
                Console.WriteLine("Total ProjectAssignments in database:");
                var totalAssignments = await _db.ProjectAssignments.CountAsync();
                Console.WriteLine($"  Total: {totalAssignments}");
                
                // Show some sample data for debugging
                var sampleAssignments = await _db.ProjectAssignments
                    .Take(5)
                    .Select(pa => new { pa.Id, pa.ProjectId, pa.InternId, pa.MentorId })
                    .ToListAsync();
                Console.WriteLine("Sample ProjectAssignments:");
                foreach (var sa in sampleAssignments)
                {
                    Console.WriteLine($"  - ProjectId: {sa.ProjectId}, InternId: {sa.InternId}, MentorId: {sa.MentorId}");
                }
                
                return null;
            }
            
            var mentor = await _db.ProjectAssignments
                .Where(pa => pa.InternId == internId)
                .Include(pa => pa.Mentor)!
                .ThenInclude(m => m.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(pa => pa.Mentor)!
                .ThenInclude(m => m.Profile)!
                .ThenInclude(p => p.Skills)
                .Select(pa => pa.Mentor)
                .FirstOrDefaultAsync();

            if (mentor == null)
            {
                Console.WriteLine($"⚠️ ProjectAssignment exists but Mentor navigation property is null");
            }
            else
            {
                Console.WriteLine($"✓ Mentor found: {mentor!.FirstName} {mentor.LastName} (ID: {mentor.Id})");
            }
            
            return mentor;
        }

        public async Task<IEnumerable<ProjectAssignment>> GetAllProjectAssignmentsAsync()
        {
            return await _db.ProjectAssignments.ToListAsync();
        }

        /// <summary>
        /// Gets all project assignments paginated
        /// </summary>
        public async Task<PaginatedResponse<ProjectAssignmentDto>> GetAllProjectAssignmentsPaginatedAsync(int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var totalCount = await _db.ProjectAssignments.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var assignments = await _db.ProjectAssignments
                .Include(pa => pa.Project)
                .Include(pa => pa.Intern)
                .Include(pa => pa.Mentor)
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            var assignmentDtos = assignments.Select(a => new ProjectAssignmentDto
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                ProjectTitle = a.Project?.Title ?? "Unknown",
                InternId = a.InternId,
                InternName = a.Intern != null ? $"{a.Intern.FirstName} {a.Intern.LastName}" : "Unknown",
                MentorId = a.MentorId,
                MentorName = a.Mentor != null ? $"{a.Mentor.FirstName} {a.Mentor.LastName}" : "Unassigned",
                AssignedAt = a.AssignedAt
            }).ToList();

            return PaginatedResponse<ProjectAssignmentDto>.Create(assignmentDtos, validatedPageNumber, validatedPageSize, totalCount);
        }

        public async Task<IEnumerable<User>> GetAllMentorsAndInternsAsync()
        {
            return await _db.Users
                .Include(u => u.UserRoles)!.ThenInclude(ur => ur.Role)
                .Include(u => u.Profile)!.ThenInclude(p => p.Skills)
                .Where(u => u.UserRoles != null && u.UserRoles.Any(ur => ur.Role != null && (ur.Role!.Name == "Mentor" || ur.Role.Name == "Intern")))
                .ToListAsync();
        }

        /// <summary>
        /// Gets all mentors and interns paginated
        /// </summary>
        public async Task<PaginatedResponse<UserDto>> GetAllMentorsAndInternsPaginatedAsync(int pageNumber = 1, int pageSize = 20)
        {
            var (validatedPageNumber, validatedPageSize) = PaginationHelper.ValidateAndNormalize(pageNumber, pageSize);
            var skip = PaginationHelper.CalculateSkip(validatedPageNumber, validatedPageSize);

            var query = _db.Users
                .Include(u => u.UserRoles)!
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Profile)!
                .ThenInclude(p => p.Skills)
                .Where(u => u.UserRoles != null && u.UserRoles.Any(ur => ur.Role != null && (ur.Role!.Name == "Mentor" || ur.Role!.Name == "Intern")));

            var totalCount = await query.CountAsync();
            var totalPages = PaginationHelper.CalculateTotalPages(totalCount, validatedPageSize);

            var users = await query
                .Skip(skip)
                .Take(validatedPageSize)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return PaginatedResponse<UserDto>.Create(userDtos, validatedPageNumber, validatedPageSize, totalCount);
        }
    }
}