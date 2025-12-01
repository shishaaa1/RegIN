// Classes/WorkingDB.cs
using MySql.Data.MySqlClient;

public static class WorkingDB
{
    private static readonly string ConnectionString = "server=localhost;port=3306;database=regin;user=root;password=;";

    public static MySqlConnection OpenConnection()
    {
        var conn = new MySqlConnection(ConnectionString);
        try { conn.Open(); return conn; }
        catch { return null; }
    }

    public static bool IsOpen(MySqlConnection conn) => conn?.State == System.Data.ConnectionState.Open;

    public static MySqlDataReader Query(string sql, MySqlConnection conn, params (string, object)[] parameters)
    {
        var cmd = new MySqlCommand(sql, conn);
        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
        return cmd.ExecuteReader();
    }

    public static int ExecuteNonQuery(string sql, MySqlConnection conn, params (string, object)[] parameters)
    {
        var cmd = new MySqlCommand(sql, conn);
        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Item1, p.Item2);
        return cmd.ExecuteNonQuery();
    }

    public static void CloseConnection(MySqlConnection conn)
    {
        conn?.Close();
        MySqlConnection.ClearPool(conn);
    }
}