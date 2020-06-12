using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using conexionBBDD.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace conexionBBDD.Controllers
{
    public class HomeController : Controller
    {
        public static postgre_contexto contexto = new postgre_contexto();

        private readonly ILogger<HomeController> _logger;
        private string path = Properties.Resources.ResourceManager.GetString("ApiRoute");
        private string uploadAcceptedFormats = Properties.Resources.ResourceManager.GetString("ExtensionesPermitidas");

        private string user = Properties.Resources.ResourceManager.GetString("UsuarioWeb");
        private string password = Properties.Resources.ResourceManager.GetString("PasswordWeb");

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (TempData["DataDB"] != null)
            {
                ViewBag.DataDB = TempData["DataDB"].ToString();
            }

            try
            {
                contexto.Database.OpenConnection();
                contexto.Database.CanConnect();

                ViewBag.Conectado = "Tenemos conexión con la base de datos.";
            }
            catch (Exception e)
            {
                ViewBag.Conectado = "QUE NO TE CONECTAS, PESADO: " + e.Message;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Add_Users()
        {
            string exito = "";
            users usr = new users();
            try
            {
                usr.name = "Test";

                contexto.users.Add(usr);
                contexto.SaveChanges();

                exito = "Usuario creado correctamente.";
            }
            catch (Exception e)
            {
                exito = "Error al crear el usuario.";
            }

            TempData["DataDB"] = exito;
            return RedirectToAction("Index");
        }

        public IActionResult Reload()
        {
            string exito = "";

            try
            {
                TempData["DataDB"] = String.Concat(contexto.users.Where(us => us.name == "Test").Select(usr => usr.name).FirstOrDefault(), ". Usuarios en BD: ", contexto.users.Where(us => us.name == "Test").ToList().Count().ToString());
                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                TempData["DataDB"] = e.Message.ToString();
                return RedirectToAction("Index");
            }
        }

        [HttpPost("UploadFiles")]
        public IActionResult Upload()
        {
            try
            {
                IFormFileCollection files = Request.Form.Files;

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        string[] extensions = uploadAcceptedFormats.Split(';');
                        string fileExtension = Path.GetExtension(file.FileName);

                        if (!extensions.Contains(fileExtension)) {
                            TempData["DataDB"] = "Extensión no permitida, seleccione otro archivo.";
                            return RedirectToAction("Index");
                        }

                        string userEncrypt = "";
                        string passEncrypt = "";

                        byte[] encrypt = System.Text.Encoding.Unicode.GetBytes(user);
                        userEncrypt = Convert.ToBase64String(encrypt);

                        encrypt = System.Text.Encoding.Unicode.GetBytes(password);
                        passEncrypt = Convert.ToBase64String(encrypt);

                        using (HttpClient client = new HttpClient())
                        {
                            byte[] data;
                            using (BinaryReader br = new BinaryReader(file.OpenReadStream()))
                                data = br.ReadBytes((int)file.OpenReadStream().Length);

                            ByteArrayContent bytes = new ByteArrayContent(data);
                            MultipartFormDataContent content = new MultipartFormDataContent();
                            content.Add(bytes, "file", file.FileName);

                            HttpResponseMessage result = client.PostAsync(String.Concat(path, "?user=", userEncrypt, "&password=", passEncrypt), content).Result;

                            if (!result.IsSuccessStatusCode)
                                TempData["DataDB"] = "Error al copiar el archivo en la API.";
                            else
                                TempData["DataDB"] = "Éxito al copiar el archivo en la API.";
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                TempData["DataDB"] = "Error al copiar el archivo en la API.";
                return RedirectToAction("Index");
            }
        }
    }
}