using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CabinetulVeterinar.Pages.Doctori
{
    public class AddDoctorModel : PageModel
    {
        private readonly ILogger<AddDoctorModel> _logger;

        [BindProperty]
        public CabinetulVeterinar.Pages.Doctori.DoctorDetails Doctor { get; set; }

        public string Message { get; set; }

        public AddDoctorModel(ILogger<AddDoctorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Inițializare pentru orice logică de pregătire a paginii (dacă este necesar).
        }

        public IActionResult OnPost()
        {
            ModelState.Remove("Doctor.Id");
            Console.WriteLine("OnPost a fost apelată pentru adăugarea doctorului.");
            _logger.LogInformation("OnPost a fost apelată pentru adăugarea doctorului.");
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
            INSERT INTO Doctori (Nume, Prenume, Telefon, Email, Adresa, Specializare, Password)
            VALUES (@Name, @Prenume, @Telefon, @Email, @Adresa, @Specializare, @Password)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Doctor.Name);
                        command.Parameters.AddWithValue("@Prenume", Doctor.Prenume);
                        command.Parameters.AddWithValue("@Telefon", Doctor.Telefon);
                        command.Parameters.AddWithValue("@Email", Doctor.Email);
                        command.Parameters.AddWithValue("@Adresa", Doctor.Adresa);
                        command.Parameters.AddWithValue("@Specializare", Doctor.Specializare);
                        command.Parameters.AddWithValue("@Password", Doctor.Password);


                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Message = "Doctorul a fost adăugat cu succes!";
                            Console.WriteLine("Doctor adăugat cu succes!");
                        }
                        else
                        {
                            Message = "A apărut o problemă la adăugarea doctorului.";
                            Console.WriteLine("Doctorul nu a fost adăugat.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la adăugarea doctorului: {ex.Message}");
                Message = "Eroare la adăugarea doctorului.";
            }

            return RedirectToPage("/Pacienti/Index");
        }

    }

    public class DoctorDetails
    {
        [BindNever] // Exclude acest câmp din binding și validare
        public string Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Prenumele este obligatoriu.")]
        public string Prenume { get; set; }

        [Required(ErrorMessage = "Telefonul este obligatoriu.")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Formatul email-ului nu este valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Adresa este obligatorie.")]
        public string Adresa { get; set; }

        [Required(ErrorMessage = "Specializarea este obligatorie.")]
        public string Specializare { get; set; }

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
        public string Password { get; set; }
    }

}
