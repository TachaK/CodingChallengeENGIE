using Core.Models;

namespace Core.Interface
{
    // Interface for the main solution 
    public interface IComputeSolution
    {
        Response Solution(Payload payload);
    }
}
