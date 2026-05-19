namespace Lzq.AI.Application.Contracts.Dtos;

public record ModelViewDto
{
    public long Id { get; set; }
    public string ConfigName { get; set; }
    public string KeyName { get; set; }
}
