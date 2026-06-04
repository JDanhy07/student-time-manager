using Microsoft.AspNetCore.Mvc;
using Timely.Models;
using Timely.Services.Interfaces;

namespace Timely.Controllers
{
    public class NotasController : Controller
    {
        private readonly INotaService _notaService;

        public NotasController(INotaService notaService)
        {
            _notaService = notaService;
        }

        public ActionResult MisNotas()
        {
            var model = _notaService.ObtenerTodas();
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
            try
            {
                if (!ModelState.IsValid)
                    return View(nota);

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
            return View(nota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Nota nota)
        {
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
            return View(nota); // Muestra la nota a confirmar antes de eliminar
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmado(int id)
        {
            try
            {
                var nota = _notaService.BuscarPorId(id);
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
