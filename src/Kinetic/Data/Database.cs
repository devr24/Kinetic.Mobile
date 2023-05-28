using Kinetic.Presentation.Data.Entities;
using SQLite;

public class Database
{
    public const SQLite.SQLiteOpenFlags Flags =
        SQLite.SQLiteOpenFlags.ReadWrite |
        SQLite.SQLiteOpenFlags.Create |
        SQLite.SQLiteOpenFlags.SharedCache;

    public static Database Instance { get; set; }

    private readonly SQLiteAsyncConnection _connection;
    //private readonly SQLiteConnection _connectionSync;

    public Database()
    {
        string dbName = "kinectic.db3";
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //folderPath = "/sdcard/Android/data/com.companyname.mauidemo/files";
        string dbPath = Path.Combine(folderPath, dbName);
        //Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

        _connection = new SQLiteAsyncConnection(dbPath, Flags);
        //_connectionSync = new SQLiteConnection(dbPath);

        _ = Task.Run(InitializeKinetictTables);
    }

    public async Task InitializeKinetictTables()
    {
        await Database.Instance.InitializeTable<SessionDataEntity>();
        await Database.Instance.InitializeTable<SessionEntity>();
    }

    public async Task InitializeTable<T>() where T : new()
    {
        //_connectionSync.CreateTable<T>();
        var result = await _connection.CreateTableAsync<T>();
    }
    
    public async Task<List<T>> GetAsync<T>() where T : new()
    {
        //SQLiteCommand command = new SQLiteCommand(_connectionSync);
        //var cmdText = $"SELECT * FROM {nameof(SessionEntity)}";
        //command.CommandText = cmdText;
        //var res1 = command.ExecuteQuery<T>();
        var items = _connection.Table<T>();
        return await items.ToListAsync();
    }

    public async Task<int> CountAsync<T>() where T:new()
    {
        return await _connection.Table<T>().CountAsync();
    }

    public Task<int> SaveAsync<T>(T data)
    {
        return _connection.InsertOrReplaceAsync(data);
    }

    public Task<int> SaveBatchAsync<T>(List<T> data) where T : new()
    {
        return _connection.InsertAllAsync(data);
    }

    public async Task PurgeAsync<T>() where T: new()
    {
        var data = await _connection.Table<T>().ToListAsync();
        foreach (var record in data)
        {
            _connection.DeleteAsync(record).GetAwaiter().GetResult();
        }
    }
}
