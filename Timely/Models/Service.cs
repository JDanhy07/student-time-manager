using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using NuGet.Protocol.Plugins;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Security.Claims;

namespace Timely.Models
{
	// Clase que hereda de DbContext y permite la interacción con la base de datos
	public class Service : DbContext
	{
		// DbSets para las entidades
		public DbSet<Usuarios> Usuarios { get; set; }
		public DbSet<Proyectos> Proyectos { get; set; }
		public DbSet<Nota> Notas { get; set; }
		public DbSet<Calendario> Calendario { get; set; }

		// Constructor que inicializa el contexto con el nombre de la base de datos
		public Service() : base("Timely") { }

		#region usuarios
		// Método para agregar un nuevo usuario
		public void agregarUsuario(Usuarios usuari)
		{
			// Agrega el nuevo usuario a la DbSet y guarda los cambios
			Usuarios.Add(usuari);
			SaveChanges();
		}

		// Método para mostrar todos los usuarios
		public List<Usuarios> mostrarUsuario()
		{
			// Retorna la lista completa de usuarios
			return Usuarios.ToList();
		}

		// Método para buscar un usuario por su nombre de usuario
		public Usuarios buscarUsuarios(string usuari)
		{
			var usuariobuscado = Usuarios.FirstOrDefault(p => p.Usuario == usuari);
			if (usuariobuscado != null)
				return usuariobuscado;
			else
				// Si no se encuentra el usuario, lanza una excepción
				throw new Exception("El usuario que intenta buscar, ya esta creado. Intenta con otro");
		}

		// Método para buscar un usuario por su ID
		public Usuarios buscarId(int id)
		{
			var idbuscado = Usuarios.FirstOrDefault(p => p.Id == id);
			if (idbuscado != null)
				return idbuscado;
			else
				// Si no se encuentra el usuario, lanza una excepción
				throw new Exception("El id que intenta buscar, no existe");
		}

		// Método para eliminar un usuario por su ID
		public void eliminarId(Usuarios idEliminado)
		{
			// Elimina el usuario de la DbSet y guarda los cambios
			Usuarios.Remove(idEliminado);
			SaveChanges();
		}

		// Método para actualizar un usuario
		public void actualizarId(Usuarios idActualizado)
		{
			var idbuscado = Usuarios.FirstOrDefault(u => u.Id == idActualizado.Id);
			if (idbuscado != null)
			{
				// Actualiza las propiedades del usuario
				idbuscado.Usuario = idActualizado.Usuario;
				idbuscado.Contrasena = idActualizado.Contrasena;
				idbuscado.Perfil = idActualizado.Perfil;
				idbuscado.Correo = idActualizado.Correo;
				idbuscado.Confirmacion = idActualizado.Confirmacion;
				idbuscado.Fecha = idActualizado.Fecha;
				SaveChanges();
			}
			else
				throw new Exception("El usuario que intenta actualizar, no existe");
		}
		#endregion usuarios


		#region Login
		// Método para validar el inicio de sesión de un usuario
		public Usuarios loginUsuarios(string usuario, int contrasena)
		{
			Console.WriteLine($"🔍 Buscando usuario: {usuario}, Contraseña: {contrasena}");

			var usuarioEncontrado = Usuarios.FirstOrDefault(u => u.Usuario == usuario && u.Contrasena == contrasena);

			if (usuarioEncontrado == null)
			{
				Console.WriteLine("❌ No se encontró el usuario en la base de datos.");
			}
			else
			{
				Console.WriteLine($"✅ Usuario encontrado: {usuarioEncontrado.Usuario}");
			}
			return usuarioEncontrado;
		}

		// Método para obtener el perfil del usuario logueado
		public Usuarios MiPerfil(ClaimsPrincipal user)
		{
			// Obtiene el nombre del usuario logueado
			var nombreUsuario = user.Identity.Name;
			var rol = user.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value;

			// Busca el usuario por su nombre
			var usuario = Usuarios.FirstOrDefault(p => p.Usuario == nombreUsuario);
			return usuario;
		}
		#endregion Login


