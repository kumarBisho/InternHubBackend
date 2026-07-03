// using AutoMapper;
// using AutoMapperProfile = AutoMapper.Profile;
// using InternMS.Domain.Entities;

// namespace InternMS.Api.DTOs
// {
//     public class MappingProfile : AutoMapperProfile
//     {
//         private object src;

//         public MappingProfile()
//         {
//             CreateMap<User, UserDto>()
//                 .ForMember(dest => dest.Role, opt => opt.MapFrom(src.UserRoles != null && src.UserRoles.count > 0 ? 
//                 src.UserRoles.First().Role.Name: null));

//             CreateMap<CreateUserDto, User>();
//             CreateMap<Profile, ProfileDto>();
//             CreateMap<UpdateProfileDto, Profile>();
//             CreateMap<Project, ProjectDto>()
//                 .FroMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

//             CreateMap<CreateProjectDto, Project>();
//             CreateMap<ProjectUpdate, ProjectUpdateDto>()
//                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

//             CreateMap<CreateProjectUpdateDto, ProjectUpdate>();
//             CreateMap<Notification, NotificationDto>();
//         }
//     }
// }

using AutoMapper;
using AutoMapperProfile = AutoMapper.Profile;
using InternMS.Domain.Entities;
using InternMS.Domain.Enums;
using System.Linq;
using InternMS.Api.DTOs.Tasks;
using InternMS.Api.DTOs.Users;
using InternMS.Api.DTOs.Profiles;
using InternMS.Api.DTOs.Projects;
using InternMS.Api.DTOs.Notifications;
using InternMS.Api.DTOs.Feedback;

namespace InternMS.Api.DTOs
{
    public class MappingProfile : AutoMapperProfile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role,
                opt => opt.MapFrom(
                    src => src.UserRoles != null && src.UserRoles.Any() ? src.UserRoles.First().Role.Name : null
                ))
                .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(src => src.Profile != null ? src.Profile.Phone : null))
                .ForMember(dest => dest.Department,
                opt => opt.MapFrom(src => src.Profile != null ? src.Profile.Department : null));

            CreateMap<CreateUserDto, User>();

            // Skill mappings
            CreateMap<Skill, SkillDto>().ReverseMap();

            // User profile mappings
            CreateMap<UserProfile, ProfileDto>()
                .ForMember(dest => dest.Skills,
                    opt => opt.MapFrom(src => src.Skills.Select(s => new SkillDto { Id = s.Id, Name = s.Name, Level = s.Level }).ToList()));

            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TechStack,
                opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.TechStack) 
                        ? new List<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(src.TechStack) ?? new List<string>()
                ));

            CreateMap<CreateProjectDto, Project>();

            CreateMap<ProjectUpdate, ProjectUpdateDto>()
                .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateProjectUpdateDto, ProjectUpdate>();

            CreateMap<Notification, NotificationDto>();

            CreateMap<ProjectTask, TaskDto>()
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<ProjectTask, TaskListDto>()
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateTaskDto, ProjectTask>()
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src =>
                        Enum.Parse<Priority>(src.Priority, true)))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => ProjectTaskStatus.Active))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateTaskDto, ProjectTask>()
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src =>
                        src.Priority != null
                            ? Enum.Parse<Priority>(src.Priority, true)
                            : default))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src =>
                        src.Status != null
                            ? Enum.Parse<ProjectTaskStatus>(src.Status, true)
                            : default))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, value) => value != null));

            CreateMap<ProjectTaskAssignment, TaskAssignmentDto>()
                .ForMember(dest => dest.InternName,
                    opt => opt.MapFrom(src => src.Intern.FirstName + " " + src.Intern.LastName))
                .ForMember(dest => dest.InternEmail,
                    opt => opt.MapFrom(src => src.Intern.Email));

            CreateMap<ProjectTask, AssignedTaskDto>()
                .ForMember(dest => dest.Priority,
                    opt => opt.MapFrom(src => src.Priority.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Assignments,
                    opt => opt.MapFrom(src => src.Assignments));

            // Feedback mappings
            CreateMap<UserFeedback, FeedbackDto>()
                .ForMember(dest => dest.MentorName,
                    opt => opt.MapFrom(src => src.Mentor.FirstName + " " + src.Mentor.LastName))
                .ForMember(dest => dest.InternName,
                    opt => opt.MapFrom(src => src.Intern.FirstName + " " + src.Intern.LastName))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.TaskTitle,
                    opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : null))
                .ForMember(dest => dest.ProjectTitle,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : null));

            CreateMap<UserFeedback, FeedbackListDto>()
                .ForMember(dest => dest.MentorName,
                    opt => opt.MapFrom(src => src.Mentor.FirstName + " " + src.Mentor.LastName))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.IsUnread,
                    opt => opt.MapFrom(src => false)); // Can be extended to track read status

            CreateMap<CreateFeedbackDto, UserFeedback>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => Enum.Parse<FeedbackType>(src.Type, true)));

            CreateMap<UpdateFeedbackDto, UserFeedback>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, value) => value != null));
                    
        }
    }
}