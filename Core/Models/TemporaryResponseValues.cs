
namespace Core.Models
{
    public class TemporaryResponseValues: Response
    {
        public decimal TotalCapacity { get; set; }
        public decimal LoadGoal { get; set; }
        public decimal TotalCost { get; set; }

        public TemporaryResponseValues(decimal loadGoal, decimal totalCapacity = 0.0m, decimal totalCost = 0.0m)
        {
            TotalCapacity = totalCapacity;
            LoadGoal = loadGoal;
            TotalCost = totalCost;
        }
    }
}
