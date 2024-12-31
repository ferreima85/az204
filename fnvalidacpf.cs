using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace httpValidaCpf
{
    public static class fnvalidacpf
    {
        [FunctionName("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Inicializando a validação de CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Verificação de entrada nula
            if (data?.cpf == null)
            {
                return new BadRequestObjectResult("CPF não fornecido no corpo da solicitação.");
            }

            string cpf = data.cpf.ToString();

            // Chama o método para validar o CPF
            if (!ValidaCPF(cpf))
            {
                return new BadRequestObjectResult("CPF inválido.");
            }

            string responseMessage = "CPF válido.";
            return new OkObjectResult(responseMessage);
        }

        /// <summary>
        /// Método para validar CPF.
        /// </summary>
        /// <param name="cpf">String contendo o CPF.</param>
        /// <returns>Retorna true se o CPF for válido, ou false caso contrário.</returns>
        private static bool ValidaCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            // Remove caracteres não numéricos
            cpf = Regex.Replace(cpf, @"[^\d]", "");

            // Verifica se o CPF tem 11 dígitos ou é uma sequência repetida
            if (cpf.Length != 11 || new string(cpf[0], 11) == cpf)
                return false;

            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            // Calcula o primeiro dígito verificador
            for (int i = 0; i < 9; i++)
                soma += (tempCpf[i] - '0') * multiplicadores1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = 0;

            // Calcula o segundo dígito verificador
            for (int i = 0; i < 10; i++)
                soma += (tempCpf[i] - '0') * multiplicadores2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            // Verifica se os dígitos calculados conferem com os fornecidos
            return cpf.EndsWith($"{digito1}{digito2}");
        }
    }
}
