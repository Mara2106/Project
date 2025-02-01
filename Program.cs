using Spectre.Console;
using static GenerarTablero;
using Casilla = GenerarTablero.Casilla;

partial class Program
{
    public const int Width = 38;
    public const int Height = 18;
    public const int CantTrampas = 35;
    public const int CantParedes = 140;
    public const int CantObjetos = 8;

    public static List<Ficha> FichasElegidas = new();

    static void Main()
    {
        StartGame.MostrarPresentacion(); // Llamar a la presentación

        // Generar tablero y trampas
        var (tablero, trampas) = Generar(Width,Height , CantTrampas , CantParedes);
        Random random = new();

        // Limpiar la selección de fichas al inicio
        FichasElegidas.Clear();

        // Seleccionar fichas para los jugadores
        ElegirFichas(tablero, FichasElegidas);

        // Colocar objetos en el tablero
        ColocarObjetos(tablero, CantObjetos, random); // Coloca 6 objetos

        // Dibujar el tablero inicial
        TableroDrawer.DibujarTablero(tablero, FichasElegidas);

        // Inicializar información del juego
        Informacion.Inicializar();

        int turnoActual = 0;

        while (FichasElegidas.Count > 0)
        {
            Console.Clear();
            TableroDrawer.DibujarTablero(tablero, FichasElegidas);
            Informacion.MostrarInformacion(FichasElegidas);
            Console.WriteLine(
                $"\nTurno del Jugador {turnoActual + 1} ({FichasElegidas[turnoActual].Nombre})"
            );

            // Menú para seleccionar acción
            var accion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Elige tu acción:")
                    .AddChoices(new[] { "Mover Ficha", "Usar Habilidad" })
            );

            if (accion == "Mover Ficha")
            {
                Console.WriteLine("Usa las teclas de dirección para mover la ficha 2 veces.");
                ConsoleKey primeraTeclaPresionada = Console.ReadKey(true).Key;

                int nuevaFila = FichasElegidas[turnoActual].Fila;
                int nuevaColumna = FichasElegidas[turnoActual].Columna;
                int filaDelta = 0,
                    columnaDelta = 0;

                // Primer movimiento
                switch (primeraTeclaPresionada)
                {
                    case ConsoleKey.UpArrow:
                        filaDelta = -1;
                        break;
                    case ConsoleKey.DownArrow:
                        filaDelta = 1;
                        break;
                    case ConsoleKey.LeftArrow:
                        columnaDelta = -1;
                        break;
                    case ConsoleKey.RightArrow:
                        columnaDelta = 1;
                        break;
                    default:
                        Console.WriteLine("Tecla inválida. Usa las flechas de dirección.");
                        continue;
                }

                int primeraFila = nuevaFila + filaDelta;
                int primeraColumna = nuevaColumna + columnaDelta;

                if (
                    MoverFicha(
                        FichasElegidas[turnoActual],
                        primeraFila,
                        primeraColumna,
                        tablero,
                        trampas
                    )
                )
                {
                    nuevaFila = primeraFila;
                    nuevaColumna = primeraColumna;

                    TableroDrawer.DibujarTablero(tablero, FichasElegidas);

                    ConsoleKey segundaTeclaPresionada = Console.ReadKey(true).Key;

                    filaDelta = 0;
                    columnaDelta = 0;
    // Segundo movimiento
                    switch (segundaTeclaPresionada)
                    {
                        case ConsoleKey.UpArrow:
                            filaDelta = -1;
                            break;
                        case ConsoleKey.DownArrow:
                            filaDelta = 1;
                            break;
                        case ConsoleKey.LeftArrow:
                            columnaDelta = -1;
                            break;
                        case ConsoleKey.RightArrow:
                            columnaDelta = 1;
                            break;
                        default:
                            Console.WriteLine("Tecla inválida. Usa las flechas de dirección.");
                            continue;
                    }

                    int segundaFila = nuevaFila + filaDelta;
                    int segundaColumna = nuevaColumna + columnaDelta;

                    if (
                        MoverFicha(
                            FichasElegidas[turnoActual],
                            segundaFila,
                            segundaColumna,
                            tablero,
                            trampas
                        )
                    ) { }
                    else
                    {
                        Console.WriteLine("Movimiento no permitido en el segundo paso.");
                        Thread.Sleep(3000);
                    }
                }
                else
                {
                    Console.WriteLine("Movimiento no permitido en el primer paso.");
                    Thread.Sleep(1500);
                }

                TableroDrawer.DibujarTablero(tablero, FichasElegidas);
                FichasElegidas[turnoActual].FinalizarTurno();
                turnoActual = (turnoActual + 1) % FichasElegidas.Count; // Avanzar al siguiente turno
            }
            else if (accion == "Usar Habilidad")
            {
                FichasElegidas[turnoActual]
                    .UsarHabilidad(tablero, random, FichasElegidas);
            }

            // Verificar si la ficha perdió todos los puntos
            if (FichasElegidas[turnoActual].Puntos == 0)
            {
                Console.WriteLine(
                    $"{FichasElegidas[turnoActual].Nombre} ha perdido todos sus puntos. ¡El juego ha terminado!"
                );
                break; // Sale del bucle y termina el juego
            }

            // Verificar si la ficha perdió todos los puntos
            if (!VerificarPuntos(FichasElegidas[turnoActual]))
            {
                FichasElegidas.RemoveAt(turnoActual);
                if (turnoActual >= FichasElegidas.Count)
                    turnoActual = 0; // Reiniciar el turno si excede el número de fichas
            }

            foreach (var ficha in FichasElegidas)
            {
                if (ficha.TurnosInmunidad > 0)
                {
                    ficha.TurnosInmunidad--;
                }
            }
        }
    }

    public static void ElegirFichas(Casilla[,] tablero, List<Ficha> fichasSeleccionadas)
    {
        var random = new Random();

        for (int i = 1; i <= 2; i++)
        {
            // Crear el prompt con detalles adicionales sobre cada ficha
            var fichaElegida = AnsiConsole.Prompt(
                new SelectionPrompt<Ficha>()
                    .Title($"Jugador {i}, elija su ficha:")
                    .UseConverter(ficha =>
                        $"{ficha.Nombre} - Habilidad: {ficha.Habilidad} - Cooldown: {ficha.Cooldown} turnos"
                    )
                    .AddChoices(Ficha.fichasDisponibles)
            );

            // Remover la ficha elegida de la lista de disponibles
            Ficha.fichasDisponibles.Remove(fichaElegida);

            // Obtener una posición aleatoria para colocar la ficha en el tablero
            var posicion = ObtenerPosicionAleatoria(tablero, random);
            fichaElegida.Fila = posicion.Item1;
            fichaElegida.Columna = posicion.Item2;

            // Agregar la ficha elegida a la lista de fichas seleccionadas
            fichasSeleccionadas.Add(fichaElegida);

            // Mostrar un mensaje con la información de la ficha seleccionada
            AnsiConsole.MarkupLine(
                $"[bold]{fichaElegida.Nombre}[/] con la habilidad [italic]{fichaElegida.Habilidad}[/] fue seleccionada por Jugador {i} y colocada en ({posicion.Item1}, {posicion.Item2})."
            );
        }
    }

    static bool MoverFicha(
        Ficha ficha,
        int nuevaFila,
        int nuevaColumna,
        Casilla[,] tablero,
        List<(int fila, int columna, Trampa.Tipo tipo)> trampas
    )
    {
        // Verificar si el movimiento es válido
        if (EsMovimientoValido(tablero, nuevaFila, nuevaColumna, ficha))
        {
            // Mover la ficha a la nueva posición
            ficha.Mover(nuevaFila, nuevaColumna, tablero);

            // Manejar trampas en la nueva posición
            Trampa.ManejarTrampa(nuevaFila, nuevaColumna, ficha, tablero, trampas);

            // Verificar si hay un objeto en la nueva posición
            if (tablero[nuevaFila, nuevaColumna] == Casilla.Objeto)
            {
                ficha.RecogerObjeto(); // Llama al método para recoger el objeto
                // Aquí puedes marcar la casilla como vacía o eliminar el objeto del tablero
                tablero[nuevaFila, nuevaColumna] = Casilla.Camino; // Cambia a casilla vacía
            }

            // Verificar los puntos de la ficha después de mover
            if (!VerificarPuntos(ficha))
            {
                return false;
            }

            // Actualizar el tablero y notificar el movimiento
            ActualizarTableroYNotificar(ficha, tablero, nuevaFila, nuevaColumna);

            // Reducir el contador de Paso Fantasma si está activo
            if (ficha.turnosPasoFantasma > 0)
            {
                ficha.turnosPasoFantasma--;
                if (ficha.turnosPasoFantasma == 0)
                {
                    Console.WriteLine($"{ficha.Nombre} ya no puede atravesar obstáculos.");
                }
            }
            return true;
        }
        else
        {
            Console.WriteLine("Movimiento inválido. Esa casilla no es transitable.");
            return false;
        }
    }

    static bool VerificarPuntos(Ficha ficha)
    {
        if (ficha.Puntos <= 0)
        {
            Console.WriteLine(
                $"{ficha.Nombre} ha perdido todos sus puntos. ¡El juego termina para esta ficha!"
            );
            return false;
        }
        return true;
    }

    static bool EsMovimientoValido(Casilla[,] tablero, int fila, int columna, Ficha ficha)
    {
        // Verificar si estamos en el borde del tablero
        bool esBorde =
            fila == 0
            || fila == tablero.GetLength(0) - 1
            || columna == 0
            || columna == tablero.GetLength(1) - 1;

        // Si la habilidad "Paso Fantasma" está activa, se puede atravesar los obstáculos
        if (ficha.turnosPasoFantasma > 0)
        {
            if (esBorde && tablero[fila, columna] == Casilla.Pared)
            {
                Console.WriteLine($"{ficha.Nombre} no puede atravesar un obstáculo en el borde.");
                return false;
            }
            if (tablero[fila, columna] == Casilla.Pared)
            {
                Console.WriteLine(
                    $"{ficha.Nombre} puede atravesar el obstáculo con Paso Fantasma."
                );
            }
            // No bloquear las trampas, ya que la ficha puede pasar por ellas igualmente
            return true; // Permite el movimiento en cualquier casilla (Obstáculo o Trampa)
        }

        // Si la casilla es un obstáculo, y la habilidad no está activa, el movimiento no es válido
        if (tablero[fila, columna] == Casilla.Pared)
        {
            Console.WriteLine($"{ficha.Nombre} no puede atravesar el obstáculo.");
            return false;
        }

        // Si la casilla es un camino, trampa u objeto, el movimiento es válido
        return tablero[fila, columna] == Casilla.Camino
            || tablero[fila, columna] == Casilla.Trampa
            || tablero[fila, columna] == Casilla.Objeto;
    }

    static void ActualizarTableroYNotificar(Ficha ficha, Casilla[,] tablero, int fila, int columna)
    {
        TableroDrawer.DibujarTablero(tablero, FichasElegidas);
    }
}
