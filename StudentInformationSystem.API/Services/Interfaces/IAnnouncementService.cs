using StudentInformationSystem.API.DTOs.Announcements;

namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IAnnouncementService
    {
        /// <summary>
        /// Hazırkı istifadəçinin roluna və təhkim/qeydiyyat olunduğu kurslara uyğun elanlar siyahısı (yenidən köhnəyə)
        /// </summary>
        Task<IReadOnlyList<AnnouncementDto>> GetAnnouncementsAsync(int currentUserId, string currentUserRole);

        /// <summary>
        /// Tək bir elanın detalları
        /// </summary>
        Task<AnnouncementDto?> GetByIdAsync(int id, int currentUserId, string currentUserRole);

        /// <summary>
        /// Yeni elan yaratmaq (Müəssisə üzrə yalnız Admin; Kursa aid müəllim və ya Admin)
        /// </summary>
        Task<(bool Success, string? ErrorMessage, int? AnnouncementId)> CreateAnnouncementAsync(AnnouncementCreateDto dto, int currentUserId, string currentUserRole);

        /// <summary>
        /// Mövcud elanı redaktə etmək (Yalnız müəllif və ya Admin)
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> UpdateAnnouncementAsync(int id, AnnouncementUpdateDto dto, int currentUserId, string currentUserRole);

        /// <summary>
        /// Elanı silmək (Yalnız müəllif və ya Admin)
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> DeleteAnnouncementAsync(int id, int currentUserId, string currentUserRole);

        /// <summary>
        /// İstifadəçinin elan yarada biləcəyi kurslar siyahısı
        /// </summary>
        Task<IReadOnlyList<AnnouncementCourseOptionDto>> GetCreatableCoursesAsync(int currentUserId, string currentUserRole);
    }
}
