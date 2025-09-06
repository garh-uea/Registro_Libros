// Registro de libros en una biblioteca
// Programa que permite registrar, consultar y listar libros en una biblioteca,
// utilizando conjuntos (HashSet) y diccionarios para organizar la información.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RegistroBibliotecaApp
{
    // Clase que representa un libro en el catálogo
    public class Libro
    {
        public string Isbn { get; set; }      // Identificador único del libro
        public string Titulo { get; set; }    // Título del libro
        public HashSet<string> Autores { get; set; }  // Conjunto de autores (sin duplicados)
        public int Anio { get; set; }         // Año de publicación
        public HashSet<string> Categorias { get; set; } // Categorías del libro

        // Constructor para inicializar los datos del libro
        public Libro(string isbn, string titulo, IEnumerable<string> autores, int anio, IEnumerable<string> categorias)
        {
            Isbn = (isbn ?? "").Trim();
            Titulo = (titulo ?? "").Trim();
            Autores = new HashSet<string>((autores ?? Enumerable.Empty<string>()), StringComparer.OrdinalIgnoreCase);
            Anio = anio;
            Categorias = new HashSet<string>((categorias ?? Enumerable.Empty<string>()), StringComparer.OrdinalIgnoreCase);
        }

        // Método que devuelve una representación en texto del libro
        public override string ToString()
        {
            string autoresTxt = Autores.Count > 0 ? string.Join(", ", Autores) : "N/D";
            string catsTxt = Categorias.Count > 0 ? string.Join(", ", Categorias) : "N/D";
            return $"ISBN: {Isbn} | Título: {Titulo} | Autores: {autoresTxt} | Año: {Anio} | Categorías: {catsTxt}";
        }
    }

    // Clase que representa el catálogo de la biblioteca
    public class CatalogoBiblioteca
    {
        // Diccionario principal con ISBN como clave
        private Dictionary<string, Libro> librosPorIsbn;
        // Índice de autores -> conjunto de ISBNs
        private Dictionary<string, HashSet<string>> indicesPorAutor;
        // Índice de categorías -> conjunto de ISBNs
        private Dictionary<string, HashSet<string>> indicesPorCategoria;

        // Constructor: inicializa las estructuras de datos
        public CatalogoBiblioteca()
        {
            librosPorIsbn = new Dictionary<string, Libro>(StringComparer.OrdinalIgnoreCase);
            indicesPorAutor = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            indicesPorCategoria = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        }

        // Método para agregar un libro al catálogo
        public bool AgregarLibro(Libro libro)
        {
            if (libro == null || string.IsNullOrWhiteSpace(libro.Isbn)) return false;

            // Verificar que no se repita el ISBN
            if (librosPorIsbn.ContainsKey(libro.Isbn))
                return false;

            librosPorIsbn[libro.Isbn] = libro;

            // Agregar índices por autor
            foreach (var autor in libro.Autores)
            {
                if (!indicesPorAutor.ContainsKey(autor))
                    indicesPorAutor[autor] = new HashSet<string>();
                indicesPorAutor[autor].Add(libro.Isbn);
            }

            // Agregar índices por categoría
            foreach (var cat in libro.Categorias)
            {
                if (!indicesPorCategoria.ContainsKey(cat))
                    indicesPorCategoria[cat] = new HashSet<string>();
                indicesPorCategoria[cat].Add(libro.Isbn);
            }

            return true;
        }

        // Consultar un libro directamente por ISBN
        public Libro ConsultarPorIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return null;
            librosPorIsbn.TryGetValue(isbn, out var libro);
            return libro;
        }

        // Buscar libros cuyo título contenga un fragmento
        public List<Libro> BuscarPorTituloParcial(string fragmento)
        {
            if (string.IsNullOrWhiteSpace(fragmento)) return new List<Libro>();
            return librosPorIsbn.Values
                .Where(l => l.Titulo.IndexOf(fragmento, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // Listar libros de un autor específico
        public List<Libro> ListarPorAutor(string autor)
        {
            if (string.IsNullOrWhiteSpace(autor)) return new List<Libro>();
            if (!indicesPorAutor.TryGetValue(autor, out var isbns))
                return new List<Libro>();
            return isbns.Select(isbn => librosPorIsbn[isbn]).ToList();
        }

        // Listar libros de una categoría específica
        public List<Libro> ListarPorCategoria(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria)) return new List<Libro>();
            if (!indicesPorCategoria.TryGetValue(categoria, out var isbns))
                return new List<Libro>();
            return isbns.Select(isbn => librosPorIsbn[isbn]).ToList();
        }

        // Listar todo el catálogo
        public List<Libro> ListarCatalogo()
        {
            return librosPorIsbn.Values.ToList();
        }

        // Método para medir el tiempo de ejecución de una operación
        public (T resultado, long ms) MedirTiempo<T>(Func<T> accion)
        {
            var sw = Stopwatch.StartNew();
            var res = accion();
            sw.Stop();
            return (res, sw.ElapsedMilliseconds);
        }
    }

    // Clase que gestiona la interfaz
    public static class AplicacionConsola
    {
        private static CatalogoBiblioteca catalogo = new CatalogoBiblioteca();

        // Método principal de interacción con el usuario
        public static void Iniciar()
        {
            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                Console.Write("Seleccione una opción: ");
                string opcion = Console.ReadLine();
                Console.WriteLine();

                // Menú de opciones
                switch (opcion)
                {
                    case "1": OpcionAgregarLibro(); break;
                    case "2": OpcionConsultarPorIsbn(); break;
                    case "3": OpcionBuscarTituloParcial(); break;
                    case "4": OpcionListarPorAutor(); break;
                    case "5": OpcionListarPorCategoria(); break;
                    case "6": OpcionListarCatalogo(); break;
                    case "0":
                        salir = true;
                        Console.WriteLine("Saliendo del programa...");
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                Console.WriteLine();
            }
        }

        // Menú principal
        private static void MostrarMenu()
        {
            Console.WriteLine("========= BIBLIOTECA UEA ==========");            
            Console.WriteLine("===== MENÚ REGISTRO DE LIBROS =====");
            Console.WriteLine("1. Agregar libro");
            Console.WriteLine("2. Consultar por ISBN");
            Console.WriteLine("3. Buscar por título parcial");
            Console.WriteLine("4. Listar por autor");
            Console.WriteLine("5. Listar por categoría");
            Console.WriteLine("6. Listar catálogo completo");
            Console.WriteLine("0. Salir");
        }

        // Opción 1: Agregar libro al catálogo
        private static void OpcionAgregarLibro()
        {
            Console.Write("Ingrese ISBN: ");
            string isbn = Console.ReadLine().Trim();

            // Validar que no se repita el ISBN
            while (catalogo.ConsultarPorIsbn(isbn) != null)
            {
                Console.WriteLine("⚠️ ISBN ya existe, ingrese otro:");
                isbn = Console.ReadLine().Trim();
            }

            Console.Write("Ingrese título: ");
            string titulo = Console.ReadLine();

            Console.Write("Ingrese autores separados por ';': ");
            var autores = (Console.ReadLine() ?? "").Split(';').Select(a => a.Trim()).Where(a => a.Length > 0);

            Console.Write("Ingrese año: ");
            int.TryParse(Console.ReadLine(), out int anio);

            Console.Write("Ingrese categorías separadas por ';': ");
            var categorias = (Console.ReadLine() ?? "").Split(';').Select(c => c.Trim()).Where(c => c.Length > 0);

            // Crear libro y agregarlo
            var libro = new Libro(isbn, titulo, autores, anio, categorias);
            var (resultado, ms) = catalogo.MedirTiempo(() => catalogo.AgregarLibro(libro));

            Console.WriteLine(resultado ? $"✅ Libro agregado en {ms} ms" : "❌ No se pudo agregar el libro.");
        }

        // Opción 2: Consultar libro por ISBN
        private static void OpcionConsultarPorIsbn()
        {
            Console.Write("Ingrese ISBN: ");
            string isbn = Console.ReadLine();
            var (libro, ms) = catalogo.MedirTiempo(() => catalogo.ConsultarPorIsbn(isbn));
            Console.WriteLine(libro != null ? $"Encontrado en {ms} ms:\n{libro}" : "No se encontró el libro.");
        }

        // Opción 3: Buscar por título parcial
        private static void OpcionBuscarTituloParcial()
        {
            Console.Write("Ingrese fragmento del título: ");
            string frag = Console.ReadLine();
            var (resultados, ms) = catalogo.MedirTiempo(() => catalogo.BuscarPorTituloParcial(frag));
            Console.WriteLine($"Resultados ({resultados.Count}) en {ms} ms:");
            resultados.ForEach(l => Console.WriteLine(l));
        }

        // Opción 4: Listar libros por autor
        private static void OpcionListarPorAutor()
        {
            Console.Write("Ingrese autor: ");
            string autor = Console.ReadLine();
            var (resultados, ms) = catalogo.MedirTiempo(() => catalogo.ListarPorAutor(autor));
            Console.WriteLine($"Resultados ({resultados.Count}) en {ms} ms:");
            resultados.ForEach(l => Console.WriteLine(l));
        }

        // Opción 5: Listar libros por categoría
        private static void OpcionListarPorCategoria()
        {
            Console.Write("Ingrese categoría: ");
            string cat = Console.ReadLine();
            var (resultados, ms) = catalogo.MedirTiempo(() => catalogo.ListarPorCategoria(cat));
            Console.WriteLine($"Resultados ({resultados.Count}) en {ms} ms:");
            resultados.ForEach(l => Console.WriteLine(l));
        }

        // Opción 6: Listar catálogo completo
        private static void OpcionListarCatalogo()
        {
            var (resultados, ms) = catalogo.MedirTiempo(() => catalogo.ListarCatalogo());
            Console.WriteLine($"Catálogo total ({resultados.Count}) en {ms} ms:");
            resultados.ForEach(l => Console.WriteLine(l));
        }
    }

    // Clase principal con método Main
    class Program
    {
        static void Main(string[] args)
        {
            AplicacionConsola.Iniciar();
        }
    }
}