		#region Proyectos
		// Método para agregar un nuevo proyecto
		public void agregarProyecto(Proyectos Proyecto)
		{
			Proyectos.Add(Proyecto);
			SaveChanges();
		}

		// Método para mostrar todos los proyectos
		public List<Proyectos> mostrarProyecto()
		{
			return Proyectos.ToList();
		}

		// Método para obtener un proyecto por su ID
		public Proyectos ObtenerProyectoPorId(int id)
		{
			return Proyectos.FirstOrDefault(p => p.Id == id)
				?? throw new Exception("El proyecto que intenta buscar no existe");
		}

		// Método para buscar un proyecto por su ID
		public Proyectos buscarProyecto(int id)
		{
			var buscadoProyecto = Proyectos.FirstOrDefault(p => p.Id == id);
			if (buscadoProyecto != null)
				return buscadoProyecto;
			else throw new Exception("El id que intenta buscar, no existe");
		}

		// Método para eliminar un proyecto
		public void eliminarProyecto(Proyectos idEliminado)
		{
			Proyectos.Remove(idEliminado);
			SaveChanges();
		}

		// Método para actualizar un proyecto
		public void actualizarProyecto(Proyectos idActualizado)
		{
			var proyectobuscado = Proyectos.FirstOrDefault(u => u.Id == idActualizado.Id);
			if (proyectobuscado != null)
			{
				proyectobuscado.Nombre = idActualizado.Nombre;
				proyectobuscado.Fecha_de_inicio = idActualizado.Fecha_de_inicio;
				proyectobuscado.Vence = idActualizado.Vence;
				SaveChanges();
			}
			else throw new Exception("El proyecto que intenta actualizar, no existe");
		}
		#endregion Proyectos


		#region Notas
		// Método para agregar una nueva nota
		public void agregarNota(Nota notas)
		{
			Notas.Add(notas);
			SaveChanges();
		}

		// Método para mostrar todas las notas
		public List<Nota> mostrarNota()
		{
			return Notas.ToList();
		}

		// Método para buscar una nota por su ID
		public Nota buscarIdnota(int id)
		{
			return Notas.FirstOrDefault(p => p.Id == id); // Devuelve null si no lo encuentra
		}

		// Método para eliminar una nota
		public void eliminarIdnotas(Nota idEliminados)
		{
			Notas.Remove(idEliminados);
			SaveChanges();
		}

		// Método para actualizar una nota
		public void actualizarNotas(Nota idActualizadonota)
		{
			var idbuscados = Notas.FirstOrDefault(u => u.Id == idActualizadonota.Id);
			if (idbuscados != null)
			{
				idbuscados.Contenido = idActualizadonota.Contenido;
				idbuscados.Carpeta = idActualizadonota.Carpeta;

				SaveChanges();
			}
			else throw new Exception("la nota que intenta editar, no existe");
		}
		#endregion Notas


		#region Calendario
		// Método para mostrar todos los eventos en el calendario
		public List<Calendario> mostrarCalendario()
		{
			return Calendario.ToList();
		}

		// Método para agregar un nuevo evento al calendario
		public void agregarEvento(Calendario eventos)
		{
			Calendario.Add(eventos);
			SaveChanges();
		}

		// Método para editar un evento existente
		public void editarEvento(Calendario evento)
		{
			var eventoExistente = Calendario.FirstOrDefault(e => e.Id == evento.Id);
			if (eventoExistente != null)
			{
				eventoExistente.Titulo = evento.Titulo;
				eventoExistente.FechaInicio = evento.FechaInicio;
				eventoExistente.FechaFinal = evento.FechaFinal;
				eventoExistente.Descripcion = evento.Descripcion;
				SaveChanges();
			}
		}

		// Método para eliminar un evento del calendario
		public void eliminarEvento(int id)
		{
			var evento = Calendario.FirstOrDefault(e => e.Id == id);
			if (evento != null)
			{
				Calendario.Remove(evento);
				SaveChanges();
			}
		}
		#endregion Calendario
	}
}
