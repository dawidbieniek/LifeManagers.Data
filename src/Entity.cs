using System.ComponentModel.DataAnnotations;

namespace LifeManagers.Data;

public class Entity
{
    [Key]
    public int Id { get; set; }
}
