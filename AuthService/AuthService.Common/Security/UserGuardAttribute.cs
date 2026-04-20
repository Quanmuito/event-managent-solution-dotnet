namespace AuthService.Common.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class UserGuardAttribute : Attribute
{
}
