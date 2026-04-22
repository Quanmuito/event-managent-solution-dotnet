namespace AuthService.Common.Security.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class OrganizerGuardAttribute : Attribute
{
}
