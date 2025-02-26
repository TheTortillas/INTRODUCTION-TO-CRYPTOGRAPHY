using CryptoBackend.DTOs;
using CryptoBackend.DTOs.Auth;
using CryptoBackend.Services;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;

namespace CryptoBackend
{
    public class DBManager
    {
        private readonly IConfiguration _config;
        // Constructor
        public DBManager(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> sign_up(UserDTO request)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_insert_user", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Configurar parámetros
                    cmd.Parameters.AddWithValue("p_first_name", request.Firstname);
                    cmd.Parameters.AddWithValue("p_last_name", request.Lastname);
                    cmd.Parameters.AddWithValue("p_second_last_name", request.SecondLastname);
                    cmd.Parameters.AddWithValue("p_email", request.Email);
                    cmd.Parameters.AddWithValue("p_password", request.Password);
                    cmd.Parameters.AddWithValue("p_salt", request.Salt);

                    // Parámetros de salida
                    MySqlParameter statusParam = new MySqlParameter("p_status_code", MySqlDbType.Int32)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    MySqlParameter messageParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };

                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    int statusCode = Convert.ToInt32(statusParam.Value);
                    return statusCode == 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        // Metodo para iniciar sesión
        public async Task<UserDTO?> sign_in(string email, string password)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_login_user", con);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("p_email", email);
                    cmd.Parameters.AddWithValue("p_password", password);

                    // Parámetros de salida
                    MySqlParameter userIdParam = new MySqlParameter("p_user_id", MySqlDbType.Int32)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    MySqlParameter statusParam = new MySqlParameter("p_status_code", MySqlDbType.Int32)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    MySqlParameter messageParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };

                    cmd.Parameters.Add(userIdParam);
                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserDTO
                            {
                                ID = reader.GetInt32("id"),
                                Firstname = reader.GetString("first_name"),
                                Lastname = reader.GetString("last_name"),
                                SecondLastname = reader.GetString("second_last_name"),
                                Email = reader.GetString("email"),
                                Password = reader.GetString("password"),
                                Salt = reader.GetString("salt")
                            };
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDTO?> FindByEmailAsync(string email)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_get_user_by_email", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("p_email", email);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserDTO
                            {
                                ID = reader.GetInt32("id"),
                                Firstname = reader.GetString("first_name"),
                                Lastname = reader.GetString("last_name"),
                                SecondLastname = reader.GetString("second_last_name"),
                                Email = reader.GetString("email"),
                                Password = reader.GetString("password"),
                                Salt = reader.GetString("salt")
                            };
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}

