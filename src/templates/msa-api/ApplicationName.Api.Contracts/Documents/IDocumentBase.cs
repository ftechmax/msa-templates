using System;

namespace ApplicationName.Api.Contracts.Documents;

public interface IDocumentBase
{
    Guid Id { get; }

    DateTime Created { get; }

    DateTime Updated { get; }
}