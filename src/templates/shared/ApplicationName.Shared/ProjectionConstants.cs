namespace ApplicationName.Shared;

public static class ProjectionConstants
{
    public const string ExampleKey = "example";

    public const string ExampleProjectionNamespace = $"{ExampleKey}:projection";

    public static string ExampleProjectionKey(Guid id) => $"{ExampleProjectionNamespace}:{id:N}";
}