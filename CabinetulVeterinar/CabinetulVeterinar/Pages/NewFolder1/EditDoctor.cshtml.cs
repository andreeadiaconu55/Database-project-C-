using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace CabinetulVeterinar.Pages.Doctori
{
    public class EditDoctorModel : PageModel
    {
        private readonly ILogger<EditDoctorModel> _logger;

        [BindProperty]
        public DoctorInfo Doctor { get; set; }

        public string Message { get; set; }

        public EditDoctorModel(ILogger<EditDoctorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string id)
        {
            Console.WriteLine($"Parametrul id primit: {id}");

            if (string.IsNullOrEmpty(id))
            {
                Message = "ID-ul doctorului nu este valid.";
                return;
            }
            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT DoctorID, Nume, Specializare, Telefon, Email FROM Doctori WHERE DoctorID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(id));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Doctor = new DoctorInfo
                                {
                                    Id = reader.GetInt32(0).ToString(),
                                    Name = reader.GetString(1),
                                    Specializare = reader.GetString(2),
                                    Telefon = reader.GetString(3),
                                    Email = reader.GetString(4)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea datelor doctorului: {ex.Message}");
                Message = "Eroare la încărcarea datelor doctorului.";
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = new List<string>();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errorMessages.Add($"Câmp: {state.Key}, Eroare: {error.ErrorMessage}");
                    }
                }

                Message = "Datele introduse nu sunt valide. Verifică câmpurile.\n" + string.Join("\n", errorMessages);
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                UPDATE Doctori
                SET Nume = @Name, Specializare = @Specializare, Telefon = @Telefon, Email = @Email
                WHERE DoctorID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(Doctor.Id));
                        command.Parameters.AddWithValue("@Name", Doctor.Name);
                        command.Parameters.AddWithValue("@Specializare", Doctor.Specializare);
                        command.Parameters.AddWithValue("@Telefon", Doctor.Telefon);
                        command.Parameters.AddWithValue("@Email", Doctor.Email);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Message = "Doctorul a fost actualizat cu succes!";
                        }
                        else
                        {
                            Message = "Doctorul nu a fost găsit.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la actualizarea doctorului: {ex.Message}");
                Message = "Eroare la actualizarea doctorului.";
            }

            return RedirectToPage("/Pacienti/Index");
        }



    }

    public class DoctorInfo
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Specializarea este obligatorie.")]
        public string Specializare { get; set; }

        [Required(ErrorMessage = "Telefonul este obligatoriu.")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Formatul email-ului nu este valid.")]
        public string Email { get; set; }

        
    }

}
