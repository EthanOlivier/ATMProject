namespace ATMProject.Application.Operations.Authorization
{
    // must be used on a class or interface declaration
    // note that | is not || (or), instead it is using bitwise
    // math to add two enum values together. 
    //
    // This comes down to binary and how integers as well
    // as enum values are stored as binary
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class RequiresAdminAttribute : Attribute;
}
