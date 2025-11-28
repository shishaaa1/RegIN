using MySql.Data.MySqlClient;
using RegIN.Classes;
using System;
using System.Collections.Generic;
using static RegN.Classes.User;

namespace RegN.Classes
{
    public class User
    {
        // <summary>
        // Код пользователя
        // </summary>
        public int Id { get; set; }
        // <summary>
        public string Login { get; set; }
        // <summary>
        public string Password { get; set; }
        // <summary>
        public string Name { get; set; }
        // <summary>
        public byte[] Image { get; set; }
        // <summary>
        // Дата и время обновления пользователя
        // </summary>
        public DateTime DateUpdate { get; set; }
        // <summary>
        // Дата и время создания пользователя
        // </summary>
        public DateTime DateCreate { get; set; }
        // <summary>
        // Событие успешной авторизации
        // </summary>
        public CorrectLogin HandleCorrectLogin;
        // <summary>
        // Событие не успешной авторизации
        // </summary>
        public IncorrectLogin HandleIncorrectLogin;

        // <summary>
        // Делегат для успешной авторизации
        // </summary>
        public delegate void CorrectLogin();
        // <summary>
        // Делегат для неуспешной авторизации
        // </summary>
        public delegate void IncorrectLogin();

        // <summary>
        // Получаем пользователя по логину и паролю
        // </summary>
        public void GetUserLogin(string Login)
        {
            // Устанавливаем первоначальные данные
            this.Id = -1;
            this.Login = String.Empty;
            this.Password = String.Empty;
            this.Name = String.Empty;
            this.Image = new byte[0];

            // Открываем соединение с базой данных
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
            // Если соединение с базой данных успешно открыто
            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                MySqlDataReader userQuery = WorkingDB.Query($"SELECT * FROM `users` WHERE `Login` = '{Login}'", mySqlConnection);
                // Проверяем что существуют данные для чтения
                if (userQuery.HasRows)
                {
                    // Читаем первые данные
                    userQuery.Read();
                    // Записываем код пользователя
                    this.Id = userQuery.GetInt32(0);
                    // Записываем логин пользователя
                    this.Login = userQuery.GetString(1);
                    // Записываем пароль пользователя
                    this.Password = userQuery.GetString(2);
                    // Записываем имя пользователя
                    this.Name = userQuery.GetString(3);

                    // Проверяем что изображение установлено
                    if (!userQuery.IsDBNull(4))
                    {
                        // Задаём размер массива
                        this.Image = new byte[64 * 1024];
                        // Записываем изображение пользователя
                        userQuery.GetBytes(4, 0, Image, 0, Image.Length);
                    }

                    // Записываем дату обновления
                    this.DateUpdate = userQuery.GetDateTime(5);
                    // Записываем дату создания
                    this.DateCreate = userQuery.GetDateTime(6);
                    // Вызываем событие успешной авторизации
                    HandleCorrectLogin.Invoke();
                }
                else
                {
                    // Если данные для чтения не существуют, вызываем событие не успешной авторизации
                    HandleIncorrectLogin.Invoke();
                }
            }
            else
            {
                // Если соединение открыть не удаётся, вызываем событие не успешной авторизации
                HandleIncorrectLogin.Invoke();
            }

            // Закрываем соединение с базой данных
            WorkingDB.CloseConnection(mySqlConnection);
        }

