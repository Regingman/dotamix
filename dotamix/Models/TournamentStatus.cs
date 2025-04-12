namespace dotamix.Models
{
    public enum TournamentStatus
    {
        Registration = 1, // Идёт регистрация команд
        TeamFormation = 2, // Формирование команд
        BracketGeneration = 3, // Генерация сетки
        InProgress = 4, // Турнир идёт
        Completed = 5 // Турнир завершён
    }
} 