namespace TestUtilities.Helpers;

using FluentAssertions;

public static class ServiceTestHelper
{
    public static void AssertDtoMatchesEntity<TDto, TEntity>(TDto dto, TEntity entity, params string[] propertiesToCompare)
        where TDto : class
        where TEntity : class
    {
        dto.Should().NotBeNull();
        entity.Should().NotBeNull();

        if (propertiesToCompare.Length == 0)
        {
            var dtoType = typeof(TDto);
            var entityType = typeof(TEntity);

            foreach (var dtoProperty in dtoType.GetProperties())
            {
                var entityProperty = entityType.GetProperty(dtoProperty.Name);
                if (entityProperty != null && dtoProperty.PropertyType == entityProperty.PropertyType)
                {
                    var dtoValue = dtoProperty.GetValue(dto);
                    var entityValue = entityProperty.GetValue(entity);
                    dtoValue.Should().Be(entityValue, $"Property {dtoProperty.Name} should match");
                }
            }
        }
        else
        {
            var dtoType = typeof(TDto);
            var entityType = typeof(TEntity);

            foreach (var propertyName in propertiesToCompare)
            {
                var dtoProperty = dtoType.GetProperty(propertyName);
                var entityProperty = entityType.GetProperty(propertyName);

                if (dtoProperty != null && entityProperty != null)
                {
                    var dtoValue = dtoProperty.GetValue(dto);
                    var entityValue = entityProperty.GetValue(entity);
                    dtoValue.Should().Be(entityValue, $"Property {propertyName} should match");
                }
            }
        }
    }

}
