using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using Timely.Services;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    [Authorize]
    public class NotasController : Controller
    {
        private readonly INotaService _notaService;
        private readonly IUsuarioService _usuarioService;

        public NotasController(INotaService notaService, IUsuarioService usuarioService)
        {
            _notaService = notaService;
            _usuarioService = usuarioService;
        }

        private int ObtenerUsuarioActualId()
        {
            var usuario = _usuarioService.ObtenerPerfil(User);
            return usuario?.Id ?? 0;
        }

        public ActionResult MisNotas()
        {
            int usuarioId = ObtenerUsuarioActualId();
            if (usuarioId == 0) return RedirectToAction("Login", "Usuarios");
            var model = _notaService.ObtenerPorUsuario(usuarioId);
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Nota nota)
        {
            nota.UsuarioId = ObtenerUsuarioActualId();

            try
            {
                // 2. Le dices al validador que ignore que estos campos no vinieron del HTML
                ModelState.Remove("Usuario");
                ModelState.Remove("UsuarioId");

                if (ModelState.IsValid)
                _notaService.AgregarNota(nota);
                return RedirectToAction(nameof(MisNotas));
            }
            catch 
            {
                ModelState.AddModelError("", "La nota que deseas añadir ya existe. Intenta con otra.");
                return View(nota);
            }
        }

        public ActionResult Edit(int id)
        {
            var nota = _notaService.BuscarPorId(id);
            if (nota.UsuarioId != ObtenerUsuarioActualId())
            {
                return Unauthorized();
            }
            return View(nota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Nota nota)
        {
            nota.UsuarioId = ObtenerUsuarioActualId();
            var notaOriginal = _notaService.BuscarPorId(nota.Id);
            if (notaOriginal.UsuarioId != nota.UsuarioId)
            {
                return Unauthorized();
            }

            try
            {
                if (!ModelState.IsValid)
                    return View(nota);

                _notaService.ActualizarNota(nota);
                return RedirectToAction(nameof(MisNotas));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(nota);
            }
        }

        public ActionResult Delete(int id)
        {
            var nota = _notaService.BuscarPorId(id);
            if (nota.UsuarioId != ObtenerUsuarioActualId())
            {
                return Unauthorized();
            }
            return View(nota); // Muestra la nota a confirmar antes de eliminar
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmado(int id)
        {
            try
            {
                var nota = _notaService.BuscarPorId(id);
                if (nota.UsuarioId != ObtenerUsuarioActualId())
                {
                    return Unauthorized();
                }
                _notaService.EliminarNota(nota);
                return RedirectToAction(nameof(MisNotas));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction(nameof(MisNotas));
            }
        }
    }
}