        // <summary>
        // Функция сохранения пользователя
        // </summary>
        public void SetUser()
        {
            // Открываем соединение с базой данных
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

            // Проверяем что соединение действительно открыто
            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                // Создаём запрос на добавление пользователя
                MySqlCommand mySqlCommand = new MySqlCommand("INSERT INTO `users` (`Login`, `Password`, `Name`, `Image`, `DateCreate`) VALUES (@Login, @Password, @Name, @Image, @DateCreate)", mySqlConnection);
                // Добавляем параметр логина
                mySqlCommand.Parameters.AddWithValue("@Login", this.Login);
                // Добавляем параметр пароля
                mySqlCommand.Parameters.AddWithValue("@Password", this.Password);
                // Добавляем параметр имени
                mySqlCommand.Parameters.AddWithValue("@Name", this.Name);
                // Добавляем параметр изображения
                mySqlCommand.Parameters.AddWithValue("@Image", this.Image);
                // Добавляем параметр даты обновления
                mySqlCommand.Parameters.AddWithValue("@DateUpdate", this.DateUpdate);
                // Добавляем параметр даты создания
                mySqlCommand.Parameters.AddWithValue("@DateCreate", this.DateCreate);
                // Выполняем запрос без возврата результата
                mySqlCommand.ExecuteNonQuery();
            }

            // Закрываем подключение к базе данных
            WorkingDB.CloseConnection(mySqlConnection);
        }

        // <summary>
        // Функция создания нового пароля
        // </summary>
        public void CreateNewPassword()
        {
            // Если наш логин не равен пустому значению
            if (Login != String.Empty)
            {
                // Вызываем функцию генерации пароля
                Password = GeneratePass();

                // Открываем подключение к базе данных
                MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

                // Проверяем что подключение действительно открыто
                if (WorkingDB.OpenConnection(mySqlConnection))
                {
                    // Выполняем запрос, обновляя пароль у выбранного пользователя
                    WorkingDB.Query($"UPDATE `users` SET `Password` = '{this.Password}' WHERE `Login` = '{this.Login}'", mySqlConnection);

                    // Закрываем подключение к базе данных
                    WorkingDB.CloseConnection(mySqlConnection);

                    // Отправляем сообщение на почту, о том что пароль изменён
                    SendMail.SendMessage($"Your account password has been changed.\nNew password: {this.Password}", this.Login);
                }
            }
        }

        // <summary>
        // Функция генерации пароля
        // </summary>
        public string GeneratePass()
        {
            // Создаём коллекцию, состоящую из символов
            List<char> NewPass = new List<char>();
            // Указываем символы, которые будут служить в выборе символа
            Random rnd = new Random();

            // Символы нумерации
            char[] ArrNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            // Символы знаков
            char[] ArrSymbols = { '!', '-', '_', '.', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+' };
            // Символы английской раскладки
            char[] ArrUppercase = { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M' };

            // Выбираем 1 случайную цифру
            for (int i = 0; i < 1; i++)
            {
                // Добавляем цифру в коллекцию
                NewPass.Add(ArrNumbers[rnd.Next(0, ArrNumbers.Length)]);
            }

            // Выбираем 1 случайный символ
            for (int i = 0; i < 1; i++)
            {
                // Добавляем символ в коллекцию
                NewPass.Add(ArrSymbols[rnd.Next(0, ArrSymbols.Length)]);
            }

            // Выбираем 2 случайные буквы английской раскладки верхнего регистра
            for (int i = 0; i < 2; i++)
            {
                // Добавляем букву верхнего регистра в коллекцию
                NewPass.Add(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]);
            }

            // Выбираем 6 случайных букв английской раскладки нижнего регистра
            for (int i = 0; i < 6; i++)
            {
                // Добавляем букву нижнего регистра в коллекцию
                NewPass.Add(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]);
            }

            // Тем самым, перемениваем коллекцию символов
            for (int i = 0; i < NewPass.Count; i++)
            {
                // Выбираем случайный символ
                int RandomSymbol = rnd.Next(0, NewPass.Count);

                // Меняем случайный символ на конкретный из списка
                char Symbol = NewPass[RandomSymbol];
                NewPass[RandomSymbol] = NewPass[i];
                NewPass[i] = Symbol;
            }

            // Объединяем переменную, которая будет содержать пароль
            string NewPassword = "";

            // Перебираем коллекцию
            foreach (char c in NewPassword)
            {
                // Добавляем в переменную с паролем символ из коллекции
                NewPassword += c;
            }

            // Возвращаем пароль
            return NewPassword;
        }
    }
}