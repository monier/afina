using System;
using System.Threading.Tasks;

namespace Afina.Shared.Interfaces
{
    public interface IEncryptionService
    {
        Task<string> EncryptAsync(string plainText, Guid tenantId);
        Task<string> DecryptAsync(string cipherText, Guid tenantId, Guid encryptionVersionId);
        Task<Guid> RotateKeyAsync(Guid tenantId);
    }
}
