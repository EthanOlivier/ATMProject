using ATMProject.Application.Users;

namespace ATMProject.Data.FileProcesses.FileModels;
public record FileAuditModel
(
    string AuditId,
    string AdminId,
    string BasicId,
    AdminInteraction InteractionType,
    DateTime DateTime
);
