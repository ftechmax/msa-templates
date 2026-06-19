using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using Npgsql;

namespace ApplicationName.Worker.Infrastructure;

// Converter for Expression<Func<T,bool>> to query the jsonb 'data' column
// Current support: '==', '!=', and '&&' on string/Guid/DateTime
internal static class JsonbPredicateTranslator
{
    // base document columns are also real columns for fast indexing
    private static readonly HashSet<string> Columns = new(StringComparer.Ordinal) { "Id", "Created", "Updated" };

    public static (string Sql, NpgsqlParameter[] Parameters) Translate<T>(Expression<Func<T, bool>> predicate)
    {
        var sql = new StringBuilder();
        var parms = new List<NpgsqlParameter>();
        Visit(predicate.Body, predicate.Parameters[0], sql, parms);
        return (sql.ToString(), parms.ToArray());
    }

    private static void Visit(Expression node, ParameterExpression parameter, StringBuilder sql,
        List<NpgsqlParameter> parms)
    {
        switch (node)
        {
            case BinaryExpression { NodeType: ExpressionType.AndAlso } b:
                sql.Append('(');
                Visit(b.Left, parameter, sql, parms);
                sql.Append(" AND ");
                Visit(b.Right, parameter, sql, parms);
                sql.Append(')');
                break;
            case BinaryExpression { NodeType: ExpressionType.Equal } b:
                EmitComparison(b, parameter, sql, parms, equal: true);
                break;
            case BinaryExpression { NodeType: ExpressionType.NotEqual } b:
                EmitComparison(b, parameter, sql, parms, equal: false);
                break;
            default:
                throw new NotSupportedException(
                    $"JSONB predicate v0 supports only '==', '!=', and '&&'; got {node.NodeType}: {node}");
        }
    }

    private static void EmitComparison(BinaryExpression node, ParameterExpression parameter, StringBuilder sql,
        List<NpgsqlParameter> parms, bool equal)
    {
        var leftMember = GetDocumentMemberPath(node.Left, parameter);
        var rightMember = GetDocumentMemberPath(node.Right, parameter);
        var member = leftMember ?? rightMember
            ?? throw new NotSupportedException(
                $"One side of '{(equal ? "==" : "!=")}' must be a document property: {node}");
        var valueExpr = ReferenceEquals(member, leftMember) ? node.Right : node.Left;
        if (IsRootedInParameter(valueExpr, parameter))
        {
            throw new NotSupportedException(
                $"Document properties can only be compared to constant or captured values: {node}");
        }

        var type = Nullable.GetUnderlyingType(member.Expression.Type) ?? member.Expression.Type;
        if (type != typeof(string) && type != typeof(Guid) && type != typeof(DateTime))
        {
            throw new NotSupportedException(
                $"JSONB predicate v0 supports only string/Guid/DateTime members; '{string.Join('.', member.Path)}' is {type.Name}");
        }

        var value = Evaluate(valueExpr);
        var field = GetSqlField(member.Path);

        if (value is null)
        {
            sql.Append($"{field} IS {(equal ? "" : "NOT ")}NULL");
            return;
        }

        if (IsColumn(member.Path))
        {
            // real column
            var name = $"p{parms.Count}";
            sql.Append(equal ? $"{field} = @{name}" : $"{field} IS DISTINCT FROM @{name}");
            parms.Add(new NpgsqlParameter(name, value));
        }
        else
        {
            // jsonb field
            var name = $"p{parms.Count}";
            sql.Append(equal ? $"{field} = @{name}" : $"{field} IS DISTINCT FROM @{name}");
            parms.Add(new NpgsqlParameter(name, ToJsonbTextValue(value)));
        }
    }

    // Member/property names are CLR members of T so safe to interpolate.
    private static object? Evaluate(Expression e) =>
        e is ConstantExpression c ? c.Value : Expression.Lambda(e).Compile().DynamicInvoke();

    private static DocumentMemberPath? GetDocumentMemberPath(Expression e, ParameterExpression parameter)
    {
        var candidate = StripConvert(e);
        if (candidate is not MemberExpression member)
        {
            return null;
        }

        var path = TryGetMemberPath(member, parameter);
        if (path is not null)
        {
            return new DocumentMemberPath(member, path);
        }

        if (IsRootedInParameter(member, parameter))
        {
            throw new NotSupportedException($"JSONB predicate v0 supports only document member paths; got {member}");
        }

        return null;
    }

    private static string[]? TryGetMemberPath(MemberExpression member, ParameterExpression parameter)
    {
        var names = new Stack<string>();
        Expression? current = member;
        while (StripConvert(current) is MemberExpression currentMember)
        {
            names.Push(currentMember.Member.Name);
            current = currentMember.Expression;
        }

        return StripConvert(current) is ParameterExpression currentParameter &&
               ReferenceEquals(currentParameter, parameter)
            ? [.. names]
            : null;
    }

    private static bool IsColumn(string[] path) => path.Length == 1 && Columns.Contains(path[0]);

    private static string GetSqlField(string[] path)
    {
        if (IsColumn(path))
        {
            return path[0].ToLowerInvariant();
        }

        return path.Length == 1 ? $"data ->> '{path[0]}'" : $"data #>> '{{{string.Join(',', path)}}}'";
    }

    private static bool IsRootedInParameter(Expression? e, ParameterExpression parameter)
    {
        var finder = new ParameterReferenceFinder(parameter);
        finder.Visit(e);
        return finder.Found;
    }

    private static Expression? StripConvert(Expression? e)
    {
        while (e is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } u)
        {
            e = u.Operand;
        }

        return e;
    }

    private static object ToJsonbTextValue(object value) =>
        value switch
        {
            Guid g => g.ToString(),
            DateTime dt => JsonSerializer.Serialize(dt).Trim('"'),
            _ => value
        };

    private sealed record DocumentMemberPath(MemberExpression Expression, string[] Path);

    private sealed class ParameterReferenceFinder(ParameterExpression parameter) : ExpressionVisitor
    {
        public bool Found { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Found |= ReferenceEquals(node, parameter);
            return node;
        }
    }
}
