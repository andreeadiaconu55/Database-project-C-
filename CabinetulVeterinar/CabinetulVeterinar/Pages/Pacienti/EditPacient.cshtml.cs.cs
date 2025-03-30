using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace CabinetulVeterinar.Pages.Pacienti
{
    public class EditPacientModel : PageModel
    {
        private readonly ILogger<EditPacientModel> _logger;

        [BindProperty]
        public PacientDet Pacient { get; set; }

        public string Message { get; set; }

        public EditPacientModel(ILogger<EditPacientModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string id)
        {
            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT PacientID, Nume, Specie, Rasa, Sex, DataNasterii, Greutate FROM Pacienti WHERE PacientID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(id));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Pacient = new PacientDet
                                {
                                    Id = reader.GetInt32(0).ToString(),
                                    Name = reader.GetString(1),
                                    Specie = reader.GetString(2),
                                    Rasa = reader.GetString(3),
                                    Sex = reader.GetString(4),
                                    DataNasterii = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                    Greutate = reader.GetDouble(6)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea datelor pacientului: {ex.Message}");
                Message = "Eroare la încărcarea datelor pacientului.";
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Message = "Datele introduse nu sunt valide. Verifică câmpurile.";
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        UPDATE Pacienti
                        SET Nume = @Name, Specie = @Specie, Rasa = @Rasa, Sex = @Sex, DataNasterii = @DataNasterii, Greutate = @Greutate
                        WHERE PacientID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(Pacient.Id));
                        command.Parameters.AddWithValue("@Name", Pacient.Name);
                        command.Parameters.AddWithValue("@Specie", Pacient.Specie);
                        command.Parameters.AddWithValue("@Rasa", Pacient.Rasa);
                        command.Parameters.AddWithValue("@Sex", Pacient.Sex);
                        command.Parameters.AddWithValue("@DataNasterii", Pacient.DataNasterii ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Greutate", Pacient.Greutate);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Message = "Pacientul a fost actualizat cu succes!";
                        }
                        else
                        {
                            Message = "Pacientul nu a fost găsit.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la actualizarea pacientului: {ex.Message}");
                Message = "Eroare la actualizarea pacientului.";
            }

            return RedirectToPage("/Pacienti/Index");
        }
    }

    public class PacientDet
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Specia este obligatorie.")]
        public string Specie { get; set; }

        [Required(ErrorMessage = "Rasa este obligatorie.")]
        public string Rasa { get; set; }

        [Required(ErrorMessage = "Sexul este obligatoriu.")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Data nașterii este obligatorie.")]
        public DateTime? DataNasterii { get; set; }

        [Required(ErrorMessage = "Greutatea este obligatorie.")]
        [Range(0.1, 500.0, ErrorMessage = "Greutatea trebuie să fie între 0.1 și 500.")]
        public double Greutate { get; set; }
    }
}
