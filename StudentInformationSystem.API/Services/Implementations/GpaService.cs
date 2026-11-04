using StudentInformationSystem.API.Services.Interfaces;

namespace StudentInformationSystem.API.Services.Implementations
{
    /// <summary>
    /// GPA (Grade Point Average) və Hərf Qiymətlərinin Hesablanması Xidməti.
    /// Bu sinif Bolonya təhsil sistemi və Azərbaycan Respublikasının ali təhsil 
    /// müəssisələrində tətbiq olunan 100 ballıq sistemin 4.0 şkalasına çevrilmə qaydasını tətbiq edir.
    /// </summary>
    public class GpaService : IGpaService
    {
        /*
         ========================================================================
         AKADEMİK QİYMƏTLƏNDİRMƏ VƏ 4.0 GPA ŞKALASI QAYDALARI:
         ------------------------------------------------------------------------
         Yekun Bal (0-100) | Hərf | 4.0 Əmsalı (Grade Point) | İzah
         ------------------+------+--------------------------+-------------------
         91.00 - 100.00    |  A   |          4.00            | Əla (Excellent)
         81.00 -  90.99    |  B   |          3.00            | Çox yaxşı (Very Good)
         71.00 -  80.99    |  C   |          2.00            | Yaxşı (Good)
         61.00 -  70.99    |  D   |          1.50            | Kafi (Satisfactory)
         51.00 -  60.99    |  E   |          1.00            | Qənaətbəxş (Sufficient)
          0.00 -  50.99    |  F   |          0.00            | Qeyri-məqbul / Kəsr (Fail)
         ========================================================================
         Keçid balı: Minimum 51 baldır. 51-dən aşağı toplayan tələbə fəndən kəsilmiş sayılır (F).
        */

        public (string LetterGrade, decimal GradePoint, string Description) CalculateLetterGradeAndPoint(decimal totalScore)
        {
            // Bal intervalını 0 ilə 100 arasında saxlayırıq
            if (totalScore < 0) totalScore = 0;
            if (totalScore > 100) totalScore = 100;

            if (totalScore >= 91.00m)
            {
                return ("A", 4.00m, "Əla");
            }
            if (totalScore >= 81.00m)
            {
                return ("B", 3.00m, "Çox yaxşı");
            }
            if (totalScore >= 71.00m)
            {
                return ("C", 2.00m, "Yaxşı");
            }
            if (totalScore >= 61.00m)
            {
                return ("D", 1.50m, "Kafi");
            }
            if (totalScore >= 51.00m)
            {
                return ("E", 1.00m, "Qənaətbəxş");
            }

            return ("F", 0.00m, "Qeyri-məqbul (Kəsr)");
        }

        /// <summary>
        /// GPA Düsturu:
        /// GPA = ( (Kurs_1_Əmsalı * Kredit_1) + (Kurs_2_Əmsalı * Kredit_2) + ... ) / (Kredit_1 + Kredit_2 + ...)
        /// Əgər tələbənin hələ qiymətləndirilmiş fənni yoxdursa və ya ümumi kredit 0-dırsa, GPA = 0.00 qayıdır.
        /// </summary>
        public decimal CalculateGpa(IEnumerable<(decimal GradePoint, int Credits)> completedCourses)
        {
            if (completedCourses == null) return 0.00m;

            var list = completedCourses.ToList();
            if (!list.Any()) return 0.00m;

            decimal totalWeightedPoints = 0.00m;
            int totalCredits = 0;

            foreach (var (gradePoint, credits) in list)
            {
                if (credits <= 0) continue;

                totalWeightedPoints += gradePoint * credits;
                totalCredits += credits;
            }

            if (totalCredits == 0) return 0.00m;

            // Nəticəni 2 onluq kəsrə qədər riyazi yuvarlaqlaşdırırıq (məsələn, 3.45)
            var rawGpa = totalWeightedPoints / totalCredits;
            return Math.Round(rawGpa, 2, MidpointRounding.AwayFromZero);
        }
    }
}
