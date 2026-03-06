// DatabaseCleanupFixture has been replaced by IntegrationTestFixture (Testcontainers + Respawn).
// This stub keeps backward compatibility for legacy test classes that still reference it.
namespace CPR.IntegrationTests;

/// <summary>
/// Stub fixture — retained for backward compatibility only.
/// New tests should use IntegrationTestFixture via [Collection("Integration")].
/// </summary>
public class DatabaseCleanupFixture { }
