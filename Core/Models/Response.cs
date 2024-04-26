
namespace Core.Models
{
    public class Response
    {
        public List<PowerplantResponse> PowerplantResponses { get; set; }

        public Response()
        {
            PowerplantResponses= new List<PowerplantResponse>();    
        }
    }
}
