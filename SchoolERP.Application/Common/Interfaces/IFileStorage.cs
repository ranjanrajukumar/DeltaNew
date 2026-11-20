namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// The upload folder. Files now live in their database rows; the folder holds only copies saved before that move
/// (the Node app's files.service diskPath / removeStoredFiles).
/// </summary>
public interface IFileStorage
{
    Task RemoveAsync(IEnumerable<string> storedNames, CancellationToken ct = default);

    /// <summary>The bytes of a file kept only in the folder (rows saved before files moved into the database); null when missing.</summary>
    Task<byte[]?> ReadAsync(string storedName, CancellationToken ct = default);
}
