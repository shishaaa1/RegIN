// Classes/User.cs
using MySql.Data.MySqlClient;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RegIN.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public byte[] Image { get; set; }  // Не nullable — будем проверять на null вручную
        public DateTime DateUpdate { get; set; }
        public DateTime DateCreate { get; set; }
        public string PinHash { get; set; }  // Может быть null

        public delegate void LoginHandler();
        public event LoginHandler OnCorrectLogin;
        public event LoginHandler OnIncorrectLogin;

        public void GetUserByLogin(string login)
        {
            Id = -1;
            Login = Password = Name = string.Empty;
            Image = null;
            PinHash = null;

            var conn = WorkingDB.OpenConnection();
            if (conn == null || conn.State != System.Data.ConnectionState.Open)
            {
                OnIncorrectLogin?.Invoke();
                return;
            }

            using (var cmd = new MySqlCommand("SELECT * FROM users WHERE Login = @login LIMIT 1", conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Id = reader.GetInt32("Id");
                        Login = reader.GetString("Login");
                        Password = reader.GetString("Password");
                        Name = reader.GetString("Name");

                        if (!reader.IsDBNull(reader.GetOrdinal("Image")))
                        {
                            var imageBytes = reader["Image"] as byte[];
                            if (imageBytes != null) Image = imageBytes;
                        }

                        DateUpdate = reader.GetDateTime("DateUpdate");
                        DateCreate = reader.GetDateTime("DateCreate");

                        if (!reader.IsDBNull(reader.GetOrdinal("PinHash")))
                            PinHash = reader.GetString("PinHash");

                        OnCorrectLogin?.Invoke();
                    }
                    else
                    {
                        OnIncorrectLogin?.Invoke();
                    }
                }
            }

            WorkingDB.CloseConnection(conn);
        }

        public void SetUser()
        {
            var conn = WorkingDB.OpenConnection();
            if (conn == null) return;

            using (var cmd = new MySqlCommand(@"INSERT INTO users 
                (Login, Password, Name, Image, DateUpdate, DateCreate) 
                VALUES (@Login, @Password, @Name, @Image, @DateUpdate, @DateCreate)", conn))
            {
                cmd.Parameters.AddWithValue("@Login", Login);
                cmd.Parameters.AddWithValue("@Password", Password);
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@Image", Image ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DateUpdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@DateCreate", DateTime.Now);
                cmd.ExecuteNonQuery();
            }

            WorkingDB.CloseConnection(conn);
        }

        public void UpdatePin(string pin)
        {
            PinHash = pin;  // Убрано хэширование
            var conn = WorkingDB.OpenConnection();
            if (conn == null) return;
            var cmd = new MySqlCommand("UPDATE users SET PinHash = @pin WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@pin", PinHash);
            cmd.Parameters.AddWithValue("@id", Id);
            cmd.ExecuteNonQuery();
            WorkingDB.CloseConnection(conn);
        }

        public bool VerifyPin(string pin)
        {
            if (string.IsNullOrEmpty(PinHash)) return false;
            return PinHash == pin; 
        }

        
    }
}