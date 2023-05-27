using SQLite;

namespace MauiDemo.Presentation.Data.Entities;

public class Person
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public int Age { get; set; }
}
