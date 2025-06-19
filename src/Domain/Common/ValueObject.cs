namespace GameServer.Domain.Common;

/// <summary>
/// Base para Value Objects usando C# 9+ records
/// </summary>
public abstract record ValueObject
{
    // Records já implementam Equals/GetHashCode e os operadores ==/!= com base nas propriedades declaradas.
    // Basta herdar e declarar as propriedades no cabeçalho do record derivado.

    // Pode-se opcionalmente sobrescrever ToString para debug:
    public override string ToString()
        => GetType().Name + " [" + string.Join(", ", GetEqualityComponents()) + "]";

    /// <summary>
    /// Componentes que definem a igualdade do VO (propriedades posicionais do record derivado)
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();
}
