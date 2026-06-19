using System.Linq.Expressions;
using System.Text.Json;
using ApplicationName.Worker.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Worker.Test.Infrastructure;

public class JsonbPredicateTranslatorTest
{
    private sealed class TestDocument
    {
        public Guid Id { get; init; }

        public DateTime Created { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Code { get; init; } = string.Empty;

        public Guid ExternalId { get; init; }

        public DateTime Occurred { get; init; }

        public ChildDocument Child { get; init; } = new();

        public int RemoteCode { get; init; }

        public double Value { get; init; }
    }

    private sealed class ChildDocument
    {
        public Guid Id { get; init; }
    }

    private sealed class ValueHolder
    {
        public string Name { get; init; } = string.Empty;
    }

    [Test]
    public void Translate_Id_Equality_Maps_To_Real_Column_With_Native_Guid()
    {
        // Arrange
        var id = Guid.NewGuid();
        Expression<Func<TestDocument, bool>> predicate = i => i.Id == id;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("id = @p0");
        parms.Length.ShouldBe(1);
        parms[0].ParameterName.ShouldBe("p0");
        parms[0].Value.ShouldBe(id);
    }

    [Test]
    public void Translate_Created_Equality_Maps_To_Real_Column_With_Native_DateTime()
    {
        // Arrange
        var created = new DateTime(2026, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        Expression<Func<TestDocument, bool>> predicate = i => i.Created == created;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("created = @p0");
        parms.Length.ShouldBe(1);
        parms[0].ParameterName.ShouldBe("p0");
        parms[0].Value.ShouldBe(created);
    }

    [Test]
    public void Translate_String_Member_Maps_To_Jsonb_Text_Extraction()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name == "x";

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'Name' = @p0");
        parms.Length.ShouldBe(1);
        parms[0].ParameterName.ShouldBe("p0");
        parms[0].Value.ShouldBe("x");
    }

    [Test]
    public void Translate_AndAlso_Combines_Both_Operands()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name == "a" && i.Code == "b";

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("(data ->> 'Name' = @p0 AND data ->> 'Code' = @p1)");
        parms.Length.ShouldBe(2);
        parms[0].Value.ShouldBe("a");
        parms[1].Value.ShouldBe("b");
    }

    [Test]
    public void Translate_Guid_Jsonb_Member_Compares_As_Its_String_Form()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        Expression<Func<TestDocument, bool>> predicate = i => i.ExternalId == externalId;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'ExternalId' = @p0");
        parms.Length.ShouldBe(1);
        parms[0].Value.ShouldBe(externalId.ToString());
    }

    [Test]
    public void Translate_DateTime_Jsonb_Member_Compares_As_Its_Json_Text_Form()
    {
        // Arrange
        var occurred = new DateTime(2026, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        Expression<Func<TestDocument, bool>> predicate = i => i.Occurred == occurred;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'Occurred' = @p0");
        parms.Length.ShouldBe(1);
        parms[0].Value.ShouldBe(JsonSerializer.Serialize(occurred).Trim('"'));
    }

    [Test]
    public void Translate_Null_Equality_Uses_Is_Null()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name == null;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'Name' IS NULL");
        parms.ShouldBeEmpty();
    }

    [Test]
    public void Translate_String_Inequality_Uses_Is_Distinct_From()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name != "x";

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'Name' IS DISTINCT FROM @p0");
        parms.Length.ShouldBe(1);
        parms[0].ParameterName.ShouldBe("p0");
        parms[0].Value.ShouldBe("x");
    }

    [Test]
    public void Translate_Id_Inequality_Maps_To_Real_Column_With_Native_Guid()
    {
        // Arrange
        var id = Guid.NewGuid();
        Expression<Func<TestDocument, bool>> predicate = i => i.Id != id;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("id IS DISTINCT FROM @p0");
        parms.Length.ShouldBe(1);
        parms[0].ParameterName.ShouldBe("p0");
        parms[0].Value.ShouldBe(id);
    }

    [Test]
    public void Translate_Null_Inequality_Uses_Is_Not_Null()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name != null;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'Name' IS NOT NULL");
        parms.ShouldBeEmpty();
    }

    [Test]
    public void Translate_Captured_Member_On_Left_Uses_Document_Member_On_Right()
    {
        // Arrange
        var value = new ValueHolder { Name = "x" };
        Expression<Func<TestDocument, bool>> predicate = i => value.Name == i.Name;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data ->> 'Name' = @p0");
        parms.Length.ShouldBe(1);
        parms[0].Value.ShouldBe("x");
    }

    [Test]
    public void Translate_Nested_Guid_Jsonb_Member_Compares_As_Its_String_Form()
    {
        // Arrange
        var id = Guid.NewGuid();
        Expression<Func<TestDocument, bool>> predicate = i => i.Child.Id == id;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data #>> '{Child,Id}' = @p0");
        parms.Length.ShouldBe(1);
        parms[0].Value.ShouldBe(id.ToString());
    }

    [Test]
    public void Translate_Nested_Guid_Inequality_Uses_Is_Distinct_From()
    {
        // Arrange
        var id = Guid.NewGuid();
        Expression<Func<TestDocument, bool>> predicate = i => i.Child.Id != id;

        // Act
        var (sql, parms) = JsonbPredicateTranslator.Translate(predicate);

        // Assert
        sql.ShouldBe("data #>> '{Child,Id}' IS DISTINCT FROM @p0");
        parms.Length.ShouldBe(1);
        parms[0].Value.ShouldBe(id.ToString());
    }

    [Test]
    public void Translate_Throws_On_Document_Member_To_Document_Member_Comparison()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name == i.Code;

        // Act
        Action act = () => JsonbPredicateTranslator.Translate(predicate);

        // Assert
        Should.Throw<NotSupportedException>(act);
    }

    [Test]
    public void Translate_Throws_On_Document_Rooted_Value_Expression()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Name == i.Code.ToUpperInvariant();

        // Act
        Action act = () => JsonbPredicateTranslator.Translate(predicate);

        // Assert
        Should.Throw<NotSupportedException>(act);
    }

    [Test]
    public void Translate_Throws_On_Unsupported_Operator()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.Value > 1;

        // Act
        Action act = () => JsonbPredicateTranslator.Translate(predicate);

        // Assert
        Should.Throw<NotSupportedException>(act);
    }

    [Test]
    public void Translate_Throws_On_Unsupported_Member_Type()
    {
        // Arrange
        Expression<Func<TestDocument, bool>> predicate = i => i.RemoteCode == 5;

        // Act
        Action act = () => JsonbPredicateTranslator.Translate(predicate);

        // Assert
        Should.Throw<NotSupportedException>(act);
    }
}
