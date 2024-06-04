using ATMProject.Application.Users;

namespace ATMProject.Data.MockDatabase;
public record MockDatabaseAuditModel
(
    string AuditId,
    string AdminId,
    string BasicId,
    AdminInteraction InteractionType,
    DateTime DateTime
);
