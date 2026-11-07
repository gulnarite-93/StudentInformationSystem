namespace StudentInformationSystem.API.Services.Interfaces
{
    public interface IGpaService
    {
        /// <summary>
        /// 100 ballıq sistemdə toplanmış yekun balı hərf qiymətinə (A, B, C, D, E, F) 
        /// və 4.0 şkalasındakı əmsala (Grade Point) çevirir.
        /// </summary>
        (string LetterGrade, decimal GradePoint, string Description) CalculateLetterGradeAndPoint(decimal totalScore);

        /// <summary>
        /// Tələbənin qiymətləndirilmiş fənləri üzrə ümumi orta müvəffəqiyyət göstəricisini (GPA) hesablayır:
        /// GPA = SUM(GradePoint * Credits) / SUM(Credits)
        /// </summary>
        decimal CalculateGpa(IEnumerable<(decimal GradePoint, int Credits)> completedCourses);
    }
}
