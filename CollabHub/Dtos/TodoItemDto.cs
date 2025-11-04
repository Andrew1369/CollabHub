namespace CollabHub.Dtos
{
    public record TodoItemDto(
        int Id,
        string Text,
        bool Done,
        int Order,
        DateTime CreatedAt
    );
}
