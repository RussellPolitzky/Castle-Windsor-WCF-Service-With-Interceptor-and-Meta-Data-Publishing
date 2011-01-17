using System.ServiceModel;

namespace UsingCastlesWcfFacility
{
    [ServiceContract]
    public interface IOperations
    {
        [OperationContract]
        int GetValueFromConstructor();
    }
}