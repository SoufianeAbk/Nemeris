namespace Nemeris.Core.Dtos;

public class ReviewCreateDto
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
}
