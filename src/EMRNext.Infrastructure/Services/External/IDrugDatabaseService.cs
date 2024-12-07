using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Services.External
{
    public interface IDrugDatabaseService
    {
        Task<DrugInfo> GetDrugInfoAsync(string ndc);
        Task<IEnumerable<DrugInteraction>> CheckInteractionsAsync(IEnumerable<string> ndcList);
        Task<IEnumerable<DrugAllergy>> CheckAllergyInteractionsAsync(string ndc, IEnumerable<string> allergyList);
        Task<IEnumerable<DrugInfo>> SearchDrugsAsync(string query, int limit = 10);
        Task<DrugFormulary> GetFormularyInfoAsync(string ndc, string insurancePlanId);
    }

    public class DrugInfo
    {
        public string NDC { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Form { get; set; }
        public string Strength { get; set; }
        public string[] ActiveIngredients { get; set; }
        public string[] TherapeuticClasses { get; set; }
        public bool IsControlled { get; set; }
        public string ScheduleClass { get; set; }
        public string[] Warnings { get; set; }
        public string[] Contraindications { get; set; }
    }

    public class DrugInteraction
    {
        public string Drug1NDC { get; set; }
        public string Drug2NDC { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string ClinicalEffect { get; set; }
        public string RecommendedAction { get; set; }
    }

    public class DrugAllergy
    {
        public string DrugNDC { get; set; }
        public string AllergyCode { get; set; }
        public string Severity { get; set; }
        public string ReactionType { get; set; }
        public string Description { get; set; }
    }

    public class DrugFormulary
    {
        public string NDC { get; set; }
        public string InsurancePlanId { get; set; }
        public string Tier { get; set; }
        public bool RequiresPA { get; set; }
        public bool RequiresStepTherapy { get; set; }
        public decimal CopayAmount { get; set; }
        public string[] Restrictions { get; set; }
    }
}
