using static GenerarTablero;

public class Trampa(Trampa.Tipo tipo)
{
    public enum Tipo
    {
        Dañominimo,
        Dañomaximo,
        AumentarCooldown, // Otro tipo de trampa
    }

    public Tipo TipoDeTrampa { get; set; } = tipo;

    public void AplicarTrampa(Ficha ficha)
    {
        switch (TipoDeTrampa)
        {
            case Tipo.Dañominimo:
                ficha.PerderPuntos(1); // Llama a PerderPuntos para restar 1 punto
                break;
            case Tipo.Dañomaximo:
                ficha.PerderPuntos(2); // Llama a PerderPuntos para restar 2 puntos
                break;
            case Tipo.AumentarCooldown:
                Console.WriteLine(
                    $"Trampa AumentarCooldown activada en {ficha.Nombre}. Cooldown antes: {ficha.Cooldown}"
                );
                ficha.AumentarCooldown(5);
                Console.WriteLine($"Cooldown después: {ficha.Cooldown}");
                break;
        }
    }

    public static void ManejarTrampa(
        int fila,
        int columna,
        Ficha ficha,
        Casilla[,] tablero,
        List<(int fila, int columna, Trampa.Tipo tipo)> trampas
    )
    {
        if (tablero[fila, columna] == Casilla.Trampa)
        {
            // Encuentra la trampa correspondiente
            var trampa = trampas.FirstOrDefault(t => t.fila == fila && t.columna == columna);

            if (trampa != default)
            {
                switch (trampa.tipo)
                {
                    case Trampa.Tipo.Dañominimo:
                        ficha.PerderPuntos(1);
                        Console.WriteLine(
                            $"Trampa activada,Ficha {ficha.Nombre} perdió 10% de vida ."
                        );
                        Thread.Sleep(3000); // Pausa de 3 segundos
                        break;

                    case Trampa.Tipo.Dañomaximo:
                        ficha.PerderPuntos(2);
                        Console.WriteLine(
                            $"Trampa activada,Ficha {ficha.Nombre} perdió 20% de vida ."
                        );
                        Thread.Sleep(3000); // Pausa de 3 segundos
                        break;

                    case Trampa.Tipo.AumentarCooldown:
                        ficha.AumentarCooldown(5);
                        Console.WriteLine(
                            $"Trampa AumentarCooldown activada en ({fila}, {columna}). Cooldown de {ficha.Nombre} aumentado en 5 turnos."
                        );
                        Thread.Sleep(3000); // Pausa de 3 segundos
                        break;

                    // Aquí puedes agregar más tipos de trampas si lo deseas
                }

                // Cambiar el estado de la casilla de Trampa a Camino
                tablero[fila, columna] = Casilla.Camino;

                // Eliminar la trampa de la lista
                trampas.Remove(trampa);
            }
        }
    }
}
