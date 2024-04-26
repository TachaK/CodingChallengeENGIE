
namespace Core.Models
{
    public class TemporaryResponseValues: Response
    {
        public decimal TotalCapacity { get; set; }
        public decimal LoadGoal { get; set; }
        public decimal TotalCost { get; set; }

        public TemporaryResponseValues(decimal totalCapacity, decimal loadGoal, decimal totalCost)
        {
            TotalCapacity = totalCapacity;
            LoadGoal = loadGoal;
            TotalCost = totalCost;
        }
    }
}
